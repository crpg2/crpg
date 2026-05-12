using System.Xml.Linq;
using Crpg.Module.Common.Network;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ModuleManager;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common;

internal class CrpgAnimationBehavior : MissionNetwork
{
    private record struct AnimInfo(ActionIndexCache ActionIndex, MBActionSet ActionSet, string DisplayName, int Channel, bool MovementCancels, bool MountRequired, bool OnlyOnFoot);

    private readonly Dictionary<string, AnimInfo> _anims = [];

    // Client-only: true while a movement_cancels animation is playing for the local agent.
    private bool _movementCancelsActive;

#if CRPG_SERVER
    internal bool IsEnabled { get; private set; } = true;
#elif CRPG_CLIENT
    internal bool IsEnabled { get; private set; } = true;
#else
    internal bool IsEnabled { get; private set; } = false;
#endif

    public override void OnBehaviorInitialize()
    {
        base.OnBehaviorInitialize();
#if CRPG_SERVER
        IsEnabled = CrpgServerConfiguration.IsAnimationsEnabled;
#endif
        BuildAnimationsCache();
    }

    // Client-only tick: send act_none when the player tries to move during a cancellable animation.
    public override void OnMissionTick(float dt)
    {
        base.OnMissionTick(dt);

        if (!GameNetwork.IsClient || !_movementCancelsActive)
        {
            return;
        }

        var agent = Mission.Current?.MainAgent;
        if (agent == null || !agent.IsActive())
        {
            _movementCancelsActive = false;
            return;
        }

        if (agent.MovementInputVector.LengthSquared > 0f)
        {
            _movementCancelsActive = false;
            GameNetwork.BeginModuleEventAsClient();
            GameNetwork.WriteMessage(new CrpgRequestPlayAnimation { ActionId = "act_none" });
            GameNetwork.EndModuleEventAsClient();
        }
    }

    public void RequestPlayAnimation(string actionId)
    {
        if (!GameNetwork.IsClient)
        {
            return;
        }

        GameNetwork.BeginModuleEventAsClient();
        GameNetwork.WriteMessage(new CrpgRequestPlayAnimation { ActionId = actionId });
        GameNetwork.EndModuleEventAsClient();
    }

    protected override void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegistererContainer registerer)
    {
        if (GameNetwork.IsServer)
        {
            registerer.Register<CrpgRequestPlayAnimation>(HandleClientRequestPlayAnimation);
        }

        if (GameNetwork.IsClient)
        {
            registerer.Register<CrpgBroadcastPlayAnimation>(HandleBroadcastPlayAnimation);
            registerer.Register<CrpgServerSendAnimationsEnabled>(HandleServerSendAnimationsEnabled);
        }
    }

#if CRPG_SERVER
    protected override void HandleEarlyNewClientAfterLoadingFinished(NetworkCommunicator peer)
    {
        base.HandleEarlyNewClientAfterLoadingFinished(peer);

        if (peer == null || !peer.IsConnectionActive)
        {
            return;
        }

        GameNetwork.BeginModuleEventAsServer(peer);
        GameNetwork.WriteMessage(new CrpgServerSendAnimationsEnabled { IsEnabled = IsEnabled });
        GameNetwork.EndModuleEventAsServer();
    }
#endif

    private void HandleServerSendAnimationsEnabled(CrpgServerSendAnimationsEnabled message)
    {
        if (GameNetwork.IsClient)
        {
            IsEnabled = message.IsEnabled;
        }

        return;
    }

    private bool HandleClientRequestPlayAnimation(NetworkCommunicator peer, CrpgRequestPlayAnimation message)
    {
        var agent = peer.ControlledAgent;
        if (agent == null || !agent.IsActive())
        {
            return true;
        }

        if (message.ActionId != "act_none")
        {
            if (!IsEnabled)
            {
                return true;
            }

            if (!_anims.TryGetValue(message.ActionId, out AnimInfo requested))
            {
                return true;
            }

            if (requested.MountRequired && agent.MountAgent == null)
            {
                return true;
            }

            if (requested.OnlyOnFoot && agent.MountAgent != null)
            {
                return true;
            }
        }

        GameNetwork.BeginBroadcastModuleEvent();
        GameNetwork.WriteMessage(new CrpgBroadcastPlayAnimation
        {
            AgentIndex = agent.Index,
            ActionId = message.ActionId,
        });
        GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
        return true;
    }

    private void HandleBroadcastPlayAnimation(CrpgBroadcastPlayAnimation message)
    {
        var agent = Mission.Current?.FindAgentWithIndex(message.AgentIndex);
        if (agent == null || !agent.IsActive())
        {
            return;
        }

        if (message.ActionId == "act_none")
        {
            agent.SetActionChannel(0, ActionIndexCache.act_none, ignorePriority: true);
            agent.SetActionChannel(1, ActionIndexCache.act_none, ignorePriority: true);
            return;
        }

        if (!_anims.TryGetValue(message.ActionId, out AnimInfo anim))
        {
            return;
        }

        PlayAnimation(agent, anim);

        if (agent == Mission.Current?.MainAgent)
        {
            _movementCancelsActive = anim.MovementCancels;
        }
    }

    private void PlayAnimation(Agent agent, AnimInfo anim)
    {
        InformationManager.DisplayMessage(new InformationMessage($"Playing: {new TextObject(anim.DisplayName)} ({anim.ActionSet.GetAnimationName(anim.ActionIndex)}) ch{anim.Channel}"));
        if (!agent.IsOnLand())
        {
            return;
        }

        AnimationSystemData asd = agent.Monster.FillAnimationSystemData(anim.ActionSet, agent.Character.GetStepSize(), agent.IsFemale);
        agent.SetActionSet(ref asd);
        agent.SetActionChannel(anim.Channel, anim.ActionIndex, true, 0UL, 0.0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
    }

    private void BuildAnimationsCache()
    {
        string path = ModuleHelper.GetXmlPath("cRPG", "crpg_menus/crpg_animation_menu");
        var doc = XDocument.Load(path);
        if (doc.Root is null)
        {
            return;
        }

        var items = doc.Root
            .Elements("menu_category")
            .SelectMany(c => c.Elements("menu_subcategory")
                .SelectMany(s => s.Elements("menu_item"))
                .Concat(c.Elements("menu_item")))
            .Select(e => (
                ActionId: (string?)e.Attribute("action_type") ?? string.Empty,
                DisplayName: (string?)e.Attribute("display_name") ?? string.Empty,
                Channel: (int?)e.Attribute("action_channel") ?? 1,
                MovementCancels: (bool?)e.Attribute("movement_cancels") ?? false,
                MountRequired: (bool?)e.Attribute("mount_required") ?? false,
                OnlyOnFoot: (bool?)e.Attribute("only_on_foot") ?? false))
            .Where(e => !string.IsNullOrEmpty(e.ActionId))
            .ToList();

        int actionSetCount = MBActionSet.GetNumberOfActionSets();
        foreach (var (actionId, displayName, channel, movementCancels, mountRequired, onlyOnFoot) in items)
        {
            ActionIndexCache actionIndex = ActionIndexCache.Create(actionId);
            for (int i = 0; i < actionSetCount; i++)
            {
                MBActionSet actionSet = MBActionSet.GetActionSetWithIndex(i);
                if (MBActionSet.CheckActionAnimationClipExists(actionSet, actionIndex))
                {
                    _anims[actionId] = new AnimInfo(actionIndex, actionSet, displayName, channel, movementCancels, mountRequired, onlyOnFoot);
                    break;
                }
            }
        }

        var missing = items.Where(e => !_anims.ContainsKey(e.ActionId)).Select(e => e.ActionId).ToList();
        string message = missing.Count > 0
            ? $"Animations cached {_anims.Count}/{items.Count}. Missing: {string.Join(", ", missing)}"
            : $"Animations cached {_anims.Count}/{items.Count}.";
        // InformationManager.DisplayMessage(new InformationMessage(message));
        Debug.Print(message, 0, Debug.DebugColor.Cyan);
    }
}

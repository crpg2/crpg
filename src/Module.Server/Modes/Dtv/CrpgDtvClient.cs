using NetworkMessages.FromServer;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Modes.Dtv;

internal class CrpgDtvClient : MissionMultiplayerGameModeBaseClient
{
    private int _currentWave;
    private int _currentRound;
    public event Action? OnUpdateCurrentProgress;
    public event Action? OnWaveStart;
    public event Action? OnRoundStart;

    public Agent? VipAgent { get; set; }

    public int CurrentRound
    {
        get => _currentRound;
        set
        {
            if (value != _currentRound)
            {
                _currentRound = value;
            }
        }
    }

    public int CurrentWave
    {
        get => _currentWave;
        set
        {
            if (value != _currentWave)
            {
                _currentWave = value;
            }
        }
    }

    public override bool IsGameModeUsingGold => false;
    public override bool IsGameModeTactical => false;
    public override bool IsGameModeUsingRoundCountdown => true;
    public override MultiplayerGameType GameType =>
        MultiplayerGameType.Battle;
    public override bool IsGameModeUsingCasualGold => false;

    public override void OnGoldAmountChangedForRepresentative(MissionRepresentativeBase representative, int goldAmount)
    {
    }

    public override int GetGoldAmount()
    {
        return 0;
    }

    public override void AfterStart()
    {
        Mission.Current.SetMissionMode(MissionMode.Battle, true);
    }

    protected override void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegistererContainer registerer)
    {
        base.AddRemoveMessageHandlers(registerer);
        registerer.Register<CrpgDtvSetTimerMessage>(HandleSetTimer);
        registerer.Register<CrpgDtvRoundStartMessage>(HandleRoundStart);
        registerer.Register<CrpgDtvWaveStartMessage>(HandleWaveStart);
        registerer.Register<CrpgDtvVipUnderAttackMessage>(HandleVipUnderAttack);
        registerer.Register<CrpgDtvGameEnd>(HandleVipDeath);
        registerer.Register<CrpgDtvVipSpawn>(HandleVipSpawn);
        registerer.Register<CrpgDtvCurrentProgressMessage>(HandleCurrentProgress);
        registerer.Register<SetStonePileAmmo>(HandleServerEventSetStonePileAmmo);
        registerer.Register<SetRangedSiegeWeaponAmmo>(HandleServerSetRangedSiegeWeaponAmmo);
    }

    private void HandleSetTimer(CrpgDtvSetTimerMessage message)
    {
        TimerComponent.StartTimerAsClient(message.StartTime, message.Duration);
    }

    private void HandleCurrentProgress(CrpgDtvCurrentProgressMessage message)
    {
        CurrentRound = message.Round + 1;
        CurrentWave = message.Wave + 1;

        OnUpdateCurrentProgress?.Invoke();
    }

    private void HandleRoundStart(CrpgDtvRoundStartMessage message)
    {
        TextObject textObject = new("{=bbg3UpPX}Round {ROUND} starting...",
            new Dictionary<string, object> { ["ROUND"] = message.Round + 1 });
        InformationManager.DisplayMessage(new InformationMessage
        {
            Information = textObject.ToString(),
            Color = new Color(0.48f, 0f, 1f),
            SoundEventPath = message.Round == 0 ? null : "event:/ui/notification/quest_finished",
        });
        CurrentRound = message.Round + 1;
        CurrentWave = 0;

        Action? onRoundStartEvent = OnRoundStart;
        if (onRoundStartEvent == null)
        {
            return;
        }

        onRoundStartEvent();
    }

    private void HandleWaveStart(CrpgDtvWaveStartMessage message)
    {
        TextObject textObject = new("{=1y04FNHB}Wave {WAVE} started!",
            new Dictionary<string, object> { ["WAVE"] = message.Wave + 1 });
        InformationManager.DisplayMessage(new InformationMessage
        {
            Information = textObject.ToString(),
            Color = new Color(218, 112, 214),
            SoundEventPath = message.Wave == 0 ? null : "event:/ui/notification/quest_update",
        });
        CurrentWave = message.Wave + 1;

        OnWaveStart?.Invoke();
    }

    private void HandleVipSpawn(CrpgDtvVipSpawn message)
    {
        VipAgent = Mission.MissionNetworkHelper.GetAgentFromIndex(message.VipAgentIndex, true);
    }

    private void HandleVipDeath(CrpgDtvGameEnd message)
    {
        var agentToDefend = Mission.MissionNetworkHelper.GetAgentFromIndex(message.VipAgentIndex, true);

        InformationManager.DisplayMessage(new InformationMessage
        {
            Information = message.VipDead
                ? new TextObject("{=4HrC30kl}{VIP} has been slaughtered!",
                new Dictionary<string, object> { ["VIP"] = agentToDefend.Name ?? "{=}The Viscount" }).ToString()
                : new TextObject("{=tdfOMWmf}The defenders have been slaughtered!").ToString(),
            Color = new Color(0.90f, 0.25f, 0.25f),
            SoundEventPath = "event:/ui/notification/death",
        });
    }

    private void HandleVipUnderAttack(CrpgDtvVipUnderAttackMessage message)
    {
        var attackerAgent = Mission.MissionNetworkHelper.GetAgentFromIndex(message.AgentAttackerIndex, true);
        var agentToDefend = Mission.MissionNetworkHelper.GetAgentFromIndex(message.AgentVictimIndex, true);

        if (attackerAgent == null)
        {
            Debug.Print($"CRPGLOG : HandleVipUnderAttack received a null agent {message.AgentAttackerIndex}");
            return;
        }

        TextObject textObject = new("{=mfD3LkeQ}{VIP} is being attacked by {AGENT}!",
        new Dictionary<string, object> { ["VIP"] = agentToDefend?.Name ?? "{=}The Viscount", ["AGENT"] = attackerAgent.Name ?? string.Empty });
        InformationManager.DisplayMessage(new InformationMessage
        {
            Information = textObject.ToString(),
            Color = new Color(0.90f, 0.25f, 0.25f),
            SoundEventPath = "event:/ui/notification/alert",
        });
    }

    private void HandleServerEventSetStonePileAmmo(SetStonePileAmmo message)
    {
        if (message.AmmoCount > 0)
        {
            StonePile? stonePile = Mission.MissionNetworkHelper.GetMissionObjectFromMissionObjectId(message.StonePileId) as StonePile;
            stonePile?.Activate();
        }
    }

    private void HandleServerSetRangedSiegeWeaponAmmo(SetRangedSiegeWeaponAmmo message)
    {
        if (message.AmmoCount > 0)
        {
            RangedSiegeWeapon? rangedSiegeWeapon = Mission.MissionNetworkHelper.GetMissionObjectFromMissionObjectId(message.RangedSiegeWeaponId) as RangedSiegeWeapon;
            rangedSiegeWeapon?.Activate();
        }
    }
}

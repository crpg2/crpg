﻿using Crpg.Module.Api.Models.Clans;
using Crpg.Module.Common.Network;
using TaleWorlds.MountAndBlade;
using TaleWorlds.PlatformService;

namespace Crpg.Module.Common;

/// <summary>
/// Used to synchronize user data between client / server.
/// </summary>
internal class CrpgUserManagerClient : MissionNetwork
{
    private MissionNetworkComponent? _missionNetworkComponent;
    private CrpgPeer? _crpgPeer;

    public override void OnBehaviorInitialize()
    {
        base.OnBehaviorInitialize();
        _missionNetworkComponent = Mission.GetMissionBehavior<MissionNetworkComponent>();
        _missionNetworkComponent.OnMyClientSynchronized += OnMyClientSynchronized;
    }

    public override void OnRemoveBehavior()
    {
        base.OnRemoveBehavior();
        _missionNetworkComponent!.OnMyClientSynchronized -= OnMyClientSynchronized;
    }

    protected override void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegistererContainer registerer)
    {
        base.AddRemoveMessageHandlers(registerer);
        registerer.Register<UpdateCrpgUser>(HandleUpdateCrpgUser);
        registerer.Register<UpdateCrpgUserClanInfo>(HandleUpdateCrpgUserClanInfo);
        registerer.Register<UpdateRewardMultiplier>(HandleUpdateRewardMultiplier);
    }

    private void HandleUpdateRewardMultiplier(UpdateRewardMultiplier message)
    {
        if (_crpgPeer == null)
        {
            return;
        }

        _crpgPeer.RewardMultiplier = message.RewardMultiplier;
    }

    private void HandleUpdateCrpgUser(UpdateCrpgUser message)
    {
        if (message.Peer == null)
        {
            return;
        }

        // If the user has no CrpgPeer -> add one
        CrpgPeer? crpgPeer = message.Peer.GetComponent<CrpgPeer>();
        if (crpgPeer == null)
        {
            crpgPeer = message.Peer.AddComponent<CrpgPeer>();
        }

        crpgPeer.User = message.User;
    }

    private void HandleUpdateCrpgUserClanInfo(UpdateCrpgUserClanInfo message)
    {
        if (message.Peer == null)
        {
            return;
        }

        // If the user has no CrpgPeer -> add one
        CrpgPeer? crpgPeer = message.Peer.GetComponent<CrpgPeer>();
        if (crpgPeer == null)
        {
            crpgPeer = message.Peer.AddComponent<CrpgPeer>();
        }

        if (crpgPeer.User == null)
        {
            return;
        }

        crpgPeer.Clan = message.Clan;
    }

    private void OnMyClientSynchronized()
    {
        _crpgPeer = GameNetwork.MyPeer.GetComponent<CrpgPeer>();

        if (PlatformServices.ProviderName == "GDK")
        {
            PlatformServices.Instance.GetPlatformId(GameNetwork.MyPeer.VirtualPlayer.Id, static xboxId =>
            {
                GameNetwork.BeginModuleEventAsClient();
                GameNetwork.WriteMessage(new XboxIdMessage { XboxId = xboxId.ToString()! });
                GameNetwork.EndModuleEventAsClient();
            });
        }
    }
}

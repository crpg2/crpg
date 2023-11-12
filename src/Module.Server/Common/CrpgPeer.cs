﻿using Crpg.Module.Api.Models.Clans;
using Crpg.Module.Api.Models.Users;
using Crpg.Module.Common.Network;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common;

internal class CrpgPeer : PeerComponent
{
    private CrpgUser? _user;
    private int _rewardMultiplier;
    private CrpgClan? _clan;
    public CrpgClan? Clan
    {
        get => _clan;
        set
        {
            _clan = value ?? throw new ArgumentNullException();
            SynchronizeUserToEveryone(); // Synchronize the property with the client.
        }
    }

    public SpawnInfo? LastSpawnInfo { get; set; }

    public CrpgUser? User
    {
        get => _user;
        set
        {
            _user = value ?? throw new ArgumentNullException();
            SynchronizeUserToEveryone(); // Synchronize the property with the client.
        }
    }

    /// <summary>
    /// True if the <see cref="User"/> is currently being fetched from the cRPG backend. <see cref="User"/> can still
    /// be non-null but it won't be the freshest data.
    /// </summary>
    public bool UserLoading { get; set; }

    public int RewardMultiplier
    {
        get => _rewardMultiplier;
        set
        {
            _rewardMultiplier = value;
            if (GameNetwork.IsServerOrRecorder)
            {
                GameNetwork.BeginModuleEventAsServer(Peer);
                GameNetwork.WriteMessage(new UpdateRewardMultiplier { RewardMultiplier = _rewardMultiplier });
                GameNetwork.EndModuleEventAsServer();
            }
        }
    }

    public void SynchronizeUserToPeer(NetworkCommunicator networkPeer)
    {
        if (User == null)
        {
            return;
        }

        GameNetwork.BeginModuleEventAsServer(networkPeer);
        GameNetwork.WriteMessage(new UpdateCrpgUser { Peer = Peer, User = User, ClanName = Clan?.Name ?? string.Empty, ClanTag = Clan?.Tag ?? string.Empty, BannerKey = Clan?.BannerKey ?? string.Empty, PrimaryColor = Clan?.PrimaryColor ?? 0, SecondaryColor = Clan?.SecondaryColor ?? 0 });
        GameNetwork.EndModuleEventAsServer();
    }

    private void SynchronizeUserToEveryone()
    {
        if (_user == null || !GameNetwork.IsServerOrRecorder)
        {
            return;
        }

        GameNetwork.BeginBroadcastModuleEvent();
        GameNetwork.WriteMessage(new UpdateCrpgUser { Peer = Peer, User = _user, ClanName = Clan?.Name ?? string.Empty, ClanTag = Clan?.Tag ?? string.Empty, BannerKey = Clan?.BannerKey ?? string.Empty, PrimaryColor = Clan?.PrimaryColor ?? 0, SecondaryColor = Clan?.SecondaryColor ?? 0 });
        GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
    }
}

using Crpg.Module.Api.Models.Clans;
using Crpg.Module.Api.Models.Users;
using Crpg.Module.Common.Network;
using TaleWorlds.Core;
using TaleWorlds.Library;
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
            _clan = value;
            SynchronizeClanToEveryone(); // Synchronize the property with the client.
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
        GameNetwork.WriteMessage(new UpdateCrpgUser { Peer = Peer, User = User });
        GameNetwork.EndModuleEventAsServer();
    }

    public void SynchronizeClanToPeer(NetworkCommunicator networkPeer)
    {
        if (User == null || Clan == null)
        {
            return;
        }

        GameNetwork.BeginModuleEventAsServer(networkPeer);
        GameNetwork.WriteMessage(new UpdateCrpgUserClanInfo { Peer = Peer, Clan = Clan });
        GameNetwork.EndModuleEventAsServer();
    }

    public void SynchronizeUserItemsToPeer(NetworkCommunicator networkPeer)
    {
        try
        {
            Debug.Print("SynchronizeUserItemsToPeer()");

            if (User == null || User.Items == null)
            {
                Debug.Print("User or User.Items is null");
                return;
            }

            Debug.Print($"Sending {User.Items.Count} items to peer {networkPeer.Index}");

            GameNetwork.BeginModuleEventAsServer(networkPeer);
            GameNetwork.WriteMessage(new UpdateCrpgUserItems { Peer = Peer, Items = User.Items });
            GameNetwork.EndModuleEventAsServer();
        }
        catch (Exception ex)
        {
            Debug.Print($"SynchronizeUserItemsToPeer() exception: {ex}");
            Console.WriteLine($"SynchronizeUserItemsToPeer() exception: {ex}");
        }
    }

    private void SynchronizeUserToEveryone()
    {
        if (_user == null || !GameNetwork.IsServerOrRecorder)
        {
            return;
        }

        GameNetwork.BeginBroadcastModuleEvent();
        GameNetwork.WriteMessage(new UpdateCrpgUser { Peer = Peer, User = _user });
        GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
    }

    private void SynchronizeClanToEveryone()
    {
        if (_user == null || !GameNetwork.IsServerOrRecorder || _clan == null)
        {
            return;
        }

        GameNetwork.BeginBroadcastModuleEvent();
        GameNetwork.WriteMessage(new UpdateCrpgUserClanInfo { Peer = Peer, Clan = _clan });
        GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
    }
}

using Crpg.Module.Modes.Dtv;
using NetworkMessages.FromServer;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Modes.Strategus;

internal class CrpgStrategusClient : MissionMultiplayerGameModeBaseClient
{
    private int _attackerTicketCount;
    private int _defenderTicketCount;

    public event Action OnUpdateTicketCount = default!;

    public int AttackerTicketCount
    {
        get => _attackerTicketCount;
        set
        {
            if (value != _attackerTicketCount)
            {
                _attackerTicketCount = value;
            }
        }
    }

    public int DefenderTicketCount
    {
        get => _defenderTicketCount;
        set
        {
            if (value != _defenderTicketCount)
            {
                _defenderTicketCount = value;
            }
        }
    }

    public override bool IsGameModeUsingGold => false;
    public override bool IsGameModeTactical => false;
    public override bool IsGameModeUsingRoundCountdown => true;
    public override MultiplayerGameType GameType =>
        MultiplayerGameType.Battle;
    public override bool IsGameModeUsingCasualGold => false;

    public override int GetGoldAmount() => 0;

    public override void OnGoldAmountChangedForRepresentative(MissionRepresentativeBase representative, int goldAmount)
    {
    }

    public override void AfterStart()
    {
        Mission.Current.SetMissionMode(MissionMode.Battle, true);
    }

    protected override void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegistererContainer registerer)
    {
        base.AddRemoveMessageHandlers(registerer);
        registerer.Register<CrpgStrategusTicketCountUpdateMessage>(HandleTicketCountUpdate);
    }

    private void HandleTicketCountUpdate(CrpgStrategusTicketCountUpdateMessage message)
    {

        AttackerTicketCount = message.AttackerTickets;
        DefenderTicketCount = message.DefenderTickets;
        OnUpdateTicketCount?.Invoke();
    }
}

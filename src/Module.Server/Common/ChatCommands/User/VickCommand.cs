using Mono.Cecil;
using NetworkMessages.FromServer;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace Crpg.Module.Common.ChatCommands.User;

internal class VickCommand : ChatCommand
{
    private readonly string[,] weaponSetsArray =
    {
        { "crpg_disabled_jamescross_v1_h3", "crpg_noble_ranger_bow_v1_h3", "crpg_barbed_arrows_v1_h3", "crpg_imperial_arrows_v2_h0" },
        { "crpg_disabled_jamescross_v1_h3", "crpg_crossbow_a_v4_h1", "crpg_bolt_g_v4_h0", "crpg_bolt_f_v4_h1" },
        { "crpg_disabled_jamescross_v1_h3", "crpg_handgonne_h3", "crpg_cla_musket_ammo_h1", "crpg_cla_musket_ammo_h3" },
        { "crpg_bolt_g_v4_h1", "crpg_crossbow_a_v4_h1", "crpg_bolt_g_v4_h0", "crpg_bolt_g_v4_h3" },
        { "crpg_hassun_yumi_v1_h3", "crpg_bodkin_arrows_v3_h3", "crpg_elitesteppe_arrows_v2_h3", "crpg_imperial_arrows_v2_h3" },
        { "crpg_throwing_heavy_stone_v3_h3", "crpg_fish_harpoon_v3_h3", "crpg_throwing_hammers_v3_h2", "crpg_jereed_v3_h3" },
        { "crpg_simple_javelin_v4_h3", "crpg_noble_ranger_bow_v1_h3", "crpg_imperial_arrows_v2_h0", "crpg_imperial_arrows_v2_h3" },
        { "crpg_hassun_yumi_v1_h3", "crpg_bodkin_arrows_v3_h3", "crpg_bolt_g_v4_h1", "crpg_imperial_arrows_v2_h3" },
    };
    private readonly string[] weaponSetNames =
    {
        "bow",
        "xbow",
        "gun",
        "xbow2",
        "bow2",
        "throw",
        "throwmix",
        "bowxbow",
    };
    public VickCommand(ChatCommandsComponent chatComponent)
        : base(chatComponent)
    {
        Name = "vick";
        string listarray = string.Join(" ", weaponSetNames);
        Description = $"'{ChatCommandsComponent.CommandPrefix}{Name} equipmentset' ( " + listarray + ")";
        Overloads = new CommandOverload[]
        {
            new(new[] { ChatCommandParameterType.String }, ExecuteSuccess),
        };
    }

    private void ExecuteSuccess(NetworkCommunicator fromPeer, object[] arguments)
    {
        string message = (string)arguments[0];
        int index = Array.IndexOf(weaponSetNames, message);

        if (index >= 0)
        {
            string outmessage = "Equipment Changed: " + message + " index: " + index;
            ChatComponent.ServerSendMessageToPlayer(fromPeer, ColorSuccess, outmessage);
        }
        else
        {
            string outmessage = "Equipment set not found: " + message;
            ChatComponent.ServerSendMessageToPlayer(fromPeer, ColorFatal, outmessage);
            index = 0;
            outmessage = "Using Default Set.";
            ChatComponent.ServerSendMessageToPlayer(fromPeer, ColorSuccess, outmessage);
        }

        EquipWeaponsToPlayer(fromPeer, index);

        Agent agent = fromPeer.ControlledAgent;
    }

    private void EquipWeaponsToPlayer(NetworkCommunicator fromPeer, int index)
    {
        if (index < 0 || index > weaponSetsArray.GetLength(0))
        {
            ChatComponent.ServerSendMessageToPlayer(fromPeer, ColorFatal, "Invalid weaponSetsArray index!");
            return;
        }

        Agent agent = fromPeer.ControlledAgent;

        for (int i = 0; i < weaponSetsArray.GetLength(1); i++)
        {
            ItemObject itemObject = MBObjectManager.Instance.GetObject<ItemObject>(weaponSetsArray[index, i]);
            if (itemObject != null)
            {
                if (!weaponSetsArray[index, i].IsEmpty())
                {
                    MissionWeapon mWeapon = new(itemObject, null, null);
                    agent.EquipWeaponWithNewEntity((EquipmentIndex)i, ref mWeapon);
                }
                else
                {
                    ChatComponent.ServerSendMessageToPlayer(fromPeer, ColorFatal, "weaponSetsArray is empty!!!");
                }
            }
            else
            {
                ChatComponent.ServerSendMessageToPlayer(fromPeer, ColorFatal, "itemObject is null!!!");
            }
        }

        agent.UpdateWeapons();
        agent.UpdateAgentProperties();
    }
}

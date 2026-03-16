using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml;
using Crpg.Module.Api.Models;
using Crpg.Module.Api.Models.Items;
using Crpg.Module.Common.Models;
using Crpg.Module.HarmonyPatches;
using Crpg.Module.Helpers.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.ModuleManager;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.Tableaus;
using TaleWorlds.MountAndBlade.View.Tableaus.Thumbnails;
using Path = System.IO.Path;

namespace Crpg.Module.DataExport;

internal static class ItemExporter
{
    private static readonly string ModuleDataPath = Path.Combine(ModuleHelper.GetModuleFullPath("cRPG"), "ModuleData");

    private static readonly string[] ItemFilePaths =
    [
        "items/head_armors.xml",
        "items/shoulder_armors.xml",
        "items/body_armors.xml",
        "items/arm_armors.xml",
        "items/leg_armors.xml",
        "items/weapons.xml",
        "items/horses_and_others.xml",
        "items/shields.xml"
    ];

    private static readonly string[] PiecesFilePaths =
    [
        "crafting_pieces.xml",
        "crafting_templates.xml",
        "weapon_descriptions.xml"
    ];

    private static readonly Dictionary<int, (int speedbonus, int maneuverbonus, float healthbonusPercentage)> MountHeirloomBonus = new()
    {
        [1] = (1, 0, 6.2f),
        [2] = (1, 1, 13f),
        [3] = (1, 2, 19.8f),
    };
    private static readonly Dictionary<int, int> HeadArmorHeirloomBonus = new()
    {
        [1] = 2,
        [2] = 4,
        [3] = 6,
    };
    private static readonly Dictionary<int, (int bodyArmorBonus, int legArmorBonus, int armArmorBonus)> BodyArmorHeirloomBonus = new()
    {
        [1] = (2, 1, 1),
        [2] = (4, 2, 2),
        [3] = (6, 3, 3),
    };
    private static readonly Dictionary<int, int> LegArmorHeirloomBonus = new()
    {
        [1] = 1,
        [2] = 3,
        [3] = 5,
    };
    private static readonly Dictionary<int, (int bodyArmorBonus, int armArmorBonus)> ShoulderArmorHeirloomBonus = new()
    {
        [1] = (2, 0),
        [2] = (3, 0),
        [3] = (4, 1),
    };
    private static readonly Dictionary<int, int> ArmArmorHeirloomBonus = new()
    {
        [1] = 1,
        [2] = 2,
        [3] = 4,
    };
    private static readonly Dictionary<int, int> HorseArmorHeirloomBonus = new()
    {
        [1] = 3,
        [2] = 6,
        [3] = 9,
    };
    private static readonly Dictionary<int, (int damageBonus, int amountBonusPercentage)> CutArrowHeirloomBonus = new()
    {
        [1] = (0, 18),
        [2] = (1, 18),
        [3] = (2, 18),
    };
    private static readonly Dictionary<int, (int damageBonus, int amountBonusPercentage)> PierceArrowHeirloomBonus = new()
    {
        [1] = (0, 13),
        [2] = (1, 4),
        [3] = (2, -10),
    };
    private static readonly Dictionary<int, (int damageBonus, int amountBonusPercentage)> BluntArrowHeirloomBonus = new()
    {
        [1] = (0, 15),
        [2] = (0, 30),
        [3] = (1, 30),
    };
    private static readonly Dictionary<int, (int damageBonus, int amountBonusPercentage)> CutBoltHeirloomBonus = new()
    {
        [1] = (2, 10),
        [2] = (4, 20),
        [3] = (6, 30),
    };
    private static readonly Dictionary<int, (int damageBonus, int amountBonusPercentage)> PierceBoltHeirloomBonus = new()
    {
        [1] = (1, 10),
        [2] = (2, 20),
        [3] = (3, 30),
    };
    private static readonly Dictionary<int, (int damageBonus, int amountBonusPercentage)> BluntBoltHeirloomBonus = new()
    {
        [1] = (0, 10),
        [2] = (0, 20),
        [3] = (1, 20),
    };
    private static readonly Dictionary<int, (int damageBonus, int accuracyBonus, int missileSpeedBonus, int reloadSpeedBonus, int aimSpeedBonus)> CrossbowHeirloomBonus = new()
    {
        [1] = (1, 1, 0, 2, 1),
        [2] = (2, 2, 1, 4, 2),
        [3] = (3, 3, 2, 5, 3),
    };
    private static readonly Dictionary<int, (int damageBonus, int accuracyBonus, int missileSpeedBonus, int reloadSpeedBonus, int aimSpeedBonus)> LightCrossbowHeirloomBonus = new()
    {
        [1] = (0, 2, 1, 3, 3),
        [2] = (1, 2, 1, 4, 5),
        [3] = (2, 2, 2, 4, 5),
    };
    private static readonly Dictionary<int, (int damageBonus, int accuracyBonus, int missileSpeedBonus, int reloadSpeedBonus, int aimSpeedBonus)> LongBowHeirloomBonus = new()
    {
        [1] = (0, 2, 2, 3, 1),
        [2] = (0, 4, 4, 4, 3),
        [3] = (1, 4, 4, 4, 3),
    };
    private static readonly Dictionary<int, (int damageBonus, int accuracyBonus, int missileSpeedBonus, int reloadSpeedBonus, int aimSpeedBonus)> BowHeirloomBonus = new()
    {
        [1] = (0, 0, 9, 3, 2),
        [2] = (0, 3, 9, 3, 6),
        [3] = (0, 9, 9, 3, 6),
    };
    private static readonly Dictionary<int, (int bonusHealthPercentage, int bodyArmorPercentage)> ShieldHeirloomBonus = new()
    {
        [1] = (8, 3),
        [2] = (16, 6),
        [3] = (24, 9),
    };
    private static readonly Dictionary<int, (int damageBonus, int amountBonusPercentage)> BulletHeirloomBonus = new()
    {
        [1] = (1, 10),
        [2] = (2, 20),
        [3] = (3, 30),
    };
    private static readonly Dictionary<int, (int damageBonus, int accuracyBonus, int missileSpeedBonus, int reloadSpeedBonus, int aimSpeedBonus)> MusketHeirloomBonus = new()
    {
        [1] = (1, 1, 0, 2, 1),
        [2] = (2, 2, 1, 4, 2),
        [3] = (3, 3, 2, 5, 3),
    };
    private static readonly Dictionary<int, (int damageBonus, int accuracyBonus, int missileSpeedBonus, int reloadSpeedBonus, int aimSpeedBonus)> LightMusketHeirloomBonus = new()
    {
        [1] = (0, 2, 1, 3, 3),
        [2] = (1, 2, 1, 4, 5),
        [3] = (2, 2, 2, 4, 5),
    };
    private static readonly Dictionary<int, (int damageBonus, int accuracyBonus, int missileSpeedBonus, int reloadSpeedBonus, int aimSpeedBonus)> PistolHeirloomBonus = new()
    {
        [1] = (1, 1, 0, 2, 1),
        [2] = (2, 2, 1, 4, 2),
        [3] = (3, 3, 2, 5, 3),
    };

    private static readonly HashSet<WeaponClass> MeleeWeaponClasses =
    [
        WeaponClass.Dagger,
        WeaponClass.Mace,
        WeaponClass.TwoHandedMace,
        WeaponClass.OneHandedSword,
        WeaponClass.TwoHandedSword,
        WeaponClass.OneHandedAxe,
        WeaponClass.TwoHandedAxe,
        WeaponClass.Pick,
        WeaponClass.LowGripPolearm,
        WeaponClass.OneHandedPolearm,
        WeaponClass.TwoHandedPolearm,
    ];

    /// <summary>
    /// Triggers a refund for a given item category by mutating item IDs in the XML files. It increments the version
    /// suffix (e.g. "v1") of items causing the old item to no longer exist, forcing the game to refund it to players.
    /// </summary>
    [CommandLineFunctionality.CommandLineArgumentFunction("refund", "crpg")]
    internal static string Refund(List<string> args)
    {
        const string weaponsPath = "items/weapons.xml";
        const string shieldsPath = "items/shields.xml";
        const string horsesPath = "items/horses_and_others.xml";

        void ToggleRefundInFile(string path, Func<XmlNode, bool> predicate)
        {
            path = Path.Combine(ModuleDataPath, path);

            XmlDocument doc = new();
            using (var r = XmlReader.Create(path, new XmlReaderSettings { IgnoreComments = true }))
            {
                doc.Load(r);
            }

            foreach (XmlNode node in doc.LastChild!.ChildNodes)
            {
                if (predicate(node))
                {
                    node.Attributes!["id"].Value = ToggleRefund(node.Attributes!["id"].Value);
                }
            }

            doc.Save(path);
        }

        bool IsItemOfType(XmlNode node, ItemObject.ItemTypeEnum[] types)
        {
            return node.Name == "Item" &&
                   types.Contains((ItemObject.ItemTypeEnum)Enum.Parse(typeof(ItemObject.ItemTypeEnum), node.Attributes!["Type"].Value));
        }

        string type = args.Count > 0 ? args[0].ToLowerInvariant() : "";
        switch (type)
        {
            case "weapons":
                ToggleRefundInFile(weaponsPath, _ => true);
                break;

            case "shield":
                ToggleRefundInFile(shieldsPath, n => IsItemOfType(n,
                    [ItemObject.ItemTypeEnum.Shield]));
                break;

            case "bow":
                ToggleRefundInFile(weaponsPath, n => IsItemOfType(n,
                    [ItemObject.ItemTypeEnum.Bow, ItemObject.ItemTypeEnum.Arrows]));
                break;

            case "crossbow":
                ToggleRefundInFile(weaponsPath, n => IsItemOfType(n,
                    [ItemObject.ItemTypeEnum.Crossbow, ItemObject.ItemTypeEnum.Bolts]));
                break;

            case "firearm":
                ToggleRefundInFile(weaponsPath, n => IsItemOfType(n,
                    [ItemObject.ItemTypeEnum.Musket, ItemObject.ItemTypeEnum.Pistol, ItemObject.ItemTypeEnum.Bullets]));
                break;

            case "armor":
                ItemObject.ItemTypeEnum[] armorTypes =
                [
                    ItemObject.ItemTypeEnum.BodyArmor, ItemObject.ItemTypeEnum.LegArmor,
                    ItemObject.ItemTypeEnum.HeadArmor, ItemObject.ItemTypeEnum.HandArmor,
                    ItemObject.ItemTypeEnum.ChestArmor, ItemObject.ItemTypeEnum.Cape
                ];
                foreach (string filepath in ItemFilePaths)
                {
                    ToggleRefundInFile(filepath, n => IsItemOfType(n, armorTypes));
                }

                break;

            case "throwing":
                ToggleRefundInFile(weaponsPath, n => IsItemOfType(n, [ItemObject.ItemTypeEnum.Thrown]));
                string[] templates = ["crpg_ThrowingKnife", "crpg_ThrowingAxe", "crpg_Javelin"];
                ToggleRefundInFile(weaponsPath, n => templates.Contains(n.Attributes!["crafting_template"]?.Value));
                break;

            case "cav":
                ToggleRefundInFile(horsesPath, n => IsItemOfType(n, [ItemObject.ItemTypeEnum.Horse, ItemObject.ItemTypeEnum.HorseHarness]));
                break;

            default:
                return $"Unknown refund type: {type}. Valid types: firearm, crossbow, armor, weapons, throwing, cav, bow, shield";
        }

        return $"Refunded {type}.";
    }

    /// <summary>
    /// Exports all cRPG items to a JSON file by converting Bannerlord item objects to cRPG models.
    /// </summary>
    [CommandLineFunctionality.CommandLineArgumentFunction("export_items", "crpg")]
    internal static string ExportItems(List<string> args)
    {
        var game = Game.CreateGame(new MultiplayerGame(), new MultiplayerGameManager());
        game.Initialize();
        var mbItems = game.ObjectManager.GetObjectTypeList<ItemObject>()
            .Where(i => i.StringId.StartsWith("crpg_"))
            .DistinctBy(i => i.StringId)
            .OrderBy(i => i.StringId)
            .ToArray();
        var crpgItems = mbItems.Select(MbToCrpgItem);
        SerializeCrpgItems(crpgItems, ModuleDataPath);

        return "Done.";
    }

    /// <summary>
    /// Generates heirloom variants (h1/h2/h3) for items and crafting pieces, and applies their stat bonuses.
    /// </summary>
    [CommandLineFunctionality.CommandLineArgumentFunction("compute_auto_stats", "crpg")]
    internal static string ComputeAutoStats(List<string> args)
    {
        foreach (string filePath in ItemFilePaths)
        {
            string fullPath = Path.Combine(ModuleDataPath, filePath);
            XmlComputeAutoStats(fullPath).Save(fullPath);
        }

        foreach (string filePath in PiecesFilePaths)
        {
            string fullPath = Path.Combine(ModuleDataPath, filePath);
            XmlComputeAutoStats(fullPath).Save(fullPath);
        }

        return "Done.";
    }

    /// <summary>
    /// Renders and saves PNG thumbnail images for all base (non-heirloom) cRPG items.
    /// </summary>
    [CommandLineFunctionality.CommandLineArgumentFunction("export_item_thumbnails", "crpg")]
    internal static string ExportItemThumbnails(List<string> args)
    {
        var game = Game.CreateGame(new MultiplayerGame(), new MultiplayerGameManager());
        game.Initialize();
        var mbItems = game.ObjectManager.GetObjectTypeList<ItemObject>()
            .Where(i => i.StringId.StartsWith("crpg_"))
            .DistinctBy(i => i.StringId)
            .OrderBy(i => i.StringId)
            .ToArray();
        string itemThumbnailsPath = Path.Combine(ModuleDataPath, "item-thumbnails");
        Directory.CreateDirectory(itemThumbnailsPath);
        RegisterRenderRequestPatch.IsEnabled = true;
        GenerateItemsThumbnail(mbItems, itemThumbnailsPath)
            .ContinueWith(t =>
            {
                RegisterRenderRequestPatch.IsEnabled = false;
                MBDebug.EchoCommandWindow(t.IsFaulted
                    ? "Error. Check the logs."
                    : "Done.");
            });

        return $"Exporting {mbItems.Length} images...";
    }

    /// <summary>
    /// Applies a flat 0.9x multiplier to the <c>missile_speed</c> attribute of every Bow-type
    /// item in <c>items/weapons.xml</c>. Used for batch-balancing bow projectile speeds.
    /// </summary>
    [CommandLineFunctionality.CommandLineArgumentFunction("scale", "crpg")]
    internal static string Scale(List<string> args)
    {
        var itemsDoc = XmlScaleClass(Path.Combine(ModuleDataPath, "items/weapons.xml"), ItemObject.ItemTypeEnum.Bow);
        itemsDoc.Save(Path.Combine(ModuleDataPath, "items/weapons.xml"));
        return "Done.";
    }

    /// <summary>
    /// Collects every blade crafting piece used by weapons with the <c>crpg_TwoHandedPolearm</c>
    /// template, then reduces each blade's <c>damage_factor</c> in <c>crafting_pieces.xml</c>.
    /// The reduction is derived from the damage type (Cut/Pierce/Blunt) so that stronger damage
    /// types receive a proportionally larger nerf.
    /// </summary>
    [CommandLineFunctionality.CommandLineArgumentFunction("scale_weapon", "crpg")]
    internal static string ScaleWeapon(List<string> args)
    {
        var itemsDoc = XmlScaleWeapon(
            Path.Combine(ModuleDataPath, "items/weapons.xml"),
            Path.Combine(ModuleDataPath, "crafting_pieces.xml"),
            "crpg_TwoHandedPolearm");
        itemsDoc.Save(Path.Combine(ModuleDataPath, "crafting_pieces.xml"));
        return "Done.";
    }

    private static CrpgItem MbToCrpgItem(ItemObject mbItem)
    {
        CrpgItem crpgItem = new()
        {
            Id = mbItem.StringId,
            BaseId = mbItem.StringId.Split('_').Last().Substring(0, 1) == "h"
                ? mbItem.StringId.Substring(0, mbItem.StringId.Length - 3)
                : mbItem.StringId,
            Name = mbItem.Name.ToString(),
            Culture = MbToCrpgCulture(mbItem.Culture),
            Type = MbToCrpgItemType(mbItem.Type),
            Price = mbItem.Value,
            Weight = mbItem.Weight,
            Rank = mbItem.Type == ItemObject.ItemTypeEnum.Banner ? 0 : int.Parse(mbItem.StringId.Split('_').Last().Substring(1)),
            Tier = mbItem.Tierf,
            Requirement = CrpgItemRequirementModel.ComputeItemRequirement(mbItem),
            Flags = MbToCrpgItemFlags(mbItem.ItemFlags),
        };

        if (mbItem.ArmorComponent != null)
        {
            crpgItem.Armor = new CrpgItemArmorComponent
            {
                HeadArmor = mbItem.ArmorComponent.HeadArmor,
                BodyArmor = mbItem.ArmorComponent.BodyArmor,
                ArmArmor = mbItem.ArmorComponent.ArmArmor,
                LegArmor = mbItem.ArmorComponent.LegArmor,
                MaterialType = MbToCrpgArmorMaterialType(mbItem.ArmorComponent.MaterialType),
                FamilyType = mbItem.ArmorComponent.FamilyType,
            };
        }

        if (mbItem.HorseComponent != null)
        {
            crpgItem.Mount = new CrpgItemMountComponent
            {
                BodyLength = mbItem.HorseComponent.BodyLength,
                ChargeDamage = mbItem.HorseComponent.ChargeDamage,
                Maneuver = mbItem.HorseComponent.Maneuver,
                Speed = mbItem.HorseComponent.Speed,
                HitPoints = mbItem.HorseComponent.HitPoints + mbItem.HorseComponent.HitPointBonus,
                FamilyType = mbItem.HorseComponent.Monster.FamilyType,
            };
        }

        if (mbItem.WeaponComponent != null)
        {
            crpgItem.Weapons = mbItem.WeaponComponent.Weapons.Select(w => new CrpgItemWeaponComponent
            {
                Class = MbToCrpgWeaponClass(w.WeaponClass),
                ItemUsage = w.ItemUsage,
                Accuracy = w.Accuracy,
                MissileSpeed = w.MissileSpeed,
                StackAmount = w.MaxDataValue,
                Length = w.WeaponLength,
                Balance = w.WeaponBalance,
                Handling = w.Handling,
                BodyArmor = w.BodyArmor,
                Flags = MbToCrpgWeaponFlags(w.WeaponFlags),
                ThrustDamage = MeleeWeaponClasses.Contains(w.WeaponClass)
                    ? (int)(w.ThrustDamageFactor * CrpgStrikeMagnitudeModel.BladeDamageFactorToDamageRatio)
                    : w.ThrustDamage,
                ThrustDamageType = MbToCrpgDamageType(w.ThrustDamageType),
                ThrustSpeed = w.ThrustSpeed,
                SwingDamage = MeleeWeaponClasses.Contains(w.WeaponClass)
                    ? (int)(w.SwingDamageFactor * CrpgStrikeMagnitudeModel.BladeDamageFactorToDamageRatio)
                    : w.SwingDamage,
                SwingDamageType = MbToCrpgDamageType(w.SwingDamageType),
                SwingSpeed = w.SwingSpeed,
            }).ToArray();
        }

        return crpgItem;
    }

    private static XmlDocument XmlScaleClass(string filePath, ItemObject.ItemTypeEnum typeToRefund)
    {
        XmlDocument itemsDoc = new();
        using (var r = XmlReader.Create(filePath, new XmlReaderSettings { IgnoreComments = true }))
        {
            itemsDoc.Load(r);
        }

        var nodes1 = itemsDoc.LastChild.ChildNodes.Cast<XmlNode>().ToArray();
        for (int i = 0; i < nodes1.Length; i += 1)
        {
            var node1 = nodes1[i];
            if (node1.Name == "Item")
            {
                var type = (ItemObject.ItemTypeEnum)Enum.Parse(typeof(ItemObject.ItemTypeEnum), node1.Attributes!["Type"].Value);
                if (type == typeToRefund)
                {
                    ModifyChildNodesAttribute(node1, "ItemComponent/Weapon", "missile_speed",
                        x => ((int)(int.Parse(x) * 0.9f)).ToString());
                }
            }
        }

        return itemsDoc;
    }

    private static XmlDocument XmlScaleWeapon(string weaponsFilePath, string craftingPiecesFilePath, string weaponCraftingTeamplate)
    {
        XmlDocument weaponsDoc = new();
        using (var r = XmlReader.Create(weaponsFilePath, new XmlReaderSettings { IgnoreComments = true }))
        {
            weaponsDoc.Load(r);
        }

        HashSet<string> piecesToScale = new();

        var nodes1 = weaponsDoc.LastChild.ChildNodes.Cast<XmlNode>().ToArray();
        for (int i = 0; i < nodes1.Length; i += 1)
        {
            if (nodes1[i].Attributes!["crafting_template"]?.Value != weaponCraftingTeamplate)
            {
                continue;
            }

            var pieces = nodes1[i].SelectSingleNode("Pieces")?.ChildNodes;
            string? pieceId = pieces?.Cast<XmlNode>().FirstOrDefault(n => n.Attributes!["Type"].Value == "Blade")?.Attributes!["id"]?.Value;
            if (pieceId != null)
            {
                piecesToScale.Add(pieceId);
            }
        }

        XmlDocument craftingPiecesDoc = new();
        using (var r = XmlReader.Create(craftingPiecesFilePath, new XmlReaderSettings { IgnoreComments = true }))
        {
            craftingPiecesDoc.Load(r);
        }

        var nodes2 = craftingPiecesDoc.LastChild.ChildNodes.Cast<XmlNode>().ToArray();
        for (int i = 0; i < nodes2.Length; i += 1)
        {
            if (nodes2[i].Attributes!["piece_type"].Value == "Blade" && piecesToScale.Contains(nodes2[i].Attributes!["id"].Value))
            {
                var bladeData = nodes2[i].SelectSingleNode("BladeData")?.ChildNodes.Cast<XmlNode>();
                if (bladeData == null)
                {
                    continue;
                }

                foreach (var damageNode in bladeData)
                {
                    float scaler = 1 - 0.03f / (float)Math.Pow(damageNode.Attributes!["damage_type"].Value switch
                    {
                        "Blunt" => CrpgItemValueModel.CalculateDamageTypeFactor(DamageTypes.Blunt),
                        "Cut" => CrpgItemValueModel.CalculateDamageTypeFactor(DamageTypes.Cut),
                        "Pierce" => CrpgItemValueModel.CalculateDamageTypeFactor(DamageTypes.Pierce),
                        _ => 1,
                    }, 1 / 2.15f);

                    if (damageNode.Name == "Swing")
                    {
                        damageNode.Attributes["damage_factor"].Value = Math
                            .Round(float.Parse(damageNode.Attributes["damage_factor"].Value) * scaler, 1)
                            .ToString(CultureInfo.InvariantCulture);
                    }
                }
            }
        }

        return craftingPiecesDoc;
    }

    private static void CloneHeirloomVariants(XmlNode node, string baseId, string attrName = "id")
    {
        for (int h = 1; h <= 3; h++)
        {
            XmlNode clone = node.CloneNode(true);
            clone.Attributes![attrName].Value = baseId + "h" + h;
            node.ParentNode!.InsertAfter(clone, node);
            node = clone;
        }
    }

    private static void ApplyRangedHeirloomBonus(
        XmlNode nonHeirloomNode,
        XmlNode heirloomNode,
        (int damageBonus, int accuracyBonus, int missileSpeedBonus, int reloadSpeedBonus, int aimSpeedBonus) bonus)
    {
        ModifyChildHeirloomNodesAttribute(nonHeirloomNode, heirloomNode, "ItemComponent/Weapon", "thrust_damage", bonus.damageBonus);
        ModifyChildHeirloomNodesAttribute(nonHeirloomNode, heirloomNode, "ItemComponent/Weapon", "speed_rating", bonus.reloadSpeedBonus);
        ModifyChildHeirloomNodesAttribute(nonHeirloomNode, heirloomNode, "ItemComponent/Weapon", "thrust_speed", bonus.aimSpeedBonus);
        ModifyChildHeirloomNodesAttribute(nonHeirloomNode, heirloomNode, "ItemComponent/Weapon", "accuracy", bonus.accuracyBonus);
        ModifyChildHeirloomNodesAttribute(nonHeirloomNode, heirloomNode, "ItemComponent/Weapon", "missile_speed", bonus.missileSpeedBonus);
    }

    private static void AddToList<TKey>(Dictionary<TKey, List<XmlNode>> dict, TKey key, XmlNode node)
        where TKey : notnull
    {
        if (!dict.TryGetValue(key, out var list))
        {
            list = [];
            dict[key] = list;
        }

        list.Add(node);
    }

    private static XmlDocument XmlComputeAutoStats(string filePath)
    {
        XmlDocument itemsDoc = new();
        using (var r = XmlReader.Create(filePath, new XmlReaderSettings { IgnoreComments = true }))
        {
            itemsDoc.Load(r);
        }

        Dictionary<string, XmlNode> baseItem = new();
        Dictionary<(string craftingtemplate, string usablepieceid), List<XmlNode>> upgradedUsablePiece = new();
        Dictionary<(string craftingtemplate, string usablepieceid), List<XmlNode>> upgradedAvailablePiece = new();
        Dictionary<string, List<XmlNode>> upgradedItem = new();
        var nodes1 = itemsDoc.LastChild.ChildNodes.Cast<XmlNode>().ToArray();

        // Pass 1: Index base (h0) and upgraded items into dictionaries.
        for (int i = 0; i < nodes1.Length; i += 1)
        {
            var node1 = nodes1[i];
            if (node1.Name is "Item" or "CraftedItem" or "CraftingPiece")
            {
                int heirloomLevel = IdToHeirloomLevel(node1.Attributes!["id"].Value);
                string baseId = node1.Attributes!["id"].Value.Remove(node1.Attributes!["id"].Value.Length - 2);
                if (heirloomLevel == 0)
                {
                    if (!baseItem.ContainsKey(baseId))
                    {
                        baseItem[baseId] = node1;
                    }
                }
                else
                {
                    AddToList(upgradedItem, baseId, node1);
                }
            }

            if (node1.Name == "WeaponDescription")
            {
                foreach (XmlNode piece in node1.SelectSingleNode("AvailablePieces")!.ChildNodes)
                {
                    int heirloomLevel = IdToHeirloomLevel(piece.Attributes!["id"].Value);
                    if (heirloomLevel == 0)
                    {
                        continue;
                    }

                    string baseId = piece.Attributes!["id"].Value.Remove(piece.Attributes!["id"].Value.Length - 2);
                    var key = (piece.ParentNode!.ParentNode!.Attributes!["id"]!.Value, baseId);
                    AddToList(upgradedAvailablePiece, key, piece);
                }
            }

            if (node1.Name == "CraftingTemplate")
            {
                foreach (XmlNode piece in node1.SelectSingleNode("UsablePieces")!.ChildNodes)
                {
                    int heirloomLevel = IdToHeirloomLevel(piece.Attributes!["piece_id"].Value);
                    if (heirloomLevel == 0)
                    {
                        continue;
                    }

                    string baseId = piece.Attributes!["piece_id"].Value.Remove(piece.Attributes!["piece_id"].Value.Length - 2);
                    var key = (piece.ParentNode!.ParentNode!.Attributes!["id"]!.Value, baseId);
                    AddToList(upgradedUsablePiece, key, piece);
                }
            }
        }

        // Pass 2: Clone base items to create h1/h2/h3 variants where they don't already exist.
        for (int i = 0; i < nodes1.Length; i += 1)
        {
            var node1 = nodes1[i];
            if (node1.Name == "Item")
            {
                int heirloomLevel = IdToHeirloomLevel(node1.Attributes!["id"].Value);
                string baseId = node1.Attributes!["id"].Value.Remove(node1.Attributes!["id"].Value.Length - 2);
                if (heirloomLevel == 0 && !upgradedItem.ContainsKey(baseId))
                {
                    CloneHeirloomVariants(node1, baseId);
                }
            }
            else if (node1.Name == "CraftedItem")
            {
                string baseId = node1.Attributes!["id"].Value.Remove(node1.Attributes!["id"].Value.Length - 2);
                if (!upgradedItem.ContainsKey(baseId))
                {
                    CloneHeirloomVariants(node1, baseId);

                    // Also update piece IDs inside each cloned CraftedItem.
                    foreach (XmlNode sibling in node1.ParentNode!.ChildNodes)
                    {
                        int siblingLevel = IdToHeirloomLevel(sibling.Attributes?["id"]?.Value ?? "");
                        if (siblingLevel == 0)
                        {
                            continue;
                        }

                        string siblingBaseId = sibling.Attributes!["id"].Value.Remove(sibling.Attributes!["id"].Value.Length - 2);
                        if (siblingBaseId != baseId)
                        {
                            continue;
                        }

                        foreach (XmlNode pieceNode in sibling.LastChild.ChildNodes)
                        {
                            string pieceBaseId = pieceNode.Attributes!["id"].Value.Remove(pieceNode.Attributes!["id"].Value.Length - 2);
                            pieceNode.Attributes!["id"].Value = pieceBaseId + "h" + siblingLevel;
                        }
                    }
                }
            }
            else if (node1.Name == "CraftingPiece")
            {
                string baseId = node1.Attributes!["id"].Value.Remove(node1.Attributes!["id"].Value.Length - 2);
                if (!upgradedItem.ContainsKey(baseId))
                {
                    CloneHeirloomVariants(node1, baseId);
                }
            }
            else if (node1.Name == "WeaponDescription")
            {
                foreach (XmlNode piece in node1.SelectSingleNode("AvailablePieces")!.ChildNodes.Cast<XmlNode>().ToArray())
                {
                    string baseId = piece.Attributes!["id"].Value.Remove(piece.Attributes!["id"].Value.Length - 2);
                    if (!upgradedAvailablePiece.ContainsKey((piece.ParentNode!.ParentNode!.Attributes!["id"]!.Value, baseId)))
                    {
                        CloneHeirloomVariants(piece, baseId);
                    }
                }
            }
            else if (node1.Name == "CraftingTemplate")
            {
                foreach (XmlNode piece in node1.SelectSingleNode("UsablePieces")!.ChildNodes.Cast<XmlNode>().ToArray())
                {
                    string baseId = piece.Attributes!["piece_id"].Value.Remove(piece.Attributes!["piece_id"].Value.Length - 2);
                    if (!upgradedUsablePiece.ContainsKey((piece.ParentNode!.ParentNode!.Attributes!["id"]!.Value, baseId)))
                    {
                        CloneHeirloomVariants(piece, baseId, "piece_id");
                    }
                }
            }
        }

        // Pass 3: Apply heirloom stat bonuses to upgraded items.
        for (int i = 0; i < nodes1.Length; i += 1)
        {
            var node1 = nodes1[i];
            if (node1.Name == "Item")
            {
                int heirloomLevel = IdToHeirloomLevel(node1.Attributes!["id"].Value);
                string baseId = node1.Attributes!["id"].Value.Remove(node1.Attributes!["id"].Value.Length - 2);
                if (heirloomLevel == 0)
                {
                    continue;
                }

                var nonHeirloomNode = baseItem[baseId];
                node1.Attributes["name"].Value = nonHeirloomNode.Attributes!["name"].Value + $" +{heirloomLevel}";
                var type = (ItemObject.ItemTypeEnum)Enum.Parse(typeof(ItemObject.ItemTypeEnum), node1.Attributes!["Type"].Value);
                switch (type)
                {
                    case ItemObject.ItemTypeEnum.Horse:

                        if (MountHeirloomBonus.TryGetValue(heirloomLevel, out var newMount))
                        {
                            ModifyChildHeirloomNodesAttribute(nonHeirloomNode, node1, "ItemComponent/Horse", "maneuver",
                                newMount.maneuverbonus);
                            ModifyChildHeirloomNodesAttribute(nonHeirloomNode, node1, "ItemComponent/Horse", "speed",
                                newMount.speedbonus);
                            ModifyChildHeirloomNodesAttribute(nonHeirloomNode, node1, "ItemComponent/Horse",
                                "extra_health", 0, ihatemountsPercentage: newMount.healthbonusPercentage);
                        }

                        break;

                    case ItemObject.ItemTypeEnum.Arrows:
                        string arrowDamageTypeStr = node1.SelectNodes("ItemComponent/Weapon")!
                            .Cast<XmlNode>().Last().Attributes!["thrust_damage_type"].Value;
                        var arrowDamageType = (DamageTypes)Enum.Parse(typeof(DamageTypes), arrowDamageTypeStr);
                        var relevantArrowDictionary = arrowDamageType switch
                        {
                            DamageTypes.Cut => CutArrowHeirloomBonus,
                            DamageTypes.Pierce => PierceArrowHeirloomBonus,
                            DamageTypes.Blunt => BluntArrowHeirloomBonus,
                            _ => throw new NotImplementedException(),
                        };
                        if (relevantArrowDictionary.TryGetValue(heirloomLevel, out var newArrow))
                        {
                            ModifyChildHeirloomNodesAttribute(nonHeirloomNode, node1, "ItemComponent/Weapon",
                                "thrust_damage", newArrow.damageBonus);
                            ModifyChildHeirloomNodesAttribute(nonHeirloomNode, node1, "ItemComponent/Weapon",
                                "stack_amount", 0, bonusPercentage: newArrow.amountBonusPercentage);
                        }

                        break;

                    case ItemObject.ItemTypeEnum.Bolts:
                        string boltDamageTypeStr = node1.SelectNodes("ItemComponent/Weapon")!
                            .Cast<XmlNode>().Last().Attributes!["thrust_damage_type"].Value;
                        var boltDamageType = (DamageTypes)Enum.Parse(typeof(DamageTypes), boltDamageTypeStr);
                        var relevantBoltDictionary = boltDamageType switch
                        {
                            DamageTypes.Cut => CutBoltHeirloomBonus,
                            DamageTypes.Pierce => PierceBoltHeirloomBonus,
                            DamageTypes.Blunt => BluntBoltHeirloomBonus,
                            _ => throw new NotImplementedException(),
                        };

                        if (relevantBoltDictionary.TryGetValue(heirloomLevel, out var newBolt))
                        {
                            ModifyChildHeirloomNodesAttribute(nonHeirloomNode, node1, "ItemComponent/Weapon", "thrust_damage", newBolt.damageBonus);
                            ModifyChildHeirloomNodesAttribute(nonHeirloomNode, node1, "ItemComponent/Weapon", "stack_amount", 0, bonusPercentage: newBolt.amountBonusPercentage);
                        }

                        break;

                    case ItemObject.ItemTypeEnum.Bullets:
                        if (BulletHeirloomBonus.TryGetValue(heirloomLevel, out var newBullet))
                        {
                            ModifyChildHeirloomNodesAttribute(nonHeirloomNode, node1, "ItemComponent/Weapon", "thrust_damage", newBullet.damageBonus);
                            ModifyChildHeirloomNodesAttribute(nonHeirloomNode, node1, "ItemComponent/Weapon", "stack_amount", 0, bonusPercentage: newBullet.amountBonusPercentage);
                        }

                        break;

                    case ItemObject.ItemTypeEnum.Shield:
                        if (ShieldHeirloomBonus.TryGetValue(heirloomLevel, out var newShield))
                        {
                            ModifyChildHeirloomNodesAttribute(nonHeirloomNode, node1, "ItemComponent/Weapon", "hit_points", 0, bonusPercentage: newShield.bonusHealthPercentage);
                            ModifyChildHeirloomNodesAttribute(nonHeirloomNode, node1, "ItemComponent/Weapon", "body_armor", 0, bonusPercentage: newShield.bodyArmorPercentage);
                        }

                        break;

                    case ItemObject.ItemTypeEnum.Bow:
                    {
                        string bowItemUsage = node1.SelectSingleNode("ItemComponent/Weapon")!.Attributes!["item_usage"].Value;
                        var bowBonusTable = bowItemUsage == "long_bow" ? LongBowHeirloomBonus : BowHeirloomBonus;
                        if (bowBonusTable.TryGetValue(heirloomLevel, out var bowBonus))
                        {
                            ApplyRangedHeirloomBonus(nonHeirloomNode, node1, bowBonus);
                        }

                        break;
                    }

                    case ItemObject.ItemTypeEnum.Crossbow:
                    {
                        string crossbowItemUsage = node1.SelectSingleNode("ItemComponent/Weapon")!.Attributes!["item_usage"].Value;
                        var crossbowBonusTable = crossbowItemUsage == "crossbow" ? CrossbowHeirloomBonus : LightCrossbowHeirloomBonus;
                        if (crossbowBonusTable.TryGetValue(heirloomLevel, out var crossbowBonus))
                        {
                            ApplyRangedHeirloomBonus(nonHeirloomNode, node1, crossbowBonus);
                        }

                        break;
                    }

                    case ItemObject.ItemTypeEnum.Musket:
                    {
                        string musketItemUsage = node1.SelectSingleNode("ItemComponent/Weapon")?.Attributes!["item_usage"]?.Value ?? string.Empty;
                        var musketBonusTable = musketItemUsage == "crpg_light_gun" ? LightMusketHeirloomBonus : MusketHeirloomBonus;
                        if (musketBonusTable.TryGetValue(heirloomLevel, out var musketBonus))
                        {
                            ApplyRangedHeirloomBonus(nonHeirloomNode, node1, musketBonus);
                        }

                        break;
                    }

                    case ItemObject.ItemTypeEnum.Pistol:
                        if (PistolHeirloomBonus.TryGetValue(heirloomLevel, out var pistolBonus))
                        {
                            ApplyRangedHeirloomBonus(nonHeirloomNode, node1, pistolBonus);
                        }

                        break;

                    case ItemObject.ItemTypeEnum.HeadArmor:

                        if (HeadArmorHeirloomBonus.TryGetValue(heirloomLevel, out int newHeadArmor))
                        {
                            ModifyChildHeirloomNodesAttribute(nonHeirloomNode, node1, "ItemComponent/Armor",
                                "head_armor", newHeadArmor);
                        }

                        break;

                    case ItemObject.ItemTypeEnum.BodyArmor:

                        if (BodyArmorHeirloomBonus.TryGetValue(heirloomLevel, out var newBodyArmor))
                        {
                            ModifyChildHeirloomNodesAttribute(nonHeirloomNode, node1, "ItemComponent/Armor",
                                "body_armor", newBodyArmor.bodyArmorBonus);
                            ModifyChildHeirloomNodesAttribute(nonHeirloomNode, node1, "ItemComponent/Armor",
                                "leg_armor", newBodyArmor.legArmorBonus);
                            ModifyChildHeirloomNodesAttribute(nonHeirloomNode, node1, "ItemComponent/Armor",
                                "arm_armor", newBodyArmor.armArmorBonus);
                        }

                        break;

                    case ItemObject.ItemTypeEnum.LegArmor:

                        if (LegArmorHeirloomBonus.TryGetValue(heirloomLevel, out int newLegArmor))
                        {
                            ModifyChildHeirloomNodesAttribute(nonHeirloomNode, node1, "ItemComponent/Armor",
                                "leg_armor", newLegArmor);
                        }

                        break;

                    case ItemObject.ItemTypeEnum.HandArmor:

                        if (ArmArmorHeirloomBonus.TryGetValue(heirloomLevel, out int newArmArmor))
                        {
                            ModifyChildHeirloomNodesAttribute(nonHeirloomNode, node1, "ItemComponent/Armor",
                                "arm_armor", newArmArmor);
                        }

                        break;

                    case ItemObject.ItemTypeEnum.Cape:

                        if (ShoulderArmorHeirloomBonus.TryGetValue(heirloomLevel, out var newShoulderArmor))
                        {
                            ModifyChildHeirloomNodesAttribute(nonHeirloomNode, node1, "ItemComponent/Armor",
                                "body_armor", newShoulderArmor.bodyArmorBonus, defaultValue: "0");
                            ModifyChildHeirloomNodesAttribute(nonHeirloomNode, node1, "ItemComponent/Armor",
                                "arm_armor", newShoulderArmor.armArmorBonus, defaultValue: "0");
                        }

                        break;

                    case ItemObject.ItemTypeEnum.HorseHarness:

                        if (HorseArmorHeirloomBonus.TryGetValue(heirloomLevel, out int newHorseArmor))
                        {
                            ModifyChildHeirloomNodesAttribute(nonHeirloomNode, node1, "ItemComponent/Armor",
                                "body_armor", newHorseArmor);
                        }

                        break;
                }
            }
        }

        for (int i = 0; i < nodes1.Length; i += 1)
        {
            var node1 = nodes1[i];
            if (node1.Name == "Item")
            {
                int heirloomLevel = IdToHeirloomLevel(node1.Attributes!["id"].Value);
                string baseId = node1.Attributes!["id"].Value.Remove(node1.Attributes!["id"].Value.Length - 2);

                var nonHeirloomNode = baseItem[baseId];
                var type = (ItemObject.ItemTypeEnum)Enum.Parse(typeof(ItemObject.ItemTypeEnum), node1.Attributes!["Type"].Value);
                if (type is ItemObject.ItemTypeEnum.HeadArmor
                         or ItemObject.ItemTypeEnum.Cape
                         or ItemObject.ItemTypeEnum.BodyArmor
                         or ItemObject.ItemTypeEnum.HandArmor
                         or ItemObject.ItemTypeEnum.LegArmor
                         or ItemObject.ItemTypeEnum.HorseHarness)
                {
                    ModifyNodeAttribute(node1, "weight",
                        _ => ModifyArmorWeight(nonHeirloomNode, type).ToString(CultureInfo.InvariantCulture));
                }

                if (type is ItemObject.ItemTypeEnum.Shield)
                {
                    ModifyNodeAttribute(node1, "weight",
                        _ => ModifyShieldWeight(nonHeirloomNode).ToString(CultureInfo.InvariantCulture));
                }

                if (type is ItemObject.ItemTypeEnum.Bow)
                {
                    ModifyNodeAttribute(node1, "weight",
                        _ => ModifyBowWeight(nonHeirloomNode, heirloomLevel).ToString(CultureInfo.InvariantCulture));
                }

                if (type is ItemObject.ItemTypeEnum.Arrows)
                {
                    ModifyNodeAttribute(node1, "weight",
                        _ => ModifyArrowWeight(nonHeirloomNode).ToString(CultureInfo.InvariantCulture));
                }
            }
        }

        return itemsDoc;
    }

    private static float ModifyArmorWeight(XmlNode nonHeirloomNode, ItemObject.ItemTypeEnum type)
    {
        XmlNode armorNode = nonHeirloomNode.SelectNodes("ItemComponent/Armor")!.Cast<XmlNode>().First();
        float armorPower =
            1.0f * (armorNode.Attributes!["head_armor"] == null ? 0f : float.Parse(armorNode.Attributes["head_armor"].Value))
          + 1.15f * (armorNode.Attributes["body_armor"] == null ? 0f : float.Parse(armorNode.Attributes["body_armor"].Value))
          + 1.0f * (armorNode.Attributes["arm_armor"] == null ? 0f : float.Parse(armorNode.Attributes["arm_armor"].Value))
          + 0.8f * (armorNode.Attributes["leg_armor"] == null ? 0f : float.Parse(armorNode.Attributes["leg_armor"].Value));
        float bestArmorPower = type switch
        {
            ItemObject.ItemTypeEnum.HeadArmor => 661f,
            ItemObject.ItemTypeEnum.Cape => 400f,
            ItemObject.ItemTypeEnum.BodyArmor => 400f,
            ItemObject.ItemTypeEnum.HandArmor => 400f,
            ItemObject.ItemTypeEnum.LegArmor => 400f,
            ItemObject.ItemTypeEnum.HorseHarness => 100f,
            _ => throw new ArgumentOutOfRangeException(),
        };
        if (type is ItemObject.ItemTypeEnum.HorseHarness)
        {
            return armorNode.Attributes["body_armor"] == null ? 0f : float.Parse(armorNode.Attributes["body_armor"].Value);
        }

        return 12.2f * (float)Math.Pow(armorPower, 1.4f) / bestArmorPower;
    }

    private static float ModifyShieldWeight(XmlNode nonHeirloomNode)
    {
        XmlNode shieldNode = nonHeirloomNode.SelectNodes("ItemComponent/Weapon")!.Cast<XmlNode>().First();
        float shieldWeightPoints =
            float.Parse(shieldNode.Attributes!["hit_points"].Value)
          * float.Parse(shieldNode.Attributes!["weapon_length"].Value)
          * float.Parse(shieldNode.Attributes!["weapon_length"].Value);
        return shieldWeightPoints / 800000f;
    }

    private static float ModifyBowWeight(XmlNode nonHeirloomNode, int heirloomLevel)
    {
        XmlNode weaponNode = nonHeirloomNode.SelectNodes("ItemComponent/Weapon")!.Cast<XmlNode>().First();
        float tier = CrpgItemValueModel.ComputeBowTier(int.Parse(weaponNode.Attributes!["thrust_damage"].Value),
            int.Parse(weaponNode.Attributes!["speed_rating"].Value),
            int.Parse(weaponNode.Attributes!["missile_speed"].Value),
            int.Parse(weaponNode.Attributes!["thrust_speed"].Value),
            int.Parse(weaponNode.Attributes!["accuracy"].Value),
            weaponNode.Attributes!["item_usage"].Value == "long_bow",
            heirloomLevel);
        return 2f * tier * int.Parse(weaponNode.Attributes!["thrust_damage"].Value) / 100f;
    }

    private static float ModifyArrowWeight(XmlNode nonHeirloomNode)
    {
        XmlNode weaponNode = nonHeirloomNode.SelectNodes("ItemComponent/Weapon")!.Cast<XmlNode>().First();
        DamageTypes damagetype = weaponNode.Attributes!["thrust_damage_type"].Value switch
        {
            "Cut" => DamageTypes.Cut,
            "Pierce" => DamageTypes.Pierce,
            "Blunt" => DamageTypes.Blunt,
            _ => DamageTypes.Blunt,
        };

        float weight = MBMath.Lerp(0.1f, 0.15f, MBMath.ClampFloat(
            int.Parse(
                weaponNode.Attributes!["thrust_damage"].Value)
            * CrpgItemValueModel.CalculateDamageTypeFactorForAmmo(damagetype) / 12f, 0f, 1f));

        return weight * 1.5f;
    }

    private static void ModifyChildHeirloomNodesAttribute(
        XmlNode nonHeirloomNode,
        XmlNode parentNode,
        string childXPath,
        string attributeName,
        int bonus,
        Func<XmlNode, bool>? filter = null,
        string? defaultValue = null,
        float bonusPercentage = 0,
        float ihatemountsPercentage = 0)
    {
        foreach (var heirloomChildNode in parentNode.SelectNodes(childXPath)!.Cast<XmlNode>())
        {
            if (filter != null && !filter(heirloomChildNode))
            {
                continue;
            }

            foreach (var nonHeirloomChildNode in nonHeirloomNode.SelectNodes(childXPath)!.Cast<XmlNode>())
            {
                if (filter != null && !filter(nonHeirloomChildNode))
                {
                    continue;
                }

                ModifyHeirloomNodeAttribute(nonHeirloomChildNode, heirloomChildNode, attributeName, bonus,
                    bonusPercentage, ihatemountsPercentage, defaultValue);
            }
        }
    }

    private static void ModifyHeirloomNodeAttribute(
        XmlNode nonHeirloomNode,
        XmlNode heirloomNode,
        string attributeName,
        int bonus,
        float bonusPercentage,
        float ihatemountsPercentage,
        string? defaultValue = null)
    {
        var heirloomAttr = heirloomNode.Attributes![attributeName];
        var nonHeirloomAttr = nonHeirloomNode.Attributes![attributeName];
        if (heirloomAttr == null)
        {
            if (defaultValue == null)
            {
                throw new KeyNotFoundException($"heirloomAttribute '{attributeName}' was not found and no default was provided");
            }

            heirloomAttr = heirloomNode.OwnerDocument!.CreateAttribute(attributeName);
            heirloomAttr.Value = defaultValue;
            heirloomNode.Attributes.Append(heirloomAttr);
        }

        string nonHeirloomAttrValue = nonHeirloomAttr == null ? "0" : nonHeirloomAttr.Value;
        heirloomAttr.Value = (int.Parse(nonHeirloomAttrValue) + bonus +
                              (int)Math.Ceiling(int.Parse(nonHeirloomAttrValue) * bonusPercentage / 100f) +
                              (int)Math.Ceiling((200 + int.Parse(nonHeirloomAttrValue)) * ihatemountsPercentage / 100f))
            .ToString();
    }

    private static void ModifyNodeAttribute(
        XmlNode node,
        string attributeName,
        Func<string, string> modify,
        string? defaultValue = null)
    {
        var attr = node.Attributes![attributeName];
        if (attr == null)
        {
            if (defaultValue == null)
            {
                throw new KeyNotFoundException($"Attribute '{attributeName}' was not found and no default was provided");
            }

            attr = node.OwnerDocument!.CreateAttribute(attributeName);
            attr.Value = defaultValue;
            node.Attributes.Append(attr);
        }

        attr.Value = modify(attr.Value);
    }

    private static void ModifyChildNodesAttribute(XmlNode parentNode,
        string childXPath,
        string attributeName,
        Func<string, string> modify,
        Func<XmlNode, bool>? filter = null,
        string? defaultValue = null)
    {
        foreach (var childNode in parentNode.SelectNodes(childXPath)!.Cast<XmlNode>())
        {
            if (filter != null && !filter(childNode))
            {
                continue;
            }

            ModifyNodeAttribute(childNode, attributeName, modify, defaultValue);
        }
    }

    private static CrpgItemType MbToCrpgItemType(ItemObject.ItemTypeEnum t) => t switch
    {
        ItemObject.ItemTypeEnum.Invalid => CrpgItemType.Undefined, // To be consistent with WeaponClass.
        ItemObject.ItemTypeEnum.Horse => CrpgItemType.Mount, // Horse includes camel and mule.
        ItemObject.ItemTypeEnum.HorseHarness => CrpgItemType.MountHarness, // Horse includes camel and mule.
        ItemObject.ItemTypeEnum.Cape => CrpgItemType.ShoulderArmor, // Cape is a bad name.
        _ => (CrpgItemType)Enum.Parse(typeof(CrpgItemType), t.ToString()),
    };

    private static int IdToHeirloomLevel(string id)
    {
        string[] parts = id.Split('_');
        string last = parts[parts.Length - 1];
        return last.StartsWith("h") && int.TryParse(last.Substring(1), out int level) ? level : 0;
    }

    private static int ItemRank(ItemObject mbItem)
    {
        return IdToHeirloomLevel(mbItem.StringId);
    }

    private static string ToggleRefund(string id)
    {
        var idParts = id.Split('_').ToList();
        string lastPart = idParts.Last();

        // If REFUND already exists, we remove it.
        if (idParts.Contains("REFUND"))
        {
            idParts = idParts.Where(part => part != "REFUND").ToList();
        }

        // Check if idParts contains any 'v' followed by a number
        int versionPartIndex = idParts.FindIndex(part => Regex.IsMatch(part, @"^v\d+$"));

        if (versionPartIndex != -1)
        {
            // Extract the version number, increment it and replace the original version string
            int versionNumber = int.Parse(idParts[versionPartIndex].Substring(1));  // remove 'v' and parse the number
            idParts[versionPartIndex] = "v" + (versionNumber + 1);  // replace the old version string with the new one
        }
        else // If neither REFUND nor vN is present, we append v1
        {
            idParts = idParts.Take(idParts.Count - 1).Append("v1").Append(lastPart).ToList();
        }

        // Join back into a string to get the transformed
        return string.Join("_", idParts);
    }

    private static CrpgItemFlags MbToCrpgItemFlags(ItemFlags f) =>
        (CrpgItemFlags)Enum.Parse(typeof(CrpgItemFlags), f.ToString());

    private static CrpgCulture MbToCrpgCulture(BasicCultureObject? culture) => culture == null
        ? CrpgCulture.Neutral // Consider no culture as neutral.
        : (CrpgCulture)Enum.Parse(typeof(CrpgCulture), culture.ToString());

    private static CrpgArmorMaterialType MbToCrpgArmorMaterialType(ArmorComponent.ArmorMaterialTypes t) => t switch
    {
        ArmorComponent.ArmorMaterialTypes.None => CrpgArmorMaterialType.Undefined, // To be consistent with WeaponClass.
        _ => (CrpgArmorMaterialType)Enum.Parse(typeof(CrpgArmorMaterialType), t.ToString()),
    };

    private static CrpgWeaponClass MbToCrpgWeaponClass(WeaponClass wc) =>
        (CrpgWeaponClass)Enum.Parse(typeof(CrpgWeaponClass), wc.ToString());

    private static CrpgWeaponFlags MbToCrpgWeaponFlags(WeaponFlags wf) => (CrpgWeaponFlags)wf;

    private static CrpgDamageType MbToCrpgDamageType(DamageTypes t) => t switch
    {
        DamageTypes.Invalid => CrpgDamageType.Undefined, // To be consistent with WeaponClass.
        _ => (CrpgDamageType)Enum.Parse(typeof(CrpgDamageType), t.ToString()),
    };
    private static void SerializeCrpgItems(IEnumerable<CrpgItem> items, string outputPath)
    {
        var serializer = JsonSerializer.Create(new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Newtonsoft.Json.Formatting.Indented,
            ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() },
            Converters = [new ArrayStringEnumFlagsConverter(), new StringEnumConverter()],
        });

        using StreamWriter s = new(Path.Combine(outputPath, "items.json"));
        serializer.Serialize(s, items);
    }

    private static Task GenerateItemsThumbnail(IEnumerable<ItemObject> mbItems, string outputPath)
    {
        List<Task> createTextureTasks = [];
        foreach (var mbItem in mbItems)
        {
            // Bannerlord generates image thumbnails by loading the 3D texture, spawning a camera and taking a screenshot
            // from it. For each item type, a different camera angle is used. For shields and hand armors, it seems like
            // they are placed on an agent. To do that without spawning an agent, their type is overriden by one that
            // does not need an agent. It was observed that the bow's camera angle and the animal's camera angle were
            // good substitute for respectively shield and hand armor.
            if (ItemRank(mbItem) > 0)
            {
                continue;
            }

            mbItem.Type = mbItem.Type switch
            {
                ItemObject.ItemTypeEnum.Shield => ItemObject.ItemTypeEnum.Bow,
                ItemObject.ItemTypeEnum.HandArmor => ItemObject.ItemTypeEnum.Animal,
                ItemObject.ItemTypeEnum.Musket => ItemObject.ItemTypeEnum.Crossbow,
                ItemObject.ItemTypeEnum.Pistol => ItemObject.ItemTypeEnum.Crossbow,
                _ => mbItem.Type,
            };

            TaskCompletionSource<object?> createTextureTaskSource = new();
            createTextureTasks.Add(createTextureTaskSource.Task);

            // Texture.SaveToFile doesn't accept absolute paths
            ThumbnailCacheManager.Current.CreateTexture(new ItemThumbnailCreationData(mbItem, null, texture =>
            {
                string baseId = mbItem.StringId.Substring(0, mbItem.StringId.Length - "_h0".Length);
                texture.SaveToFile(Path.Combine(outputPath, baseId + ".png"), true);
                createTextureTaskSource.SetResult(null);
            }, null));
        }

        return Task.WhenAll(createTextureTasks);
    }
}

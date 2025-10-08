using Crpg.Module.Api.Models;
using Crpg.Module.Api.Models.Items;
using Crpg.Module.Common;
using Crpg.Module.Common.Network.Armory;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.GUI.Inventory;

public class ItemInfoVM : ViewModel
{
    private readonly CrpgConstants? _constants;
    private readonly CrpgClanArmoryClient? _clanArmory;
    internal CrpgCharacterLoadoutBehaviorClient? UserLoadoutBehavior { get; set; }

    private bool _isVisible;
    private bool _isArmoryItem;
    private bool _isArmoryButtonEnabled;
    private bool _isEquipped;

    private int _userItemId = -1;
    private string _armoryButtonText = string.Empty;
    private string _name = string.Empty;
    private MBBindingList<ItemInfoTupleVM> _tuples = new();
    private MBBindingList<ItemInfoRowVM> _rows = new();
    private ItemObject? _itemObj;
    private ImageIdentifierVM? _imageIdentifier;
    private ItemRankIconVM? _itemRankIcon;
    private ItemArmoryIconVM? _itemArmoryIcon;

    public CrpgUserItemExtended? UserItemExtended { get; set; } = default!;

    private float _positionX;
    [DataSourceProperty]
    public float PositionX
    {
        get => _positionX;
        set => SetField(ref _positionX, value, nameof(PositionX));
    }

    private float _positionY;
    [DataSourceProperty]
    public float PositionY
    {
        get => _positionY;
        set => SetField(ref _positionY, value, nameof(PositionY));
    }

    public void OnOverlayClick()
    {
        InformationManager.DisplayMessage(new InformationMessage("OnOverlayClick()", Colors.White));
        // Close the popup
        IsVisible = false;
    }

    public void ArmoryButtonClick()
    {
        if (_clanArmory == null)
        {
            InformationManager.DisplayMessage(new InformationMessage("_clanArmory is required but not found in current mission", Colors.Red));
            return;
        }

        if (IsArmoryItem && UserItemExtended is not null)
        {
            if (_clanArmory is not null && _clanArmory.GetCrpgUserItemArmoryStatus(UserItemExtended.Id, out var itemArmoryStatus))
            {
                switch (itemArmoryStatus)
                {
                    case CrpgGameArmoryItemStatus.YoursAvailable:
                        _clanArmory.RequestArmoryAction(ClanArmoryActionType.Remove, UserItemExtended);
                        break;
                    case CrpgGameArmoryItemStatus.YoursBorrowed:
                        _clanArmory.RequestArmoryAction(ClanArmoryActionType.Remove, UserItemExtended);
                        break;
                    case CrpgGameArmoryItemStatus.NotYoursAvailible:
                        _clanArmory.RequestArmoryAction(ClanArmoryActionType.Borrow, UserItemExtended);
                        break;
                    case CrpgGameArmoryItemStatus.NotYoursBorrowed:
                        // maybe check clan rank for option to return to armory
                        if (UserLoadoutBehavior?.User?.ClanMembership?.Role > 0)
                        {
                            _clanArmory.RequestArmoryAction(ClanArmoryActionType.Return, UserItemExtended);
                        }

                        break;
                    case CrpgGameArmoryItemStatus.BorrowedByYou:
                        _clanArmory.RequestArmoryAction(ClanArmoryActionType.Return, UserItemExtended);
                        break;
                    default:
                        ArmoryButtonText = "ArmoryStatusInvalid";
                        break;
                }
            }
        }
        else
        {
            _clanArmory.RequestArmoryAction(ClanArmoryActionType.Add, UserItemExtended);
        }
    }

    [DataSourceProperty]
    public MBBindingList<ItemInfoTupleVM> Tuples { get => _tuples; set => SetField(ref _tuples, value, nameof(Tuples)); }

    [DataSourceProperty]
    public MBBindingList<ItemInfoRowVM> Rows { get => _rows; set => SetField(ref _rows, value, nameof(Rows)); }

    [DataSourceProperty]
    public ItemObject? ItemObj { get => _itemObj; set => SetField(ref _itemObj, value, nameof(ItemObj)); }

    [DataSourceProperty]
    public ImageIdentifierVM? ImageIdentifier { get => _imageIdentifier; set => SetField(ref _imageIdentifier, value, nameof(ImageIdentifier)); }

    [DataSourceProperty]
    public string Name { get => _name; set => SetField(ref _name, value, nameof(Name)); }

    [DataSourceProperty]
    public bool IsArmoryItem { get => _isArmoryItem; set => SetField(ref _isArmoryItem, value, nameof(IsArmoryItem)); }

    [DataSourceProperty]
    public bool IsArmoryButtonEnabled { get => _isArmoryButtonEnabled; set => SetField(ref _isArmoryButtonEnabled, value, nameof(IsArmoryButtonEnabled)); }
    [DataSourceProperty]
    public string ArmoryButtonText { get => _armoryButtonText; set => SetField(ref _armoryButtonText, value, nameof(ArmoryButtonText)); }
    [DataSourceProperty]
    public ItemRankIconVM? ItemRankIcon { get => _itemRankIcon; set => SetField(ref _itemRankIcon, value, nameof(ItemRankIcon)); }
    [DataSourceProperty]
    public ItemArmoryIconVM? ItemArmoryIcon { get => _itemArmoryIcon; set => SetField(ref _itemArmoryIcon, value, nameof(ItemArmoryIcon)); }
    [DataSourceProperty]
    public bool IsEquipped { get => _isEquipped; set => SetField(ref _isEquipped, value, nameof(IsEquipped)); }
    [DataSourceProperty]
    public bool IsVisible { get => _isVisible; set => SetField(ref _isVisible, value, nameof(IsVisible)); }

    public ItemInfoVM(ItemObject? item, int userItemId = -1)
    {
        UserLoadoutBehavior = Mission.Current?.GetMissionBehavior<CrpgCharacterLoadoutBehaviorClient>();
        if (UserLoadoutBehavior == null)
        {
            InformationManager.DisplayMessage(new InformationMessage("CrpgCharacterLoadoutBehaviorClient is required but not found in current mission", Colors.Red));
            return;
        }

        _constants = UserLoadoutBehavior.Constants;
        _clanArmory = Mission.Current?.GetMissionBehavior<CrpgClanArmoryClient>();

        _userItemId = userItemId;
        var userItemExtended = UserLoadoutBehavior.GetCrpgUserItem(userItemId);

        if (item == null)
        {
            InformationManager.DisplayMessage(new InformationMessage("ItemInfoVM Constructed; item == null", Colors.Yellow));
            _imageIdentifier = new ImageIdentifierVM(ImageIdentifierType.Item);
            _itemRankIcon = new ItemRankIconVM();
            _itemArmoryIcon = new ItemArmoryIconVM();
            _itemObj = null;
            Name = string.Empty;
            IsVisible = false;
            IsArmoryItem = false;
            IsArmoryButtonEnabled = false;
            UserItemExtended = userItemExtended;
        }
        else
        {
            InformationManager.DisplayMessage(new InformationMessage($"ItemInfoVM Constructed userItemId: {userItemExtended?.Id} itemid: {item.Id}", Colors.Yellow));
            ItemObj = item;
            ImageIdentifier = new ImageIdentifierVM(ItemObj);
            Name = item.Name.ToString();
            UserItemExtended = userItemExtended;
            IsArmoryItem = userItemExtended?.IsArmoryItem ?? false;
            IsArmoryButtonEnabled = userItemExtended != null;

            ItemRankIcon = new ItemRankIconVM(userItemExtended?.Rank ?? 0);

            ItemArmoryIcon = new ItemArmoryIconVM();
            ItemArmoryIcon.UpdateItemArmoryIconFromItem(userItemExtended?.Id ?? -1);

            ArmoryButtonText = string.Empty;
        }
    }

    public override void RefreshValues()
    {
        base.RefreshValues();
    }

    internal void GenerateItemInfo(ItemObject item, int userItemId = -1)
    {
        // InformationManager.DisplayMessage(new InformationMessage("ItemInfoVM GenerateItemInfo", Colors.Yellow));

        if (item == null)
        {
            InformationManager.DisplayMessage(new InformationMessage("ItemInfoVM GenerateItemInfo: item is null!!!", Colors.Yellow));
            return;
        }

        if (UserLoadoutBehavior == null)
        {
            InformationManager.DisplayMessage(new InformationMessage("CrpgCharacterLoadoutBehaviorClient is required but not found in current mission", Colors.Red));
            return;
        }

        var userItemEx = UserLoadoutBehavior?.GetCrpgUserItem(userItemId);
        _userItemId = userItemId;

        ItemObj = item;
        ImageIdentifier = new ImageIdentifierVM(item);
        Name = item.Name.ToString();
        IsVisible = _isVisible;

        GenerateTuplesFromItem();
        GenerateRowsFromTuples();

        if (userItemEx is not null)
        {
            UserItemExtended = userItemEx;
            userItemId = userItemEx.Id;
            IsArmoryItem = userItemEx?.IsArmoryItem ?? false;
            IsArmoryButtonEnabled = userItemEx != null;
            IsArmoryItem = userItemEx?.IsArmoryItem ?? false;
            IsArmoryButtonEnabled = userItemEx is not null;

            ItemRankIcon = new ItemRankIconVM(userItemEx?.Rank ?? 0);

            if (IsArmoryItem)
            {
                if (_clanArmory is not null && _clanArmory.GetCrpgUserItemArmoryStatus(userItemId, out var itemArmoryStatus))
                {
                    switch (itemArmoryStatus)
                    {
                        case CrpgGameArmoryItemStatus.YoursAvailable:
                            ArmoryButtonText = "Remove from armory";
                            break;
                        case CrpgGameArmoryItemStatus.YoursBorrowed:
                            ArmoryButtonText = "Remove from armory";
                            break;
                        case CrpgGameArmoryItemStatus.NotYoursAvailible:
                            ArmoryButtonText = "Borrow from armory";
                            break;
                        case CrpgGameArmoryItemStatus.NotYoursBorrowed:
                            ArmoryButtonText = "Unavailable";
                            // maybe check clan rank for option to return to armory
                            if (UserLoadoutBehavior?.User?.ClanMembership?.Role > 0)
                            {
                                ArmoryButtonText = "Force Return";
                            }

                            break;
                        case CrpgGameArmoryItemStatus.BorrowedByYou:
                            ArmoryButtonText = "Return to armory";
                            break;
                        default:
                            ArmoryButtonText = "ArmoryStatusInvalid";
                            break;
                    }

                    ItemArmoryIcon = new ItemArmoryIconVM((int)itemArmoryStatus);
                }
            }
            else
            {
                ItemArmoryIcon = new ItemArmoryIconVM();
                ArmoryButtonText = "Add to armory";
            }
        }

        IsEquipped = UserLoadoutBehavior?.IsItemEquipped(userItemId) ?? false;
    }

    private void GenerateTuplesFromItem()
    {
        // InformationManager.DisplayMessage(new InformationMessage("ItemInfoVM GenerateTuplesFromItem", Colors.Yellow));
        Tuples.Clear();
        if (ItemObj == null)
        {
            InformationManager.DisplayMessage(new InformationMessage("ItemInfoVM GenerateTuplesFromItem: Item is null!!!", Colors.Yellow));
            return;
        }

        var crpgItem = DataExport.ItemExporter.MbToCrpgItemPub(ItemObj);

        // General Type/Class

        var tup = new ItemInfoTupleVM
        {
            CategoryName = "Type/Class",
        };
        tup.Icons.Add(new ItemInfoIconVM { IconSprite = GetItemTypeIconString(crpgItem), HintText = crpgItem.Type.ToString() });
        tup.Icons.Add(new ItemInfoIconVM { IconSprite = GetItemWeaponClassIconString(crpgItem), HintText = crpgItem.Weapons?.FirstOrDefault()?.Class.ToString() ?? string.Empty });

        Tuples.Add(tup);

        // Armor Only
        if (crpgItem.Armor != null)
        {
            if (crpgItem.Armor.FamilyType > 0)
            {
                string famTypeStr = string.Empty;
                if (crpgItem.Armor.FamilyType == 3)
                {
                    famTypeStr = "EBA";
                }

                tup = new ItemInfoTupleVM
                {
                    CategoryName = "Armor set",
                    ValueText = famTypeStr,
                };
                Tuples.Add(tup);
            }

            // Culture
            tup = new ItemInfoTupleVM
            {
                CategoryName = "Culture",
                ValueText = crpgItem.Culture.ToString(),
            };
            Tuples.Add(tup);

            // Features
            if ((crpgItem.Flags & CrpgItemFlags.UseTeamColor) != 0)
            {
                // Item has the UseTeamColor flag
                tup = new ItemInfoTupleVM
                {
                    CategoryName = "Features",
                };
                tup.Icons.Add(new ItemInfoIconVM { IconSprite = "ui_crpg_icon_white_useteamcolor", HintText = "Use team color" });
                Tuples.Add(tup);
            }

            // Material
            Tuples.Add(new ItemInfoTupleVM
            {
                CategoryName = "Material",
                ValueText = crpgItem.Armor.MaterialType.ToString(),
            });

            // Weight
            Tuples.Add(new ItemInfoTupleVM
            {
                CategoryName = "Weight",
                ValueText = crpgItem.Weight.ToString("F2"),
            });

            // Armor Amounts
            var armorValues = new (string label, int value)[]
            {
                ("Head armor", crpgItem.Armor.HeadArmor),
                ("Body armor", crpgItem.Armor.BodyArmor),
                ("Arm armor",  crpgItem.Armor.ArmArmor),
                ("Leg armor",  crpgItem.Armor.LegArmor),
            };

            foreach (var (label, value) in armorValues)
            {
                if (value > 0)
                {
                    Tuples.Add(new ItemInfoTupleVM
                    {
                        CategoryName = label,
                        ValueText = value.ToString(),
                    });
                }
            }
        }

        // Mount only
        if (crpgItem.Mount != null)
        {
            string familyText = "Horse";
            string familyIcon = "ui_crpg_icon_white_mount";
            if (crpgItem.Mount.FamilyType == 2) // camel
            {
                familyIcon = "ui_crpg_icon_white_camel";
                familyText = "Camel";
            }

            tup = new ItemInfoTupleVM
            {
                CategoryName = "Mount type",
            };
            tup.Icons.Add(new ItemInfoIconVM { IconSprite = familyIcon, HintText = familyText });
            Tuples.Add(tup);

            var mountStats = new (string label, int value)[]
            {
                ("Body length",   crpgItem.Mount.BodyLength),
                ("Charge damage", crpgItem.Mount.ChargeDamage),
                ("Maneuver",      crpgItem.Mount.Maneuver),
                ("Speed",         crpgItem.Mount.Speed),
                ("Hit points",    crpgItem.Mount.HitPoints),
            };

            foreach (var (label, value) in mountStats)
            {
                Tuples.Add(new ItemInfoTupleVM
                {
                    CategoryName = label,
                    ValueText = value.ToString(),
                });
            }
        }

        // Weapons
        if (crpgItem.Weapons != null && crpgItem.Weapons.Count > 0)
        {
            var weaponFeatures = GetWeaponFeatures(crpgItem, ItemObj);
            if (weaponFeatures.Count > 0)
            {
                tup = new ItemInfoTupleVM
                {
                    CategoryName = "Features",
                };

                foreach (var feat in weaponFeatures)
                {
                    tup.Icons.Add(new ItemInfoIconVM
                    {
                        IconSprite = feat.icon,
                        HintText = feat.name, // <-- tooltip text
                    });
                }

                Tuples.Add(tup);
            }
        }

        var iType = crpgItem.Type;
        // Usage for melee weapons
        if (crpgItem.Weapons != null && crpgItem.Weapons.Count > 0 &&
            (iType == CrpgItemType.OneHandedWeapon ||
             iType == CrpgItemType.TwoHandedWeapon ||
             iType == CrpgItemType.Polearm))
        {
            var weapon = crpgItem.Weapons[0];

            Tuples.Add(new ItemInfoTupleVM { CategoryName = "Weight", ValueText = crpgItem.Weight.ToString("F2") });
            Tuples.Add(new ItemInfoTupleVM { CategoryName = "Reach", ValueText = weapon.Length.ToString() });
            Tuples.Add(new ItemInfoTupleVM { CategoryName = "Handling", ValueText = weapon.Handling.ToString() });

            // Add damage stats if present
            AddWeaponStat("Thrust damage", weapon.ThrustDamage, weapon.ThrustDamageType);
            AddWeaponStat("Thrust speed", weapon.ThrustSpeed, skipIfZero: true);
            AddWeaponStat("Swing damage", weapon.SwingDamage, weapon.SwingDamageType);
            AddWeaponStat("Swing speed", weapon.SwingSpeed, skipIfZero: true);
        }

        // Ammo
        if (crpgItem.Weapons != null && crpgItem.Weapons.Count > 0 &&
            (iType == CrpgItemType.Arrows ||
            iType == CrpgItemType.Bolts ||
            iType == CrpgItemType.Bullets))
        {
            var weapon = crpgItem.Weapons[0];

            // Map damage type to brush and suffix
            (string sprite, string suffix, string name) = weapon.ThrustDamageType switch
            {
                CrpgDamageType.Cut => ("ui_crpg_icon_white_cut", "c", "Cut"),
                CrpgDamageType.Pierce => ("ui_crpg_icon_white_pierce", "p", "Pierce"),
                _ => (string.Empty, string.Empty, string.Empty),
            };

            tup = new ItemInfoTupleVM
            {
                CategoryName = "Damage type",
            };

            tup.Icons.Add(new ItemInfoIconVM { IconSprite = sprite, HintText = name });
            Tuples.Add(tup);

            Tuples.Add(new ItemInfoTupleVM
            {
                CategoryName = "Damage",
                ValueText = $"{weapon.ThrustDamage} {suffix}",
            });

            Tuples.Add(new ItemInfoTupleVM
            {
                CategoryName = "Stack weight",
                ValueText = $"{crpgItem.Weight * weapon.StackAmount:F2}",
            });

            Tuples.Add(new ItemInfoTupleVM
            {
                CategoryName = "Ammo",
                ValueText = $"{weapon.StackAmount}",
            });
        }

        // Ranged weapon
        if (crpgItem.Weapons != null && crpgItem.Weapons.Count > 0 &&
           (iType == CrpgItemType.Bow ||
            iType == CrpgItemType.Crossbow ||
            iType == CrpgItemType.Musket ||
            iType == CrpgItemType.Pistol))
        {
            var weapon = crpgItem.Weapons[0];

            // Features

            // Map damage type to brush and suffix
            string suffix = weapon.ThrustDamageType switch
            {
                CrpgDamageType.Cut => "c",
                CrpgDamageType.Pierce => "p",
                _ => string.Empty,
            };

            Tuples.Add(new ItemInfoTupleVM
            {
                CategoryName = "Weight",
                ValueText = crpgItem.Weight.ToString("F2"),
            });

            Tuples.Add(new ItemInfoTupleVM
            {
                CategoryName = "Damage",
                ValueText = $"{weapon.ThrustDamage} {suffix}",
            });

            Tuples.Add(new ItemInfoTupleVM
            {
                CategoryName = "Accuracy",
                ValueText = $"{weapon.Accuracy}",
            });

            Tuples.Add(new ItemInfoTupleVM
            {
                CategoryName = "Missile speed",
                ValueText = $"{weapon.MissileSpeed}",
            });

            Tuples.Add(new ItemInfoTupleVM
            {
                CategoryName = "Reload speed",
                ValueText = $"{weapon.Handling}",
            });
            Tuples.Add(new ItemInfoTupleVM
            {
                CategoryName = "Aim speed",
                ValueText = $"{weapon.ThrustSpeed}",
            });
        }

        // Shield
        if (crpgItem.Weapons != null && crpgItem.Weapons.Count > 0 &&
            (iType == CrpgItemType.Shield))
        {
            var weapon = crpgItem.Weapons[0];

            Tuples.Add(new ItemInfoTupleVM
            {
                CategoryName = "Weight",
                ValueText = crpgItem.Weight.ToString("F2"),
            });
            Tuples.Add(new ItemInfoTupleVM
            {
                CategoryName = "Reach",
                ValueText = weapon.Length.ToString(),
            });
            Tuples.Add(new ItemInfoTupleVM
            {
                CategoryName = "Speed",
                ValueText = weapon.ThrustSpeed.ToString(),
            });
            Tuples.Add(new ItemInfoTupleVM
            {
                CategoryName = "Durability",
                ValueText = weapon.StackAmount.ToString(),
            });
            Tuples.Add(new ItemInfoTupleVM
            {
                CategoryName = "Armor",
                ValueText = weapon.BodyArmor.ToString(),
            });
        }

        // Upkeep
        Tuples.Add(new ItemInfoTupleVM
        {
            CategoryName = "Upkeep",
            ValueText = $"{ComputeAverageRepairCostPerHour(crpgItem.Price):N0} / h",
            IsGoldVisible = true,
        });

        // Price
        Tuples.Add(new ItemInfoTupleVM
        {
            CategoryName = "Price",
            ValueText = $"{crpgItem.Price:N0}",
            IsGoldVisible = true,
        });

        // InformationManager.DisplayMessage(new InformationMessage($"Tuples Generated: {Tuples.Count} ", Colors.Yellow));
    }

    private int ComputeAverageRepairCostPerHour(int price)
    {
        if (_constants != null)
        {
            return (int)Math.Floor(price * _constants.ItemRepairCostPerSecond * 3600 * _constants.ItemBreakChance);
        }

        return 0;
    }

    private void GenerateRowsFromTuples()
    {
        Rows.Clear();

        for (int i = 0; i < Tuples.Count; i += 2)
        {
            var left = Tuples[i];
            ItemInfoTupleVM? right = new() // make dummy right in case no right
            {
                CategoryName = string.Empty,
                ValueText = string.Empty,
                IsGoldVisible = false,
            };

            if (i + 1 < Tuples.Count)
            {
                right = Tuples[i + 1];
            }

            Rows.Add(new ItemInfoRowVM
            {
                Left = left,
                Right = right,
            });
        }

        // InformationManager.DisplayMessage(new InformationMessage($"Rows Generated: {Rows.Count} ", Colors.Yellow));
    }

    private void AddWeaponStat(string category, int value, CrpgDamageType? damageType = null, bool skipIfZero = true)
    {
        if (skipIfZero && value <= 0)
        {
            return;
        }

        string valText = damageType.HasValue ? $"{value} {GetDamageTypeSuffixString(damageType.Value)}" : value.ToString();

        Tuples.Add(new ItemInfoTupleVM
        {
            CategoryName = category,
            ValueText = valText,
        });
    }

    private string GetDamageTypeSuffixString(CrpgDamageType damageType)
    {
        return damageType switch
        {
            CrpgDamageType.Blunt => "b",
            CrpgDamageType.Cut => "c",
            CrpgDamageType.Pierce => "p",
            _ => string.Empty,
        };
    }

    private string GetItemTypeIconString(CrpgItem crpgItem)
    {
        var itemType = crpgItem.Type;
        return itemType switch
        {
            CrpgItemType.Arrows => "ui_crpg_icon_white_ammo",
            CrpgItemType.Banner => "ui_crpg_icon_white_banner",
            CrpgItemType.BodyArmor => "ui_crpg_icon_white_chestarmor",
            CrpgItemType.Bolts => "ui_crpg_icon_white_ammo",
            CrpgItemType.Bow => "ui_crpg_icon_white_ranged",
            CrpgItemType.Bullets => "ui_crpg_icon_white_ammo",
            CrpgItemType.Crossbow => "ui_crpg_icon_white_ranged",
            CrpgItemType.HandArmor => "ui_crpg_icon_white_handarmor",
            CrpgItemType.HeadArmor => "ui_crpg_icon_white_headarmor",
            CrpgItemType.LegArmor => "ui_crpg_icon_white_legarmor",
            CrpgItemType.Mount => "ui_crpg_icon_white_mount",
            CrpgItemType.MountHarness => "ui_crpg_icon_white_mountharness",
            CrpgItemType.Musket => "ui_crpg_icon_white_ranged",
            CrpgItemType.OneHandedWeapon => "ui_crpg_icon_white_onehanded",
            CrpgItemType.Pistol => "ui_crpg_icon_white_ranged",
            CrpgItemType.Polearm => "ui_crpg_icon_white_polearm",
            CrpgItemType.Shield => "ui_crpg_icon_white_shield",
            CrpgItemType.ShoulderArmor => "ui_crpg_icon_white_cape",
            CrpgItemType.Thrown => "ui_crpg_icon_white_thrown",
            CrpgItemType.TwoHandedWeapon => "ui_crpg_icon_white_twohanded",
            CrpgItemType.Undefined => string.Empty,
            _ => string.Empty,
        };
    }

    private List<(string icon, string name)> GetWeaponFeatures(CrpgItem crpgItem, ItemObject item)
    {
        var features = new Dictionary<string, string>(); // icon → name

        if (crpgItem.Weapons == null || crpgItem.Weapons.Count == 0)
        {
            return new List<(string, string)>();
        }

        if (crpgItem.Flags.HasFlag(CrpgItemFlags.DropOnWeaponChange))
        {
            features["ui_crpg_icon_white_droponchange"] = "Drop on weapon change";
        }

        // CrpgWeaponFlags → (icon, name)
        var crpgWeaponFlagMap = new Dictionary<CrpgWeaponFlags, (string icon, string name)>
    {
        { CrpgWeaponFlags.CanCrushThrough, ("ui_crpg_icon_white_crushthrough", "Crush Through") },
        { CrpgWeaponFlags.BonusAgainstShield, ("ui_crpg_icon_white_bonusagainstshield", "Bonus vs Shield") },
        { CrpgWeaponFlags.CanPenetrateShield, ("ui_crpg_icon_white_penetratesshield", "Penetrates Shield") },
        { CrpgWeaponFlags.CanDismount, ("ui_crpg_icon_white_candismount", "Can Dismount") },
        { CrpgWeaponFlags.CanKnockDown, ("ui_crpg_icon_white_knockdown", "Knock Down") },
        { CrpgWeaponFlags.CantReloadOnHorseback, ("ui_crpg_icon_white_cantreloadonhorseback", "Can't Reload on Horseback") },
    };

        // ItemUsage → (icon, name)
        var itemUsageMap = new Dictionary<string, (string icon, string name)>
    {
        { "crossbow_light", ("ui_crpg_icon_white_crossbow_light", "Light Crossbow") },
        { "crossbow", ("ui_crpg_icon_white_crossbow_heavy", "Heavy Crossbow") },
        { "long_bow", ("ui_crpg_icon_white_bow_longbow", "Longbow") },
        { "bow", ("ui_crpg_icon_white_bow", "Bow") },
        { "polearm_bracing", ("ui_crpg_icon_white_brace", "Brace") },
        { "polearm_pike", ("ui_crpg_icon_white_pike", "Pike") },
    };

        // Native checks → icon + name
        var nativeWeaponCheck = new List<Func<WeaponComponentData, (string? icon, string? name)>>
    {
        w => w.WeaponFlags.HasFlag(WeaponFlags.CanHook) ? ("ui_crpg_icon_white_candismount", "Can Dismount") : default,
        w => w.WeaponFlags.HasFlag(WeaponFlags.CanCrushThrough) ? ("ui_crpg_icon_white_crushthrough", "Crush Through") : default,
        w => w.WeaponFlags.HasFlag(WeaponFlags.CanKnockDown) ? ("ui_crpg_icon_white_knockdown", "Knock Down") : default,
        w => w.WeaponClass == WeaponClass.LargeShield ? ("ui_crpg_icon_white_cantuseonhorseback", "Can't Use on Horseback") : default,
    };

        // Process CrpgItem weapons
        foreach (var weapon in crpgItem.Weapons.Where(w => w != null))
        {
            foreach (var kvp in crpgWeaponFlagMap)
            {
                if (weapon.Flags.HasFlag(kvp.Key))
                {
                    features[kvp.Value.icon] = kvp.Value.name;
                }
            }

            if (!string.IsNullOrEmpty(weapon.ItemUsage) && itemUsageMap.TryGetValue(weapon.ItemUsage, out var usage))
            {
                features[usage.icon] = usage.name;
            }
        }

        // Process native ItemObject weapons
        foreach (var weapon in item.Weapons.Where(w => w != null))
        {
            foreach (var check in nativeWeaponCheck)
            {
                var (icon, name) = check(weapon);
                if (icon != null)
                {
                    features[icon] = name ?? icon;
                }
            }
        }

        return features.Select(kvp => (kvp.Key, kvp.Value)).ToList();
    }

    private string GetItemWeaponClassIconString(CrpgItem crpgItem)
    {
        if (crpgItem.Weapons == null || crpgItem.Weapons.Count == 0)
        {
            return string.Empty;
        }

        var weaponClass = crpgItem.Weapons[0].Class;

        return weaponClass switch
        {
            CrpgWeaponClass.Dagger => "ui_crpg_icon_white_dagger",
            CrpgWeaponClass.OneHandedSword => "ui_crpg_icon_white_onehanded_sword",
            CrpgWeaponClass.TwoHandedSword => "ui_crpg_icon_white_twohanded_sword",
            CrpgWeaponClass.OneHandedAxe => "ui_crpg_icon_white_onehanded_axe",
            CrpgWeaponClass.TwoHandedAxe => "ui_crpg_icon_white_twohanded_axe",
            CrpgWeaponClass.Mace => "ui_crpg_icon_white_onehanded_mace",
            CrpgWeaponClass.Pick => "ui_crpg_icon_white_onehanded_mace",
            CrpgWeaponClass.TwoHandedMace => "ui_crpg_icon_white_twohanded_mace",
            CrpgWeaponClass.OneHandedPolearm => "ui_crpg_icon_white_onehanded_polearm",
            CrpgWeaponClass.TwoHandedPolearm => "ui_crpg_icon_white_twohanded_polearm",
            CrpgWeaponClass.LowGripPolearm => "ItemIcon.PolearmLow",
            CrpgWeaponClass.Arrow => "ui_crpg_icon_white_arrow",
            CrpgWeaponClass.Bolt => "ui_crpg_icon_white_bolt",
            CrpgWeaponClass.Cartridge => "ui_crpg_icon_white_bullets",
            CrpgWeaponClass.Bow => "ui_crpg_icon_white_crossbow",
            CrpgWeaponClass.Crossbow => "ui_crpg_icon_white_crossbow",
            CrpgWeaponClass.Stone => "ui_crpg_icon_white_thrown_stone",
            CrpgWeaponClass.Boulder => string.Empty,
            CrpgWeaponClass.ThrowingAxe => "ui_crpg_icon_white_thrown_axe",
            CrpgWeaponClass.ThrowingKnife => "ui_crpg_icon_white_thrown_knife",
            CrpgWeaponClass.Javelin => "ui_crpg_icon_white_thrown_javelin",
            CrpgWeaponClass.Pistol => "ui_crpg_icon_white_musket",
            CrpgWeaponClass.Musket => "ui_crpg_icon_white_musket",
            CrpgWeaponClass.SmallShield => "ui_crpg_icon_white_shield_small",
            CrpgWeaponClass.LargeShield => "ui_crpg_icon_white_shield_large",
            CrpgWeaponClass.Banner => "ui_crpg_icon_white_banner",
            _ => string.Empty,
        };
    }

    public enum ItemFamilyType
    {
        Undefined = 0,
        Horse = 1,
        Camel = 2,
        EBA = 3,
    }
}

using Crpg.Module.Api.Models;
using Crpg.Module.Api.Models.Items;
using Crpg.Module.Common;
using Crpg.Module.Common.Network.Armory;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.ImageIdentifiers;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.GUI.Inventory;

public class ItemInfoVM : ViewModel
{
    private readonly CrpgConstants? _constants;
    private readonly CrpgCharacterLoadoutBehaviorClient? _userLoadout;
    private readonly CrpgClanArmoryClient? _clanArmory;
    private readonly CrpgTeamInventoryClient? _teamInventory;

    public CrpgUserItemExtended? UserItemExtended { get; private set; } = default!;

    internal ItemInfoVM(ItemObject? item, int userItemId = -1)
    {
        _userLoadout = Mission.Current?.GetMissionBehavior<CrpgCharacterLoadoutBehaviorClient>();
        _constants = CrpgCharacterEquipUiHandler.Constants;
        _clanArmory = Mission.Current?.GetMissionBehavior<CrpgClanArmoryClient>();
        _teamInventory = Mission.Current?.GetMissionBehavior<CrpgTeamInventoryClient>();

        if (_teamInventory is not null)
        {
            _teamInventory.OnSingleTeamItemQuantityUpdated += HandleTeamItemQuantityUpdated;
            _teamInventory.OnTeamItemsUpdated += HandleTeamItemsUpdated;
        }

        SetDefaults();

        if (item != null)
        {
            GenerateItemInfo(item, userItemId);
        }
    }

    private void SetDefaults()
    {
        ImageIdentifier = new ItemImageIdentifierVM(null);
        ItemRankIcon = new ItemRankIconVM();
        ItemArmoryIcon = new ItemArmoryIconVM();
        ItemObj = null;
        Name = string.Empty;
        IsVisible = false;
        IsArmoryItem = false;
        IsArmoryButtonVisible = false;
        IsEquipped = false;
        IsQuantityVisible = false;
        ArmoryButtonText = string.Empty;
        QuantityText = string.Empty;
        UserItemExtended = null;
    }

    private void OnOverlayClick() => IsVisible = false;
    private void ExecuteClose() => IsVisible = false;
    private void ArmoryButtonClick()
    {
        if (_clanArmory == null)
        {
            InformationManager.DisplayMessage(new InformationMessage("_clanArmory is required but not found in current mission", Colors.Red));
            return;
        }

        if (IsArmoryItem && UserItemExtended is not null)
        {
            if (_clanArmory.GetCrpgUserItemArmoryStatus(UserItemExtended.Id, out var itemArmoryStatus))
            {
                bool isLeader = _userLoadout?.User?.ClanMembership?.Role == Api.Models.Clans.CrpgClanMemberRole.Leader;

                switch (itemArmoryStatus)
                {
                    case CrpgGameArmoryItemStatus.YoursAvailable:
                    case CrpgGameArmoryItemStatus.YoursBorrowed:
                        _clanArmory.RequestArmoryAction(ClanArmoryActionType.Remove, UserItemExtended);
                        break;
                    case CrpgGameArmoryItemStatus.NotYoursAvailable:
                        _clanArmory.RequestArmoryAction(ClanArmoryActionType.Borrow, UserItemExtended);
                        break;
                    case CrpgGameArmoryItemStatus.NotYoursBorrowed when isLeader:
                        _clanArmory.RequestArmoryAction(ClanArmoryActionType.Return, UserItemExtended);
                        break;
                    case CrpgGameArmoryItemStatus.BorrowedByYou:
                        _clanArmory.RequestArmoryAction(ClanArmoryActionType.Return, UserItemExtended);
                        break;
                }
            }
        }
        else
        {
            _clanArmory.RequestArmoryAction(ClanArmoryActionType.Add, UserItemExtended);
        }

        UpdateArmoryState(UserItemExtended);
    }

    [DataSourceProperty]
    public MBBindingList<ItemInfoTupleVM> Tuples { get; set => SetField(ref field, value, nameof(Tuples)); } = new();
    [DataSourceProperty]
    public MBBindingList<ItemInfoRowVM> Rows { get; set => SetField(ref field, value, nameof(Rows)); } = new();
    [DataSourceProperty]
    public ItemObject? ItemObj { get; set => SetField(ref field, value, nameof(ItemObj)); }
    [DataSourceProperty]
    public ImageIdentifierVM? ImageIdentifier { get; set => SetField(ref field, value, nameof(ImageIdentifier)); }
    [DataSourceProperty]
    public string Name { get; set => SetField(ref field, value, nameof(Name)); } = string.Empty;
    [DataSourceProperty]
    public bool IsArmoryItem { get; set => SetField(ref field, value, nameof(IsArmoryItem)); }
    [DataSourceProperty]
    public bool IsArmoryButtonVisible { get; set => SetField(ref field, value, nameof(IsArmoryButtonVisible)); }
    [DataSourceProperty]
    public string ArmoryButtonText { get; set => SetField(ref field, value, nameof(ArmoryButtonText)); } = string.Empty;
    [DataSourceProperty]
    public ItemRankIconVM? ItemRankIcon { get; set => SetField(ref field, value, nameof(ItemRankIcon)); }
    [DataSourceProperty]
    public ItemArmoryIconVM? ItemArmoryIcon { get; set => SetField(ref field, value, nameof(ItemArmoryIcon)); }
    [DataSourceProperty]
    public bool IsEquipped { get; set => SetField(ref field, value, nameof(IsEquipped)); }
    [DataSourceProperty]
    public bool IsVisible { get; set => SetField(ref field, value, nameof(IsVisible)); }
    [DataSourceProperty]
    public float PositionX { get; set => SetField(ref field, value, nameof(PositionX)); }
    [DataSourceProperty]
    public float PositionY { get; set => SetField(ref field, value, nameof(PositionY)); }
    [DataSourceProperty]
    public int ItemQuantity { get; set => SetField(ref field, value, nameof(ItemQuantity)); }
    [DataSourceProperty]
    public bool IsQuantityVisible { get; set => SetField(ref field, value, nameof(IsQuantityVisible)); }
    [DataSourceProperty]
    public string QuantityText { get; set => SetField(ref field, value, nameof(QuantityText)); } = string.Empty;

    public override void OnFinalize()
    {
        base.OnFinalize();
        if (_teamInventory is not null)
        {
            _teamInventory.OnSingleTeamItemQuantityUpdated -= HandleTeamItemQuantityUpdated;
            _teamInventory.OnTeamItemsUpdated -= HandleTeamItemsUpdated;
        }
    }

    internal void GenerateItemInfo(ItemObject item, int userItemId = -1)
    {
        if (item == null)
        {
            return;
        }

        // Resolve item and user item
        CrpgUserItemExtended? userItemEx = ResolveUserItem(item, userItemId);

        // Set item display data
        ItemObj = item;
        ImageIdentifier = new ItemImageIdentifierVM(item);
        Name = item.Name.ToString();
        IsVisible = true;

        // Set equipped/quantity
        UpdateEquippedAndQuantity(item, userItemId);

        // Set user item data
        UserItemExtended = userItemEx;
        IsArmoryItem = userItemEx?.IsArmoryItem ?? false;
        IsArmoryButtonVisible = userItemEx != null && !CrpgTeamInventoryClient.IsTeamInventoryItem(userItemEx.Id);
        ItemRankIcon = new ItemRankIconVM(userItemEx?.Rank ?? 0);

        // Set armory state
        UpdateArmoryState(userItemEx);

        GenerateTuplesFromItem();
        GenerateRowsFromTuples();
    }

    private void UpdateEquippedAndQuantity(ItemObject item, int userItemId)
    {
        if (CrpgTeamInventoryClient.IsTeamInventoryItem(userItemId))
        {
            var teamItem = _teamInventory?.FindTeamInventoryItem(item.StringId);
            UpdateQuantity(teamItem?.Quantity ?? 0);
            IsEquipped = _teamInventory?.IsTeamItemEquipped(item.StringId) ?? false;
        }
        else if (_userLoadout != null)
        {
            UpdateQuantity(0);
            IsEquipped = _userLoadout.IsItemEquipped(userItemId);
        }
        else
        {
            UpdateQuantity(0);
            IsEquipped = false;
        }
    }

    private void UpdateArmoryState(CrpgUserItemExtended? userItemEx)
    {
        if (userItemEx == null)
        {
            ItemArmoryIcon = new ItemArmoryIconVM();
            ArmoryButtonText = string.Empty;
            return;
        }

        ItemArmoryIcon ??= new ItemArmoryIconVM();
        ItemArmoryIcon.UpdateItemArmoryIconFromItem(userItemEx.Id);

        if (!IsArmoryItem)
        {
            ArmoryButtonText = new TextObject("{=KC9dx156}Add to armory").ToString();
            return;
        }

        if (_clanArmory == null || !_clanArmory.GetCrpgUserItemArmoryStatus(userItemEx.Id, out var status))
        {
            ArmoryButtonText = string.Empty;
            return;
        }

        bool isOfficer = _userLoadout?.User?.ClanMembership?.Role == Api.Models.Clans.CrpgClanMemberRole.Officer;

        ArmoryButtonText = status switch
        {
            CrpgGameArmoryItemStatus.YoursAvailable => new TextObject("{=KC9dx157}Remove from armory").ToString(),
            CrpgGameArmoryItemStatus.YoursBorrowed => new TextObject("{=KC9dx157}Remove from armory").ToString(), // reuse same ID
            CrpgGameArmoryItemStatus.NotYoursAvailable => new TextObject("{=KC9dx158}Borrow from armory").ToString(),
            CrpgGameArmoryItemStatus.NotYoursBorrowed => isOfficer ? new TextObject("{=KC9dx159}Force Return").ToString() : new TextObject("{=KC9dx160}Unavailable").ToString(),
            CrpgGameArmoryItemStatus.BorrowedByYou => new TextObject("{=KC9dx161}Return to armory").ToString(),
            _ => new TextObject("{=KC9dx162}Armory Status Invalid").ToString(),
        };
    }

    private void UpdateQuantity(int quantity)
    {
        ItemQuantity = quantity;
        QuantityText = quantity > 1 ? quantity.ToString() : string.Empty;
        IsQuantityVisible = quantity > 0;
    }

    private CrpgUserItemExtended? ResolveUserItem(ItemObject item, int userItemId)
    {
        if (CrpgTeamInventoryClient.IsTeamInventoryItem(userItemId))
        {
            return new CrpgUserItemExtended
            {
                Id = userItemId,
                ItemId = item.StringId,
                IsArmoryItem = false,
                Rank = CrpgTeamInventoryClient.GetRankFromItemId(item.StringId),
            };
        }

        if (_userLoadout != null)
        {
            return _userLoadout.GetCrpgUserItem(userItemId);
        }

        return null;
    }

    private void HandleTeamItemQuantityUpdated(CrpgTeamInventoryItem item)
    {
        if (ItemObj?.StringId == item.Id)
        {
            UpdateQuantity(item.Quantity);
        }
    }

    private void HandleTeamItemsUpdated()
    {
        if (ItemObj == null)
        {
            return;
        }

        var teamItem = _teamInventory?.FindTeamInventoryItem(ItemObj.StringId);
        if (teamItem != null)
        {
            UpdateQuantity(teamItem.Quantity);
        }
    }

    private void GenerateTuplesFromItem()
    {
        Tuples.Clear();
        if (ItemObj == null || !ItemObj.StringId.StartsWith("crpg_"))
        {
            return;
        }

        var crpgItem = DataExport.ItemExporter.MbToCrpgItemPub(ItemObj);

        AddTypeClassTuple(crpgItem);

        if (crpgItem.Armor != null)
        {
            AddArmorTuples(crpgItem);
        }

        if (crpgItem.Mount != null)
        {
            AddMountTuples(crpgItem);
        }

        if (crpgItem.Weapons?.Count > 0)
        {
            AddWeaponTuples(crpgItem, ItemObj);
        }

        Tuples.Add(new ItemInfoTupleVM
        {
            CategoryName = new TextObject("{=KC9dx163}Upkeep").ToString(),
            ValueText = $"{ComputeAverageRepairCostPerHour(crpgItem.Price):N0} / h",
            IsGoldVisible = true,
        });
        Tuples.Add(new ItemInfoTupleVM
        {
            CategoryName = new TextObject("{=KC9dx164}Price").ToString(),
            ValueText = $"{crpgItem.Price:N0}",
            IsGoldVisible = true,
        });
    }

    private void AddTypeClassTuple(CrpgItem crpgItem)
    {
        var tup = new ItemInfoTupleVM { CategoryName = new TextObject("{=KC9dx165}Type/Class").ToString() };
        tup.Icons.Add(new ItemInfoIconVM { IconSprite = GetItemTypeIconString(crpgItem), HintText = crpgItem.Type.ToString() });
        tup.Icons.Add(new ItemInfoIconVM { IconSprite = GetItemWeaponClassIconString(crpgItem), HintText = crpgItem.Weapons?.FirstOrDefault()?.Class.ToString() ?? string.Empty });
        Tuples.Add(tup);
    }

    private void AddArmorTuples(CrpgItem crpgItem)
    {
        var armor = crpgItem.Armor!;

        if (armor.FamilyType > 0)
        {
            string famTypeStr = (ItemFamilyType)armor.FamilyType == ItemFamilyType.EBA ? "EBA" : string.Empty;
            Tuples.Add(new ItemInfoTupleVM { CategoryName = new TextObject("{=KC9dx166}Armor set").ToString(), ValueText = famTypeStr });
        }

        Tuples.Add(new ItemInfoTupleVM { CategoryName = new TextObject("{=KC9dx167}Culture").ToString(), ValueText = crpgItem.Culture.ToString() });

        if ((crpgItem.Flags & CrpgItemFlags.UseTeamColor) != 0)
        {
            var tup = new ItemInfoTupleVM { CategoryName = new TextObject("{=KC9dx168}Features").ToString() };
            tup.Icons.Add(new ItemInfoIconVM { IconSprite = "ui_crpg_icon_white_useteamcolor", HintText = new TextObject("{=KC9dx169}Use team color").ToString() });
            Tuples.Add(tup);
        }

        Tuples.Add(new ItemInfoTupleVM { CategoryName = new TextObject("{=KC9dx170}Material").ToString(), ValueText = armor.MaterialType.ToString() });
        Tuples.Add(new ItemInfoTupleVM { CategoryName = new TextObject("{=KC9dx171}Weight").ToString(), ValueText = crpgItem.Weight.ToString("F2") });

        var armorValues = new (string label, int value)[]
        {
            (new TextObject("{=KC9dx172}Head armor").ToString(), armor.HeadArmor),
            (new TextObject("{=KC9dx173}Body armor").ToString(), armor.BodyArmor),
            (new TextObject("{=KC9dx174}Arm armor").ToString(),  armor.ArmArmor),
            (new TextObject("{=KC9dx175}Leg armor").ToString(),  armor.LegArmor),
        };

        foreach (var (label, value) in armorValues)
        {
            if (value > 0)
            {
                Tuples.Add(new ItemInfoTupleVM { CategoryName = label, ValueText = value.ToString() });
            }
        }
    }

    private void AddMountTuples(CrpgItem crpgItem)
    {
        var mount = crpgItem.Mount!;
        bool isCamel = (ItemFamilyType)mount.FamilyType == ItemFamilyType.Camel;

        var tup = new ItemInfoTupleVM { CategoryName = new TextObject("{=KC9dx176}Mount type").ToString() };
        tup.Icons.Add(new ItemInfoIconVM
        {
            IconSprite = isCamel ? "ui_crpg_icon_white_camel" : "ui_crpg_icon_white_mount",
            HintText = isCamel ? new TextObject("{=KC9dx177}Camel").ToString() : new TextObject("{=KC9dx178}Horse").ToString(),
        });

        var mountStats = new (string label, int value)[]
        {
            (new TextObject("{=KC9dx179}Body length").ToString(),   mount.BodyLength),
            (new TextObject("{=KC9dx180}Charge damage").ToString(), mount.ChargeDamage),
            (new TextObject("{=KC9dx181}Maneuver").ToString(),      mount.Maneuver),
            (new TextObject("{=KC9dx182}Speed").ToString(),         mount.Speed),
            (new TextObject("{=KC9dx183}Hit points").ToString(),    mount.HitPoints),
        };

        foreach (var (label, value) in mountStats)
        {
            Tuples.Add(new ItemInfoTupleVM { CategoryName = label, ValueText = value.ToString() });
        }
    }

    private void AddWeaponTuples(CrpgItem crpgItem, ItemObject itemObj)
    {
        // Features row applies to all weapon types
        var weaponFeatures = GetWeaponFeatures(crpgItem, itemObj);
        if (weaponFeatures.Count > 0)
        {
            var tup = new ItemInfoTupleVM { CategoryName = new TextObject("{=KC9dx168}Features").ToString() };
            foreach (var feat in weaponFeatures)
            {
                tup.Icons.Add(new ItemInfoIconVM { IconSprite = feat.icon, HintText = feat.name });
            }

            Tuples.Add(tup);
        }

        var iType = crpgItem.Type;

        if (iType is CrpgItemType.OneHandedWeapon or CrpgItemType.TwoHandedWeapon or CrpgItemType.Polearm)
        {
            AddMeleeWeaponTuples(crpgItem);
        }
        else if (iType is CrpgItemType.Arrows or CrpgItemType.Bolts or CrpgItemType.Bullets)
        {
            AddAmmoTuples(crpgItem);
        }
        else if (iType is CrpgItemType.Bow or CrpgItemType.Crossbow or CrpgItemType.Musket or CrpgItemType.Pistol)
        {
            AddRangedWeaponTuples(crpgItem);
        }
        else if (iType == CrpgItemType.Shield)
        {
            AddShieldTuples(crpgItem);
        }
    }

    private void AddMeleeWeaponTuples(CrpgItem crpgItem)
    {
        var weapon = crpgItem.Weapons![0];
        Tuples.Add(new ItemInfoTupleVM { CategoryName = new TextObject("{=KC9dx171}Weight").ToString(), ValueText = crpgItem.Weight.ToString("F2") }); // reuse KC9dx171
        Tuples.Add(new ItemInfoTupleVM { CategoryName = new TextObject("{=KC9dx184}Reach").ToString(), ValueText = weapon.Length.ToString() });
        Tuples.Add(new ItemInfoTupleVM { CategoryName = new TextObject("{=KC9dx185}Handling").ToString(), ValueText = weapon.Handling.ToString() });
        AddWeaponStat(new TextObject("{=KC9dx186}Thrust damage").ToString(), weapon.ThrustDamage, weapon.ThrustDamageType);
        AddWeaponStat(new TextObject("{=KC9dx187}Thrust speed").ToString(), weapon.ThrustSpeed, skipIfZero: true);
        AddWeaponStat(new TextObject("{=KC9dx188}Swing damage").ToString(), weapon.SwingDamage, weapon.SwingDamageType);
        AddWeaponStat(new TextObject("{=KC9dx189}Swing speed").ToString(), weapon.SwingSpeed, skipIfZero: true);
    }

    private void AddAmmoTuples(CrpgItem crpgItem)
    {
        var weapon = crpgItem.Weapons![0];
        (string sprite, string suffix, string name) = weapon.ThrustDamageType switch
        {
            CrpgDamageType.Cut => ("ui_crpg_icon_white_cut", "c", new TextObject("{=KC9dx190}Cut").ToString()),
            CrpgDamageType.Pierce => ("ui_crpg_icon_white_pierce", "p", new TextObject("{=KC9dx191}Pierce").ToString()),
            _ => (string.Empty, string.Empty, string.Empty),
        };

        var tup = new ItemInfoTupleVM { CategoryName = new TextObject("{=KC9dx192}Damage type").ToString() };
        tup.Icons.Add(new ItemInfoIconVM { IconSprite = sprite, HintText = name });
        Tuples.Add(tup);

        Tuples.Add(new ItemInfoTupleVM { CategoryName = new TextObject("{=KC9dx193}Damage").ToString(), ValueText = $"{weapon.ThrustDamage} {suffix}" });
        Tuples.Add(new ItemInfoTupleVM { CategoryName = new TextObject("{=KC9dx194}Stack weight").ToString(), ValueText = $"{crpgItem.Weight * weapon.StackAmount:F2}" });
        Tuples.Add(new ItemInfoTupleVM { CategoryName = new TextObject("{=KC9dx195}Ammo").ToString(), ValueText = $"{weapon.StackAmount}" });
    }

    private void AddRangedWeaponTuples(CrpgItem crpgItem)
    {
        var weapon = crpgItem.Weapons![0];
        string suffix = weapon.ThrustDamageType switch
        {
            CrpgDamageType.Cut => "c",
            CrpgDamageType.Pierce => "p",
            _ => string.Empty,
        };

        Tuples.Add(new ItemInfoTupleVM { CategoryName = new TextObject("{=KC9dx171}Weight").ToString(), ValueText = crpgItem.Weight.ToString("F2") }); // reuse KC9dx171
        Tuples.Add(new ItemInfoTupleVM { CategoryName = new TextObject("{=KC9dx193}Damage").ToString(), ValueText = $"{weapon.ThrustDamage} {suffix}" }); // reuse KC9dx193
        Tuples.Add(new ItemInfoTupleVM { CategoryName = new TextObject("{=KC9dx196}Accuracy").ToString(), ValueText = $"{weapon.Accuracy}" });
        Tuples.Add(new ItemInfoTupleVM { CategoryName = new TextObject("{=KC9dx197}Missile speed").ToString(), ValueText = $"{weapon.MissileSpeed}" });
        Tuples.Add(new ItemInfoTupleVM { CategoryName = new TextObject("{=KC9dx198}Reload speed").ToString(), ValueText = $"{weapon.Handling}" });
        Tuples.Add(new ItemInfoTupleVM { CategoryName = new TextObject("{=KC9dx199}Aim speed").ToString(), ValueText = $"{weapon.ThrustSpeed}" });
    }

    private void AddShieldTuples(CrpgItem crpgItem)
    {
        var weapon = crpgItem.Weapons![0];
        Tuples.Add(new ItemInfoTupleVM { CategoryName = new TextObject("{=KC9dx171}Weight").ToString(), ValueText = crpgItem.Weight.ToString("F2") }); // reuse KC9dx171
        Tuples.Add(new ItemInfoTupleVM { CategoryName = new TextObject("{=KC9dx184}Reach").ToString(), ValueText = weapon.Length.ToString() }); // reuse KC9dx184
        Tuples.Add(new ItemInfoTupleVM { CategoryName = new TextObject("{=KC9dx182}Speed").ToString(), ValueText = weapon.ThrustSpeed.ToString() }); // reuse KC9dx182
        Tuples.Add(new ItemInfoTupleVM { CategoryName = new TextObject("{=KC9dx200}Durability").ToString(), ValueText = weapon.StackAmount.ToString() });
        Tuples.Add(new ItemInfoTupleVM { CategoryName = new TextObject("{=KC9dx201}Armor").ToString(), ValueText = weapon.BodyArmor.ToString() });
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

    private static readonly Dictionary<CrpgWeaponFlags, (string icon, string name)> CrpgWeaponFlagMap = new()
{
    { CrpgWeaponFlags.CanCrushThrough,       ("ui_crpg_icon_white_crushthrough",          new TextObject("{=KC9dx202}Crush Through").ToString()) },
    { CrpgWeaponFlags.BonusAgainstShield,    ("ui_crpg_icon_white_bonusagainstshield",    new TextObject("{=KC9dx203}Bonus vs Shield").ToString()) },
    { CrpgWeaponFlags.CanPenetrateShield,    ("ui_crpg_icon_white_penetratesshield",      new TextObject("{=KC9dx204}Penetrates Shield").ToString()) },
    { CrpgWeaponFlags.CanDismount,           ("ui_crpg_icon_white_candismount",           new TextObject("{=KC9dx205}Can Dismount").ToString()) },
    { CrpgWeaponFlags.CanKnockDown,          ("ui_crpg_icon_white_knockdown",             new TextObject("{=KC9dx206}Knock Down").ToString()) },
    { CrpgWeaponFlags.CantReloadOnHorseback, ("ui_crpg_icon_white_cantreloadonhorseback", new TextObject("{=KC9dx207}Can't Reload on Horseback").ToString()) },
};

    private static readonly Dictionary<string, (string icon, string name)> ItemUsageMap = new()
{
    { "crossbow_light",  ("ui_crpg_icon_white_crossbow_light", new TextObject("{=KC9dx208}Light Crossbow").ToString()) },
    { "crossbow",        ("ui_crpg_icon_white_crossbow_heavy", new TextObject("{=KC9dx209}Heavy Crossbow").ToString()) },
    { "long_bow",        ("ui_crpg_icon_white_bow_longbow",    new TextObject("{=KC9dx210}Longbow").ToString()) },
    { "bow",             ("ui_crpg_icon_white_bow",            new TextObject("{=KC9dx211}Bow").ToString()) },
    { "polearm_bracing", ("ui_crpg_icon_white_brace",          new TextObject("{=KC9dx212}Brace").ToString()) },
    { "polearm_pike",    ("ui_crpg_icon_white_pike",           new TextObject("{=KC9dx213}Pike").ToString()) },
};

    private static readonly List<Func<WeaponComponentData, (string? icon, string? name)>> NativeWeaponChecks =
    [
    w => w.WeaponFlags.HasFlag(WeaponFlags.CanHook) ? ("ui_crpg_icon_white_candismount",      new TextObject("{=KC9dx205}Can Dismount").ToString()) : default,        // reuse KC9dx205
    w => w.WeaponFlags.HasFlag(WeaponFlags.CanCrushThrough) ? ("ui_crpg_icon_white_crushthrough",    new TextObject("{=KC9dx202}Crush Through").ToString()) : default,        // reuse KC9dx202
    w => w.WeaponFlags.HasFlag(WeaponFlags.CanKnockDown) ? ("ui_crpg_icon_white_knockdown",       new TextObject("{=KC9dx206}Knock Down").ToString()) : default,           // reuse KC9dx206
    w => w.WeaponClass == WeaponClass.LargeShield ? ("ui_crpg_icon_white_cantuseonhorseback", new TextObject("{=KC9dx214}Can't Use on Horseback").ToString()) : default,
];

    private List<(string icon, string name)> GetWeaponFeatures(CrpgItem crpgItem, ItemObject item)
    {
        var features = new Dictionary<string, string>(); // icon → name

        if (crpgItem.Weapons == null || crpgItem.Weapons.Count == 0)
        {
            return new List<(string, string)>();
        }

        if (crpgItem.Flags.HasFlag(CrpgItemFlags.DropOnWeaponChange))
        {
            features["ui_crpg_icon_white_droponchange"] = new TextObject("{=KC9dx215}Drop on weapon change").ToString();
        }

        // Process CrpgItem weapons
        foreach (var weapon in crpgItem.Weapons.Where(w => w != null))
        {
            foreach (var kvp in CrpgWeaponFlagMap)
            {
                if (weapon.Flags.HasFlag(kvp.Key))
                {
                    features[kvp.Value.icon] = kvp.Value.name;
                }
            }

            if (!string.IsNullOrEmpty(weapon.ItemUsage) && ItemUsageMap.TryGetValue(weapon.ItemUsage, out var usage))
            {
                features[usage.icon] = usage.name;
            }
        }

        // Process native ItemObject weapons
        foreach (var weapon in item.Weapons.Where(w => w != null))
        {
            foreach (var check in NativeWeaponChecks)
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

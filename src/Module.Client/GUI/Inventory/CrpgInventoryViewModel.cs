using Crpg.Module.Common;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;                   // For Agent, CharacterTableau

namespace Crpg.Module.GUI.Inventory;

public class CrpgInventoryViewModel : ViewModel
{
    private bool _isVisible;
    private CharacterViewModel _characterPreview;

    private ImageIdentifierVM _headArmor;
    private ImageIdentifierVM _capeArmor;
    private ImageIdentifierVM _bodyArmor;
    private ImageIdentifierVM _handArmor;
    private ImageIdentifierVM _legArmor;

    private ImageIdentifierVM _weapon0;
    private ImageIdentifierVM _weapon1;
    private ImageIdentifierVM _weapon2;
    private ImageIdentifierVM _weapon3;
    private ImageIdentifierVM _extraWeaponSlot;

    private ImageIdentifierVM _horse;
    private ImageIdentifierVM _horseArmor;

    [DataSourceProperty]
    public bool IsVisible
    {
        get
        {
            return _isVisible;
        }
        set
        {
            if (value != _isVisible)
            {
                _isVisible = value;
                OnPropertyChangedWithValue(value, "IsVisible");
            }
        }
    }

    [DataSourceProperty]
    public CharacterViewModel CharacterPreview
    {
        get
        {
            return _characterPreview;
        }
        set
        {
            if (value != _characterPreview)
            {
                _characterPreview = value;
                OnPropertyChangedWithValue(value, "CharacterPreview");
            }
        }
    }

    public CrpgInventoryViewModel()
    {

        Agent? agentMain = Agent.Main;
        _characterPreview = new CharacterViewModel();

        _headArmor = new ImageIdentifierVM(ImageIdentifierType.Item);
        _capeArmor = new ImageIdentifierVM(ImageIdentifierType.Item);
        _bodyArmor = new ImageIdentifierVM(ImageIdentifierType.Item);
        _handArmor = new ImageIdentifierVM(ImageIdentifierType.Item);
        _legArmor = new ImageIdentifierVM(ImageIdentifierType.Item);

        _weapon0 = new ImageIdentifierVM(ImageIdentifierType.Item);
        _weapon1 = new ImageIdentifierVM(ImageIdentifierType.Item);
        _weapon2 = new ImageIdentifierVM(ImageIdentifierType.Item);
        _weapon3 = new ImageIdentifierVM(ImageIdentifierType.Item);

        _extraWeaponSlot = new ImageIdentifierVM(ImageIdentifierType.Item);
        _horse = new ImageIdentifierVM(ImageIdentifierType.Item);
        _horseArmor = new ImageIdentifierVM(ImageIdentifierType.Item);

        if (agentMain?.Character != null)
        {
            CharacterPreview.FillFrom(agentMain.Character);
            RefreshCharacterPreview();
        }
        else
        {
            // Optionally log or fallback
            InformationManager.DisplayMessage(new InformationMessage("Agent.Main or its character is null."));
        }
    }

    [DataSourceProperty]
    public ImageIdentifierVM HeadArmor
    {
        get => _headArmor;
        set
        {
            if (value != _headArmor)
            {
                _headArmor = value;
                OnPropertyChangedWithValue<ImageIdentifierVM>(value, nameof(HeadArmor));
                OnPropertyChanged(nameof(ShowDefaultHeadArmorIcon));
            }
        }
    }

    [DataSourceProperty]
    public ImageIdentifierVM CapeArmor
    {
        get => _capeArmor;
        set
        {
            if (value != _capeArmor)
            {
                _capeArmor = value;
                OnPropertyChangedWithValue<ImageIdentifierVM>(value, nameof(CapeArmor));
                OnPropertyChanged(nameof(ShowDefaultCapeArmorIcon));
            }
        }
    }

    [DataSourceProperty]
    public ImageIdentifierVM BodyArmor
    {
        get => _bodyArmor;
        set
        {
            if (value != _bodyArmor)
            {
                _bodyArmor = value;
                OnPropertyChangedWithValue<ImageIdentifierVM>(value, nameof(BodyArmor));
                OnPropertyChanged(nameof(ShowDefaultBodyArmorIcon));
            }
        }
    }

    [DataSourceProperty]
    public ImageIdentifierVM HandArmor
    {
        get => _handArmor;
        set
        {
            if (value != _handArmor)
            {
                _handArmor = value;
                OnPropertyChangedWithValue<ImageIdentifierVM>(value, nameof(HandArmor));
                OnPropertyChanged(nameof(ShowDefaultHandArmorIcon));
            }
        }
    }

    [DataSourceProperty]
    public ImageIdentifierVM LegArmor
    {
        get => _legArmor;
        set
        {
            if (value != _legArmor)
            {
                _legArmor = value;
                OnPropertyChangedWithValue<ImageIdentifierVM>(value, nameof(LegArmor));
                OnPropertyChanged(nameof(ShowDefaultLegArmorIcon));
            }
        }
    }

    [DataSourceProperty]
    public ImageIdentifierVM Weapon0
    {
        get => _weapon0;
        set
        {
            if (value != _weapon0)
            {
                _weapon0 = value;
                OnPropertyChangedWithValue<ImageIdentifierVM>(value, nameof(Weapon0));
                OnPropertyChanged(nameof(ShowDefaultWeapon0Icon));
            }
        }
    }

    [DataSourceProperty]
    public ImageIdentifierVM Weapon1
    {
        get => _weapon1;
        set
        {
            if (value != _weapon1)
            {
                _weapon1 = value;
                OnPropertyChangedWithValue<ImageIdentifierVM>(value, nameof(Weapon1));
                OnPropertyChanged(nameof(ShowDefaultWeapon1Icon));
            }
        }
    }

    [DataSourceProperty]
    public ImageIdentifierVM Weapon2
    {
        get => _weapon2;
        set
        {
            if (value != _weapon2)
            {
                _weapon2 = value;
                OnPropertyChangedWithValue<ImageIdentifierVM>(value, nameof(Weapon2));
                OnPropertyChanged(nameof(ShowDefaultWeapon2Icon));
            }
        }
    }

    [DataSourceProperty]
    public ImageIdentifierVM Weapon3
    {
        get => _weapon3;
        set
        {
            if (value != _weapon3)
            {
                _weapon3 = value;
                OnPropertyChangedWithValue<ImageIdentifierVM>(value, nameof(Weapon3));
                OnPropertyChanged(nameof(ShowDefaultWeapon3Icon));
            }
        }
    }

    [DataSourceProperty]
    public ImageIdentifierVM ExtraWeaponSlot
    {
        get => _extraWeaponSlot;
        set
        {
            if (value != _extraWeaponSlot)
            {
                _extraWeaponSlot = value;
                OnPropertyChangedWithValue<ImageIdentifierVM>(value, nameof(ExtraWeaponSlot));
                OnPropertyChanged(nameof(ShowDefaultExtraWeaponSlotIcon));
            }
        }
    }

    [DataSourceProperty]
    public ImageIdentifierVM Horse
    {
        get => _horse;
        set
        {
            if (value != _horse)
            {
                _horse = value;
                OnPropertyChangedWithValue<ImageIdentifierVM>(value, nameof(Horse));
                OnPropertyChanged(nameof(ShowDefaultHorseIcon));
            }
        }
    }

    [DataSourceProperty]
    public ImageIdentifierVM HorseArmor
    {
        get => _horseArmor;
        set
        {
            if (value != _horseArmor)
            {
                _horseArmor = value;
                OnPropertyChangedWithValue<ImageIdentifierVM>(value, nameof(HorseArmor));
                OnPropertyChanged(nameof(ShowDefaultHorseArmorIcon));
            }
        }
    }

    [DataSourceProperty]
    public bool ShowDefaultWeapon0Icon => string.IsNullOrEmpty(_weapon0?.Id);

    [DataSourceProperty]
    public bool ShowDefaultWeapon1Icon => string.IsNullOrEmpty(_weapon1?.Id);

    [DataSourceProperty]
    public bool ShowDefaultWeapon2Icon => string.IsNullOrEmpty(_weapon2?.Id);

    [DataSourceProperty]
    public bool ShowDefaultWeapon3Icon => string.IsNullOrEmpty(_weapon3?.Id);

    [DataSourceProperty]
    public bool ShowDefaultExtraWeaponSlotIcon => string.IsNullOrEmpty(_extraWeaponSlot?.Id);

    [DataSourceProperty]
    public bool ShowDefaultHorseIcon => string.IsNullOrEmpty(_horse?.Id);

    [DataSourceProperty]
    public bool ShowDefaultHorseArmorIcon => string.IsNullOrEmpty(_horseArmor?.Id);

    [DataSourceProperty]
    public bool ShowDefaultHeadArmorIcon => string.IsNullOrEmpty(_headArmor?.Id);

    [DataSourceProperty]
    public bool ShowDefaultCapeArmorIcon => string.IsNullOrEmpty(_capeArmor?.Id);

    [DataSourceProperty]
    public bool ShowDefaultBodyArmorIcon => string.IsNullOrEmpty(_bodyArmor?.Id);

    [DataSourceProperty]
    public bool ShowDefaultHandArmorIcon => string.IsNullOrEmpty(_handArmor?.Id);

    [DataSourceProperty]
    public bool ShowDefaultLegArmorIcon => string.IsNullOrEmpty(_legArmor?.Id);

    public void RefreshCharacterPreview()
    {
        if (CharacterPreview == null)
        {
            return;
        }

        var crpgPeer = GameNetwork.MyPeer.GetComponent<CrpgPeer>();
        if (crpgPeer == null)
        {
            InformationManager.DisplayMessage(new InformationMessage("crpgPeer is null."));
            return;
        }

        Agent? agent = Agent.Main;

        if (agent == null)
        {
            // Optionally log or fallback
            InformationManager.DisplayMessage(new InformationMessage("Agent.Main or its character is null."));
            return;
        }

        var crpgUser = crpgPeer.User;
        if (crpgUser == null)
        {
            InformationManager.DisplayMessage(new InformationMessage("crpgUser is null."));
            return;
        }

        Equipment equipment = CrpgCharacterBuilder.CreateCharacterEquipment(crpgUser.Character.EquippedItems);

        SetImageFromSpawnEquipment(equipment, EquipmentIndex.Weapon0, image => Weapon0 = image, "Weapon0");
        SetImageFromSpawnEquipment(equipment, EquipmentIndex.Weapon1, image => Weapon1 = image, "Weapon1");
        SetImageFromSpawnEquipment(equipment, EquipmentIndex.Weapon2, image => Weapon2 = image, "Weapon2");
        SetImageFromSpawnEquipment(equipment, EquipmentIndex.Weapon3, image => Weapon3 = image, "Weapon3");

        SetImageFromSpawnEquipment(equipment, EquipmentIndex.Head, image => HeadArmor = image, "HeadArmor");
        SetImageFromSpawnEquipment(equipment, EquipmentIndex.Cape, image => CapeArmor = image, "CapeArmor");
        SetImageFromSpawnEquipment(equipment, EquipmentIndex.Body, image => BodyArmor = image, "BodyArmor");
        SetImageFromSpawnEquipment(equipment, EquipmentIndex.Leg, image => LegArmor = image, "LegArmor");

        SetImageFromSpawnEquipment(equipment, EquipmentIndex.ExtraWeaponSlot, image => Weapon3 = image, "ExtraWeaponSlot");
        SetImageFromSpawnEquipment(equipment, EquipmentIndex.Horse, image => Weapon3 = image, "Horse");
        SetImageFromSpawnEquipment(equipment, EquipmentIndex.HorseHarness, image => Weapon3 = image, "HorseHarness");

        CharacterPreview.EquipmentCode = equipment.CalculateEquipmentCode();

        // CharacterPreview.BannerCodeText = agent?.Origin?.Banner?.Serialize() ?? string.Empty;
    }

    private void SetImageFromSpawnEquipment(Equipment equipment, EquipmentIndex index, Action<ImageIdentifierVM> setter, string label)
    {
        EquipmentElement ee = equipment[index];
        if (!ee.IsEmpty && ee.Item != null)
        {
            setter(new ImageIdentifierVM(ee.Item));
            InformationManager.DisplayMessage(new InformationMessage($"Updated {label} image."));
        }
        else
        {
            InformationManager.DisplayMessage(new InformationMessage($"{label} is empty or invalid."));
        }
    }

    private EquipmentElement? GetEquipmentElementFromMissionWeapon(MissionWeapon missionWeapon)
    {
        if (missionWeapon.IsEmpty)
        {
            return null;
        }

        // You can extract the base item and form a minimal EquipmentElement
        ItemObject item = missionWeapon.Item;
        if (item == null)
        {
            return null;
        }

        // Try to preserve modifiers and usage index if needed
        ItemModifier modifier = missionWeapon.ItemModifier;
        EquipmentElement element = new(item, modifier);
        return element;
    }
}

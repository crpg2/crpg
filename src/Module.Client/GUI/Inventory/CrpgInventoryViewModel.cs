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
    private ImageIdentifierVM _legArmor;

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
        _legArmor = new ImageIdentifierVM(ImageIdentifierType.Item);


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
            }
        }
    }
    public void RefreshCharacterPreview()
    {
        if (CharacterPreview == null)
        {
            return;
        }

        Agent? agent = Agent.Main;

        if (agent == null)
        {
            // Optionally log or fallback
            InformationManager.DisplayMessage(new InformationMessage("Agent.Main or its character is null."));
            return;

        }

        Equipment equipment = new();

        for (int i = 0; i < (int)EquipmentIndex.NumAllWeaponSlots; i++)
        {
            EquipmentIndex index = (EquipmentIndex)i;
            MissionWeapon missionWeapon = agent.Equipment[index];

            // Manually get the EquipmentElement from the MissionWeapon
            EquipmentElement? maybeElement = GetEquipmentElementFromMissionWeapon(missionWeapon);
            if (maybeElement.HasValue)
            {
                equipment[index] = maybeElement.Value;
            }
        }

        for (int i = (int)EquipmentIndex.ArmorItemBeginSlot; i <= (int)EquipmentIndex.ArmorItemEndSlot; i++)
        {
            EquipmentIndex index = (EquipmentIndex)i;
            EquipmentElement ee = agent.SpawnEquipment[index];
            if (!ee.IsEmpty)
            {
                equipment[index] = ee;
            }
        }

        EquipmentElement headArmorElement = equipment[EquipmentIndex.Head];
        if (!headArmorElement.IsEmpty && headArmorElement.Item != null)
        {
            InformationManager.DisplayMessage(new InformationMessage("Updating HeadArmor Image."));
            HeadArmor = new ImageIdentifierVM(headArmorElement.Item);
        }
        else
        {
            InformationManager.DisplayMessage(new InformationMessage("HeadArmor is empty/not item."));
        }

        EquipmentElement capeArmorElement = equipment[EquipmentIndex.Cape];
        if (!capeArmorElement.IsEmpty && capeArmorElement.Item != null)
        {
            InformationManager.DisplayMessage(new InformationMessage("Updating capeArmor Image."));
            CapeArmor = new ImageIdentifierVM(capeArmorElement.Item);
        }
        else
        {
            InformationManager.DisplayMessage(new InformationMessage("capeArmor is empty/not item."));
        }

        EquipmentElement bodyArmorElement = equipment[EquipmentIndex.Body];
        if (!bodyArmorElement.IsEmpty && bodyArmorElement.Item != null)
        {
            InformationManager.DisplayMessage(new InformationMessage("Updating bodyArmor Image."));
            CapeArmor = new ImageIdentifierVM(bodyArmorElement.Item);
        }
        else
        {
            InformationManager.DisplayMessage(new InformationMessage("bodyArmor is empty/not item."));
        }

        EquipmentElement legArmorElement = equipment[EquipmentIndex.Leg];
        if (!legArmorElement.IsEmpty && legArmorElement.Item != null)
        {
            InformationManager.DisplayMessage(new InformationMessage("Updating legArmor Image."));
            CapeArmor = new ImageIdentifierVM(legArmorElement.Item);
        }
        else
        {
            InformationManager.DisplayMessage(new InformationMessage("legArmor is empty/not item."));
        }

        CharacterPreview.EquipmentCode = equipment.CalculateEquipmentCode();
        // CharacterPreview.BannerCodeText = agent?.Origin?.Banner?.Serialize() ?? string.Empty;
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

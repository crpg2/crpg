using System;
using Crpg.Module.Api.Models.Items;
using Crpg.Module.Common.Network;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using System.Collections.Generic;

namespace Crpg.Module.GUI.Inventory
{
    public class EquipmentSlotVM : ViewModel
    {
        private readonly Action<EquipmentSlotVM>? _alternateClick;
        private ImageIdentifierVM? _imageIdentifier;
        private bool _isDragging;
        private string _defaultSprite;
        private CrpgItemSlot _crpgItemSlotIndex;
        private ItemObject? _itemObj;
        private int _userItemId;

        public EquipmentSlotVM(ImageIdentifierVM imageIdentifier, CrpgItemSlot crpgSlot, Action<EquipmentSlotVM>? handleAlternateClick = null)
        {
            _imageIdentifier = imageIdentifier;
            _crpgItemSlotIndex = crpgSlot;
            _defaultSprite = GetDefaultSpriteForSlot(_crpgItemSlotIndex);
            _alternateClick = handleAlternateClick;

            _itemObj = null;
            if (_imageIdentifier != null && _imageIdentifier.IsValid && _imageIdentifier.ImageTypeCode == (int)ImageIdentifierType.Item)
            {
                _itemObj = Game.Current.ObjectManager.GetObject<ItemObject>(_imageIdentifier.Id);
            }

            RefreshValues();
        }

        public static EquipmentIndex ConvertToEquipmentIndex(CrpgItemSlot slot)
        {
            return CrpgToEquipIndex.TryGetValue(slot, out var result) ? result : EquipmentIndex.None;
        }

        public void ExecuteAlternateClick()
        {
            _alternateClick?.Invoke(this);
            ClearItem();
        }

        public void ExecuteDragBegin() => IsDragging = true;
        public void ExecuteDragEnd() => IsDragging = false;

        public void ExecuteTryEquipItem(ViewModel draggedItemVM, int index)
        {
            ItemObject? item = null;

            if (draggedItemVM is InventorySlotVM inventorySlot)
            {
                item = inventorySlot.ItemObj;

                // send request with CrpgItemSlot
                GameNetwork.BeginModuleEventAsClient();
                GameNetwork.WriteMessage(new UserRequestEquipCharacterItem { Slot = CrpgItemSlotIndex, UserItemId = inventorySlot.UserItemId });
                GameNetwork.EndModuleEventAsClient();

                if (Equipment.IsItemFitsToSlot(ConvertToEquipmentIndex(CrpgItemSlotIndex), item))
                {
                    SetItem(new ImageIdentifierVM(item), inventorySlot.UserItemId);
                }
            }
            else if (draggedItemVM is EquipmentSlotVM equipmentSlot)
            {
                item = equipmentSlot.ItemObj;

                if (item == null)
                {
                    InformationManager.DisplayMessage(new InformationMessage($"item is null"));
                    return;
                }

                if (equipmentSlot.CrpgItemSlotIndex == CrpgItemSlotIndex)
                    return;

                // send request with CrpgItemSlot
                GameNetwork.BeginModuleEventAsClient();
                GameNetwork.WriteMessage(new UserRequestEquipCharacterItem { Slot = CrpgItemSlotIndex, UserItemId = equipmentSlot.UserItemId });
                GameNetwork.EndModuleEventAsClient();

                if (Equipment.IsItemFitsToSlot(ConvertToEquipmentIndex(CrpgItemSlotIndex), item))
                {
                    // equipmentSlot.ClearItem();
                    SetItem(new ImageIdentifierVM(item), equipmentSlot.UserItemId);
                }
            }
        }

        /*
                public void SetItem(ImageIdentifierVM? newIdentifier)
                {
                    ImageIdentifier = newIdentifier;
                    _itemObj = (newIdentifier != null && newIdentifier.IsValid && newIdentifier.ImageTypeCode == (int)ImageIdentifierType.Item)
                        ? Game.Current.ObjectManager.GetObject<ItemObject>(newIdentifier.Id)
                        : null;
                    OnPropertyChanged(nameof(ItemObj));
                }
        */
        public void SetItem(ImageIdentifierVM? newIdentifier, int? userItemId = null)
        {
            ImageIdentifier = newIdentifier;

            if (newIdentifier != null && newIdentifier.IsValid)
            {
                _itemObj = Game.Current.ObjectManager.GetObject<ItemObject>(newIdentifier.Id);
            }
            else
            {
                _itemObj = null;
            }

            UserItemId = userItemId ?? 0;

            OnPropertyChanged(nameof(ItemObj));
            OnPropertyChanged(nameof(ImageIdentifier));
            OnPropertyChanged(nameof(UserItemId));
        }

        private static readonly Dictionary<CrpgItemSlot, EquipmentIndex> CrpgToEquipIndex = new()
        {
            [CrpgItemSlot.Head] = EquipmentIndex.Head,
            [CrpgItemSlot.Shoulder] = EquipmentIndex.Cape,
            [CrpgItemSlot.Body] = EquipmentIndex.Body,
            [CrpgItemSlot.Hand] = EquipmentIndex.Gloves,
            [CrpgItemSlot.Leg] = EquipmentIndex.Leg,
            [CrpgItemSlot.MountHarness] = EquipmentIndex.HorseHarness,
            [CrpgItemSlot.Mount] = EquipmentIndex.Horse,
            [CrpgItemSlot.Weapon0] = EquipmentIndex.Weapon0,
            [CrpgItemSlot.Weapon1] = EquipmentIndex.Weapon1,
            [CrpgItemSlot.Weapon2] = EquipmentIndex.Weapon2,
            [CrpgItemSlot.Weapon3] = EquipmentIndex.Weapon3,
            [CrpgItemSlot.WeaponExtra] = EquipmentIndex.ExtraWeaponSlot,
        };

        private string GetDefaultSpriteForSlot(CrpgItemSlot slot)
        {
            return slot switch
            {
                CrpgItemSlot.Head => "General\\EquipmentIcons\\equipment_type_head_armor",
                CrpgItemSlot.Shoulder => "General\\EquipmentIcons\\equipment_type_cape",
                CrpgItemSlot.Body => "General\\EquipmentIcons\\equipment_type_body_armor",
                CrpgItemSlot.Hand => "General\\EquipmentIcons\\equipment_type_hand_armor",
                CrpgItemSlot.Leg => "General\\EquipmentIcons\\equipment_type_leg_armor",
                CrpgItemSlot.Weapon0 => "General\\EquipmentIcons\\equipment_type_default",
                CrpgItemSlot.Weapon1 => "General\\EquipmentIcons\\equipment_type_default",
                CrpgItemSlot.Weapon2 => "General\\EquipmentIcons\\equipment_type_default",
                CrpgItemSlot.Weapon3 => "General\\EquipmentIcons\\equipment_type_default",
                CrpgItemSlot.WeaponExtra => "General\\EquipmentIcons\\equipment_type_banner",
                CrpgItemSlot.Mount => "General\\EquipmentIcons\\equipment_type_mount",
                CrpgItemSlot.MountHarness => "General\\EquipmentIcons\\equipment_type_default",
                _ => "General\\EquipmentIcons\\equipment_type_default",
            };
        }

        public void ClearItem() => SetItem(new ImageIdentifierVM(ImageIdentifierType.Item));

        [DataSourceProperty]
        public ImageIdentifierVM? ImageIdentifier
        {
            get => _imageIdentifier;
            set
            {
                if (_imageIdentifier != value)
                {
                    _imageIdentifier = value;
                    OnPropertyChanged(nameof(ImageIdentifier));
                    OnPropertyChanged(nameof(ShouldShowDefaultIcon));
                    OnPropertyChanged(nameof(CanAcceptDrag));
                }
            }
        }

        [DataSourceProperty]
        public ItemObject? ItemObj => _itemObj;

        [DataSourceProperty]
        public CrpgItemSlot CrpgItemSlotIndex
        {
            get => _crpgItemSlotIndex;
            set
            {
                if (_crpgItemSlotIndex != value)
                {
                    _crpgItemSlotIndex = value;
                    OnPropertyChanged(nameof(CrpgItemSlotIndex));
                    // optionally update default sprite when slot changes
                    DefaultSprite = GetDefaultSpriteForSlot(_crpgItemSlotIndex);
                }
            }
        }

        [DataSourceProperty]
        public int UserItemId
        {
            get => _userItemId;
            set
            {
                if (_userItemId != value)
                {
                    _userItemId = value;
                    OnPropertyChanged(nameof(UserItemId));
                }
            }
        }

        [DataSourceProperty]
        public bool ShouldShowDefaultIcon => (_imageIdentifier == null || !_imageIdentifier.IsValid) || _isDragging;

        public bool IsDragging
        {
            get => _isDragging;
            set
            {
                if (_isDragging != value)
                {
                    _isDragging = value;
                    OnPropertyChanged(nameof(IsDragging));
                    OnPropertyChanged(nameof(ShouldShowDefaultIcon));
                }
            }
        }

        [DataSourceProperty]
        public string DefaultSprite
        {
            get => _defaultSprite;
            set
            {
                if (_defaultSprite != value)
                {
                    _defaultSprite = value;
                    OnPropertyChanged(nameof(DefaultSprite));
                }
            }
        }


        [DataSourceProperty]
        public bool CanAcceptDrag => _imageIdentifier != null && _imageIdentifier.IsValid;
    }
}

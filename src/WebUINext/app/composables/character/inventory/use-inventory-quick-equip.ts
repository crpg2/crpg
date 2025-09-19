import type { ItemSlot } from '~/models/item'
import type { UserItem } from '~/models/user'

import { useInventoryEquipment } from '~/composables/character/inventory/use-inventory-equipment'
import { useCharacterItems } from '~/composables/character/use-character-items'
import { getAvailableSlotsByItem, isWeaponBySlot } from '~/services/item-service'

export const useInventoryQuickEquip = () => {
  const toast = useToast()
  const { t } = useI18n()

  const { onUpdateCharacterItems, equippedItemsBySlot } = useCharacterItems()
  const { getUnEquipItemsLinked, isEquipItemAllowed } = useInventoryEquipment()

  const getTargetSlot = (slots: ItemSlot[]): ItemSlot | undefined => slots
    .filter(slot => isWeaponBySlot(slot) ? !toValue(equippedItemsBySlot)[slot] : true)
    .at(0)

  const onQuickEquip = (item: UserItem) => {
    if (!isEquipItemAllowed(item)) {
      return
    }

    const { slots, warning } = getAvailableSlotsByItem(item.item, toValue(equippedItemsBySlot))

    if (warning) {
      toast.add({
        title: t(warning),
        color: 'warning',
        close: false,
      })
    }

    const targetSlot = getTargetSlot(slots)

    if (targetSlot) {
      onUpdateCharacterItems([{ slot: targetSlot, userItemId: item.id }])
    }
  }

  const onQuickUnEquip = (slot: ItemSlot) => {
    onUpdateCharacterItems(getUnEquipItemsLinked(slot, toValue(equippedItemsBySlot)))
  }

  return {
    onQuickEquip,
    onQuickUnEquip,
  }
}

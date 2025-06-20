import type { EquippedItemsBySlot } from '~/models/character'
import type { ItemSlot } from '~/models/item'
import type { UserItem } from '~/models/user'

import { useInventoryEquipment } from '~/composables/character/inventory/use-inventory-equipment'
import { useCharacterItems } from '~/composables/character/use-character-items'
import { getAvailableSlotsByItem, isWeaponBySlot } from '~/services/item-service'

export const useInventoryQuickEquip = (equippedItemsBySlot: MaybeRefOrGetter<EquippedItemsBySlot>) => {
  const { updateCharacterItems } = useCharacterItems()
  const { getUnEquipItemsLinked, isEquipItemAllowed } = useInventoryEquipment()

  const getTargetSlot = (slots: ItemSlot[]): ItemSlot | undefined => slots
    .filter(slot => isWeaponBySlot(slot) ? !toValue(equippedItemsBySlot)[slot] : true)
    .at(0)

  const onQuickEquip = async (item: UserItem) => {
    if (!isEquipItemAllowed(item)) {
      return
    }

    const availableSlots = getAvailableSlotsByItem(item.item, toValue(equippedItemsBySlot))
    const targetSlot = getTargetSlot(availableSlots)

    if (targetSlot) {
      await updateCharacterItems([{ slot: targetSlot, userItemId: item.id }])
    }
  }

  const onQuickUnEquip = async (slot: ItemSlot) => {
    await updateCharacterItems(getUnEquipItemsLinked(slot, toValue(equippedItemsBySlot)))
  }

  return {
    onQuickEquip,
    onQuickUnEquip,
  }
}

import type { EquippedItemId, EquippedItemsBySlot } from '~/models/character'
import type { ItemSlot } from '~/models/item'
import type { UserItem } from '~/models/user'

import { useInventoryEquipment } from '~/composables/character/inventory/use-inventory-equipment'
import { updateCharacterItems } from '~/services/character-service'
import { getAvailableSlotsByItem, isWeaponBySlot } from '~/services/item-service'
import { useUserStore } from '~/stores/user'

export const useInventoryQuickEquip = (equippedItemsBySlot: MaybeRefOrGetter<EquippedItemsBySlot>) => {
  const { user } = toRefs(useUserStore())

  // TODO:
  // const character = injectStrict(characterKey)

  // const { loadCharacterItems } = injectStrict(characterItemsKey)

  const { getUnEquipItemsLinked, isEquipItemAllowed } = useInventoryEquipment()

  const getTargetSlot = (slots: ItemSlot[]): ItemSlot | undefined => slots
    .filter(slot => isWeaponBySlot(slot) ? !toValue(equippedItemsBySlot)[slot] : true)
    .at(0)

  const updateItems = async (items: EquippedItemId[]) => {
    // await updateCharacterItems(character.value.id, items)
    // await loadCharacterItems(0, { id: character.value.id })
  }

  const onQuickEquip = async (item: UserItem) => {
    if (!isEquipItemAllowed(item, user.value!.id)) {
      return
    }

    const availableSlots = getAvailableSlotsByItem(item.item, toValue(equippedItemsBySlot))
    const targetSlot = getTargetSlot(availableSlots)

    if (targetSlot) {
      await updateItems([{ slot: targetSlot, userItemId: item.id }])
    }
  }

  const onQuickUnEquip = async (slot: ItemSlot) => {
    await updateItems(getUnEquipItemsLinked(slot, toValue(equippedItemsBySlot)))
  }

  return {
    onQuickEquip,
    onQuickUnEquip,
  }
}

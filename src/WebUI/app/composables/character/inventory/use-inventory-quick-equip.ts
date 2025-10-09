import { useI18n, useToast } from '#imports'

import type { ItemSlot } from '~/models/item'
import type { UserItem } from '~/models/user'

import { useCharacterItems } from '~/composables/character/use-character-items'
import { useUser } from '~/composables/user/use-user'
import { getAvailableSlotsByItem, getUnEquipItems, isWeaponBySlot } from '~/services/item-service'

export const useInventoryQuickEquip = () => {
  const { user } = useUser()
  const toast = useToast()
  const { t } = useI18n()

  const { onUpdateCharacterItems, equippedItemsBySlot } = useCharacterItems()

  const onQuickEquip = (item: UserItem) => {
    const { slots, warning } = getAvailableSlotsByItem(item, user.value!.id, equippedItemsBySlot.value)

    if (warning) {
      toast.add({ title: t(warning), color: 'warning', duration: 2000, close: false })
      return
    }

    const targetSlot = slots.find(slot => !isWeaponBySlot(slot) || !(slot in equippedItemsBySlot.value))

    if (targetSlot) {
      onUpdateCharacterItems([{ slot: targetSlot, userItemId: item.id }])
    }
  }

  const onQuickUnEquip = (slot: ItemSlot) => {
    const unEquipItems = getUnEquipItems(slot, equippedItemsBySlot.value)
    if (unEquipItems.length) {
      onUpdateCharacterItems(unEquipItems)
    }
  }

  return {
    onQuickEquip,
    onQuickUnEquip,
  }
}

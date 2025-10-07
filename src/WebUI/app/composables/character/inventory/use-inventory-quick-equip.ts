import type { ItemSlot } from '~/models/item'
import type { UserItem } from '~/models/user'

import { useCharacterItems } from '~/composables/character/use-character-items'
import { useUser } from '~/composables/user/use-user'
import { getAvailableSlotsByItem, getUnEquipItemsLinked, isWeaponBySlot } from '~/services/item-service'

export const useInventoryQuickEquip = () => {
  const { user } = useUser()

  const toast = useToast()
  const { t } = useI18n()

  const { onUpdateCharacterItems, equippedItemsBySlot } = useCharacterItems()

  const getTargetSlot = (slots: ItemSlot[]): ItemSlot | undefined => slots
    .filter(slot => isWeaponBySlot(slot) ? !equippedItemsBySlot.value[slot] : true)
    .at(0)

  const onQuickEquip = (item: UserItem) => {
    const { slots, warning } = getAvailableSlotsByItem(item, user.value!.id, equippedItemsBySlot.value)

    if (warning) {
      toast.add({
        title: t(warning),
        color: 'warning',
        close: false,
      })
      return
    }

    const targetSlot = getTargetSlot(slots)

    if (targetSlot) {
      // TODO:
      onUpdateCharacterItems([{ slot: targetSlot, userItemId: item.id }])
    }
  }

  const onQuickUnEquip = (slot: ItemSlot) => {
    // TODO:
    onUpdateCharacterItems(getUnEquipItemsLinked(slot, equippedItemsBySlot.value))
  }

  return {
    onQuickEquip,
    onQuickUnEquip,
  }
}

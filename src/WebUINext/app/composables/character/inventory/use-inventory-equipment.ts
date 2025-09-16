import type { EquippedItemsBySlot } from '~/models/character'
import type { ItemSlot } from '~/models/item'
import type { UserItem } from '~/models/user'

import { getLinkedSlots } from '~/services/item-service'

export const useInventoryEquipment = () => {
  const { t } = useI18n()
  const toast = useToast()
  const userStore = useUserStore()

  const isEquipItemAllowed = (item: UserItem) => {
    if (item.isBroken) {
      toast.add({
        title: t('character.inventory.item.broken.notify.warning'),
        color: 'warning',
        close: false,
      })
      return false
    }

    if (item.isArmoryItem && userStore.user?.id === item.userId) {
      toast.add({
        title: t('character.inventory.item.clanArmory.inArmory.notify.warning'),
        color: 'warning',
        close: false,
      })
      return false
    }

    return true
  }

  const getUnEquipItemsLinked = (slot: ItemSlot, equippedItemsBySlot: EquippedItemsBySlot) => {
    return [
      { slot, userItemId: null },
      ...getLinkedSlots(slot, equippedItemsBySlot).map(ls => ({
        slot: ls,
        userItemId: null,
      })),
    ]
  }

  return {
    isEquipItemAllowed,
    getUnEquipItemsLinked,
  }
}

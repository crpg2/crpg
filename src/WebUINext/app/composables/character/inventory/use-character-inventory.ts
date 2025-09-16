import { usePageLoading } from '~/composables/app/use-page-loading'
import { useAsyncCallback } from '~/composables/utils/use-async-callback'
import { addItemToClanArmory, removeItemFromClanArmory, returnItemToClanArmory } from '~/services/clan-service'
import { getLinkedSlots } from '~/services/item-service'
import { reforgeUserItem, repairUserItem, sellUserItem, upgradeUserItem } from '~/services/user-service'

import { useCharacterItems } from '../use-character-items'

export const useCharacterInventory = () => {
  const { t } = useI18n()
  const toast = useToast()
  const userStore = useUserStore()
  const { clan } = toRefs(userStore)

  const {
    equippedItemsBySlot,
    characterItems,
    updateCharacterItems,
    loadCharacterItems,
  } = useCharacterItems()

  function _refreshData() {
    return Promise.all([
      userStore.fetchUser(), // update gold
      userStore.fetchUserItems(),
      loadCharacterItems(),
    ])
  }

  const {
    execute: onSellUserItem,
    isLoading: sellingUserItem,
  } = useAsyncCallback(async (userItemId: number) => {
  // unEquip linked slots
    const characterItem = characterItems.value.find(ci => ci.userItem.id === userItemId)
    if (characterItem !== undefined) {
      const linkedItems = getLinkedSlots(characterItem.slot, equippedItemsBySlot.value)
        .map(ls => ({
          slot: ls,
          userItemId: null,
        }))

      if (linkedItems.length) {
        await updateCharacterItems(linkedItems)
      }
    }

    await sellUserItem(userItemId)
    await _refreshData()
    toast.add({
      title: t('character.inventory.item.sell.notify.success'),
      color: 'success',
      close: false,
    })
  })

  const {
    execute: onRepairUserItem,
    isLoading: repairingUserItem,
  } = useAsyncCallback(async (userItemId: number) => {
    await repairUserItem(userItemId)
    await _refreshData()
    toast.add({
      title: t('character.inventory.item.repair.notify.success'),
      color: 'success',
      close: false,
    })
  })

  const {
    execute: onUpgradeUserItem,
    isLoading: upgradingUserItem,
  } = useAsyncCallback(async (userItemId: number) => {
    await upgradeUserItem(userItemId)
    await _refreshData()
    toast.add({
      title: t('character.inventory.item.upgrade.notify.success'),
      color: 'success',
      close: false,
    })
  })

  const {
    execute: onReforgeUserItem,
    isLoading: reforgingUserItem,
  } = useAsyncCallback(async (userItemId: number) => {
    await reforgeUserItem(userItemId)
    await _refreshData()
    toast.add({
      title: t('character.inventory.item.reforge.notify.success'),
      color: 'success',
      close: false,
    })
  })

  const {
    execute: onAddItemToClanArmory,
    isLoading: addingItemToClanArmory,
  } = useAsyncCallback(async (userItemId: number) => {
    if (!clan.value) {
      return
    }
    await addItemToClanArmory(clan.value.id, userItemId)
    await _refreshData()
    toast.add({
      title: t('clan.armory.item.add.notify.success'),
      color: 'success',
      close: false,
    })
  })

  const {
    execute: onReturnToClanArmory,
    isLoading: returningItemToClanArmory,
  } = useAsyncCallback(async (userItemId: number) => {
    if (!clan.value) {
      return
    }
    await returnItemToClanArmory(clan.value.id, userItemId)
    await _refreshData()
    toast.add({
      title: t('clan.armory.item.return.notify.success'),
      color: 'success',
      close: false,
    })
  })

  const {
    execute: onRemoveFromClanArmory,
    isLoading: removingItemToClanArmory,
  } = useAsyncCallback(async (userItemId: number) => {
    if (!clan.value) {
      return
    }
    await removeItemFromClanArmory(clan.value.id, userItemId)
    await _refreshData()
    toast.add({
      title: t('clan.armory.item.remove.notify.success'),
      color: 'success',
      close: false,
    })
  })

  usePageLoading({
    watch: [
      sellingUserItem,
      repairingUserItem,
      upgradingUserItem,
      reforgingUserItem,
      addingItemToClanArmory,
      removingItemToClanArmory,
      returningItemToClanArmory,
    ],
  })

  return {
    onSellUserItem,

    onRepairUserItem,

    onUpgradeUserItem,
    onReforgeUserItem,

    onAddItemToClanArmory,
    onReturnToClanArmory,
    onRemoveFromClanArmory,
  }
}

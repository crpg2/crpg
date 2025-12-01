import { useI18n } from '#imports'

import { useCharacterItems } from '~/composables/character/use-character-items'
import { useUser } from '~/composables/user/use-user'
import { useUserItems } from '~/composables/user/use-user-items'
import { useAsyncCallback } from '~/composables/utils/use-async-callback'
import { addItemToClanArmory, removeItemFromClanArmory, returnItemToClanArmory } from '~/services/clan-service'
import { getLinkedSlots } from '~/services/item-service'
import { reforgeUserItem, repairUserItem, sellUserItem, upgradeUserItem } from '~/services/user-service'
import { objectEntries } from '~/utils/object'

export const useCharacterInventory = () => {
  const { t } = useI18n()
  const { clan, fetchUser } = useUser()
  const { refreshUserItems } = useUserItems()

  const {
    equippedItemsBySlot,
    onUpdateCharacterItems,
    loadCharacterItems,
  } = useCharacterItems()

  function _refreshData() {
    return Promise.all([
      fetchUser(), // update gold
      refreshUserItems(),
      loadCharacterItems(),
    ])
  }

  const [onSellUserItem] = useAsyncCallback(async (userItemId: number) => {
    // unEquip linked slots TODO: move to backend
    const slot = objectEntries(equippedItemsBySlot.value).find(([, userItem]) => userItem.id === userItemId)?.[0]
    if (slot) {
      const linkedItems = getLinkedSlots(slot, equippedItemsBySlot.value).map(slot => ({ slot, userItemId: null }))
      if (linkedItems.length) {
        await onUpdateCharacterItems(linkedItems)
      }
    }

    await sellUserItem(userItemId)
    await _refreshData()
  }, {
    successMessage: t('character.inventory.item.sell.notify.success'),
    pageLoading: true,
  })

  const [onRepairUserItem] = useAsyncCallback(async (userItemId: number) => {
    await repairUserItem(userItemId)
    await _refreshData()
  }, {
    successMessage: t('character.inventory.item.repair.notify.success'),
    pageLoading: true,
  })

  const [onUpgradeUserItem] = useAsyncCallback(async (userItemId: number) => {
    await upgradeUserItem(userItemId)
    await _refreshData()
  }, {
    successMessage: t('character.inventory.item.upgrade.notify.success'),
  })

  const [onReforgeUserItem] = useAsyncCallback(async (userItemId: number) => {
    await reforgeUserItem(userItemId)
    await _refreshData()
  }, {
    successMessage: t('character.inventory.item.reforge.notify.success'),
    pageLoading: true,
  })

  const [onAddItemToClanArmory] = useAsyncCallback(async (userItemId: number) => {
    if (!clan.value) {
      return
    }
    await addItemToClanArmory(clan.value.id, userItemId)
    await _refreshData()
  }, {
    successMessage: t('clan.armory.item.add.notify.success'),
    pageLoading: true,
  })

  const [onReturnToClanArmory] = useAsyncCallback(async (userItemId: number) => {
    if (!clan.value) {
      return
    }
    await returnItemToClanArmory(clan.value.id, userItemId)
    await _refreshData()
  }, {
    successMessage: t('clan.armory.item.return.notify.success'),
    pageLoading: true,
  })

  const [onRemoveFromClanArmory] = useAsyncCallback(async (userItemId: number) => {
    if (!clan.value) {
      return
    }
    await removeItemFromClanArmory(clan.value.id, userItemId)
    await _refreshData()
  }, {
    successMessage: t('clan.armory.item.remove.notify.success'),
    pageLoading: true,
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

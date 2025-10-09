import { useUser } from '~/composables/user/use-user'
import { useUserItems } from '~/composables/user/use-user-items'
import { useAsyncCallback } from '~/composables/utils/use-async-callback'
import { addItemToClanArmory, removeItemFromClanArmory, returnItemToClanArmory } from '~/services/clan-service'
import { getLinkedSlots } from '~/services/item-service'
import { reforgeUserItem, repairUserItem, sellUserItem, upgradeUserItem } from '~/services/user-service'

import { useCharacterItems } from '../use-character-items'

export const useCharacterInventory = () => {
  const { t } = useI18n()
  const { clan, fetchUser } = useUser()

  const {
    equippedItemsBySlot,
    characterItems,
    onUpdateCharacterItems,
    loadCharacterItems,
  } = useCharacterItems()

  const { refreshUserItems } = useUserItems()

  function _refreshData() {
    return Promise.all([
      fetchUser(), // update gold
      refreshUserItems(),
      loadCharacterItems(),
    ])
  }

  const {
    execute: onSellUserItem,
  } = useAsyncCallback(async (userItemId: number) => {
    // unEquip linked slots TODO: move to backend
    const characterItem = characterItems.value.find(ci => ci.userItem.id === userItemId)

    if (characterItem !== undefined) {
      const linkedItems = getLinkedSlots(characterItem.slot, equippedItemsBySlot.value)
        .map(ls => ({
          slot: ls,
          userItemId: null,
        }))

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

  const {
    execute: onRepairUserItem,
  } = useAsyncCallback(async (userItemId: number) => {
    await repairUserItem(userItemId)
    await _refreshData()
  }, {
    successMessage: t('character.inventory.item.repair.notify.success'),
    pageLoading: true,
  })

  const {
    execute: onUpgradeUserItem,
  } = useAsyncCallback(async (userItemId: number) => {
    await upgradeUserItem(userItemId)
    await _refreshData()
  }, {
    successMessage: t('character.inventory.item.upgrade.notify.success'),
  })

  const {
    execute: onReforgeUserItem,
  } = useAsyncCallback(async (userItemId: number) => {
    await reforgeUserItem(userItemId)
    await _refreshData()
  }, {
    successMessage: t('character.inventory.item.reforge.notify.success'),
    pageLoading: true,
  })

  const {
    execute: onAddItemToClanArmory,
  } = useAsyncCallback(async (userItemId: number) => {
    if (!clan.value) {
      return
    }
    await addItemToClanArmory(clan.value.id, userItemId)
    await _refreshData()
  }, {
    successMessage: t('clan.armory.item.add.notify.success'),
    pageLoading: true,
  })

  const {
    execute: onReturnToClanArmory,
  } = useAsyncCallback(async (userItemId: number) => {
    if (!clan.value) {
      return
    }
    await returnItemToClanArmory(clan.value.id, userItemId)
    await _refreshData()
  }, {
    successMessage: t('clan.armory.item.return.notify.success'),
    pageLoading: true,
  })

  const {
    execute: onRemoveFromClanArmory,
  } = useAsyncCallback(async (userItemId: number) => {
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

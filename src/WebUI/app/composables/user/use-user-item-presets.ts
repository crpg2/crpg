import { useI18n } from '#imports'

import type { ItemSlot } from '~/models/item'
import type { UserItemPreset, UserItemsBySlot } from '~/models/user'

import { useAsyncCallback } from '~/composables/utils/use-async-callback'
import { getAsyncData, refreshAsyncData, useAsyncDataCustom } from '~/composables/utils/use-async-data-custom'
import { ITEM_SLOT } from '~/models/item'
import { USER_QUERY_KEYS } from '~/queries'
import {
  createUserItemPreset,
  deleteUserItemPreset,
  getUserItemPresets,
  updateUserItemPreset,
} from '~/services/user-service'

const ALL_ITEM_SLOTS = Object.values(ITEM_SLOT)

export const useUserItemPresetsProvider = () => {
  return useAsyncDataCustom(
    () => USER_QUERY_KEYS.itemPresets(),
    () => getUserItemPresets(),
    {
      default: () => [],
    },
  )
}

export const useUserItemPresets = () => {
  const _key = USER_QUERY_KEYS.itemPresets()
  const userItemPresets = getAsyncData<UserItemPreset[]>(_key)
  const refreshUserItemPresets = refreshAsyncData(_key)

  return {
    userItemPresets,
    refreshUserItemPresets,
  }
}

export const useUserItemPresetActions = () => {
  const { t } = useI18n()
  const { refreshUserItemPresets } = useUserItemPresets()

  const [onCreateUserItemPreset] = useAsyncCallback(
    async (name: string, equippedItemsBySlot: Partial<UserItemsBySlot>) => {
      await createUserItemPreset({
        name,
        slots: mapEquippedItemsToPresetSlots(equippedItemsBySlot),
      })
      await refreshUserItemPresets()
    },
    { successMessage: t('character.inventory.presets.notify.created') },
  )

  const [onUpdateUserItemPreset] = useAsyncCallback(
    async (presetId: number, name: string, equippedItemsBySlot: Partial<UserItemsBySlot>) => {
      await updateUserItemPreset(presetId, {
        name,
        slots: mapEquippedItemsToPresetSlots(equippedItemsBySlot),
      })
      await refreshUserItemPresets()
    },
    { successMessage: t('character.inventory.presets.notify.updated') },
  )

  const [onDeleteUserItemPreset] = useAsyncCallback(
    async (presetId: number) => {
      await deleteUserItemPreset(presetId)
      await refreshUserItemPresets()
    },
    { successMessage: t('character.inventory.presets.notify.deleted') },
  )

  return {
    onCreateUserItemPreset,
    onUpdateUserItemPreset,
    onDeleteUserItemPreset,
  }
}

function mapEquippedItemsToPresetSlots(equippedItemsBySlot: Partial<UserItemsBySlot>): Array<{ slot: ItemSlot, itemId: string | null }> {
  return ALL_ITEM_SLOTS.map(slot => ({
    slot,
    itemId: equippedItemsBySlot[slot]?.item.id ?? null,
  }))
}

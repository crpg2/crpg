import { useI18n } from '#imports'

import type { UserItemPreset, UserItemPresetSlot, UserItemPresetSlotUpdate, UserItemsBySlot } from '~/models/user'

import { useAsyncCallback } from '~/composables/utils/use-async-callback'
import { ITEM_SLOT } from '~/models/item'
import { USER_QUERY_KEYS } from '~/queries'
import {
  createUserItemPreset,
  deleteUserItemPreset,
  getUserItemPresets,
} from '~/services/user-service'

import { useCharacterItems } from '../character/use-character-items'
import { useUserItems } from './use-user-items'

const ALL_ITEM_SLOTS = Object.values(ITEM_SLOT)

export const useUserItemPresets = () => {
  const { userItems } = useUserItems()

  function getUserItemByItemId(itemId: string): number | null {
    return userItems.value.find(userItem =>
      userItem.item.id === itemId
      && !userItem.isBroken
      && !userItem.isArmoryItem)?.id ?? null
  }

  const {
    data: userItemPresets,
    refresh: refreshUserItemPresets,
  } = useAsyncData<UserItemPreset[]>(toCacheKey(USER_QUERY_KEYS.itemPresets()), async () => {
    const presets = await getUserItemPresets()

    const presetsWithAvailability: UserItemPreset[] = presets.map((preset) => {
      const slotsWithAvailability: UserItemPresetSlot[] = preset.slots.map((slot) => {
        return {
          ...slot,
          available: true,
          userItemId: slot.item ? getUserItemByItemId(slot.item.id) : null,
        }
      })

      return {
        id: preset.id,
        name: preset.name,
        slots: slotsWithAvailability,
      }
    })

    return presetsWithAvailability
  }, {
    default: () => [],
  })

  return {
    userItemPresets,
    refreshUserItemPresets,
  }
}

function mapEquippedItemsToPresetSlots(equippedItemsBySlot: Partial<UserItemsBySlot>): UserItemPresetSlotUpdate[] {
  return ALL_ITEM_SLOTS.map(slot => ({
    slot,
    itemId: equippedItemsBySlot[slot]?.item?.id ?? null,
  }))
}

export const useUserItemPresetActions = () => {
  const { t } = useI18n()
  const toast = useToast()
  const { onUpdateCharacterItems } = useCharacterItems()
  const { refreshUserItemPresets } = useUserItemPresets()

  const [onDeleteUserItemPreset] = useAsyncCallback(
    async (presetId: number) => {
      await deleteUserItemPreset(presetId)
      await refreshUserItemPresets()
    },
    { successMessage: t('character.inventory.presets.notify.deleted') },
  )

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

  const [onApplyUserItemPreset] = useAsyncCallback(async (preset: UserItemPreset) => {
    const missingItemsCount = preset.slots.filter(slot => slot.item && !slot.userItemId).length

    if (missingItemsCount > 0) {
      toast.add({
        title: t('character.inventory.presets.notify.missingItems', { count: missingItemsCount }),
        color: 'warning',
      })
    }

    await onUpdateCharacterItems(preset.slots.map(({ slot, userItemId }) => ({ slot, userItemId })))
  }, { successMessage: t('character.inventory.presets.notify.applied') })

  return {
    onCreateUserItemPreset,
    onApplyUserItemPreset,
    onDeleteUserItemPreset,
  }
}

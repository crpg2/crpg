import type { UserItem, UserItemPreset, UserItemPresetSlot, UserItemPresetSlotUpdate, UserItemsBySlot } from '~/models/user'

import { useI18n } from '#imports'
import { useAsyncCallback } from '~/composables/utils/use-async-callback'
import { ITEM_SLOT } from '~/models/item'
import { USER_QUERY_KEYS } from '~/queries'
import {
  createUserItemPreset,
  deleteUserItemPreset,
  getUserItemPresets,
} from '~/services/user-service'

import { useCharacterItems } from '../character/use-character-items'
import { useUser } from './use-user'
import { useUserItems } from './use-user-items'

const getAvailableUserItemByItemId = (userId: number, userItems: UserItem[], itemId: string): number | null =>
  userItems.find(userItem =>
    userItem.item.id === itemId
    && !userItem.isBroken
    && userItem.clanArmoryLender?.user.id !== userId)?.id ?? null

const ALL_ITEM_SLOTS = Object.values(ITEM_SLOT)

function mapEquippedItemsToPresetSlots(equippedItemsBySlot: Partial<UserItemsBySlot>): UserItemPresetSlotUpdate[] {
  return ALL_ITEM_SLOTS.map(slot => ({
    slot,
    itemId: equippedItemsBySlot[slot]?.item?.id ?? null,
  }))
}

export const useUserItemPresets = () => {
  const { userItems } = useUserItems()
  const { user } = useUser()

  const {
    data: presets,
    refresh: refreshUserItemPresets,
  } = useAsyncData(toCacheKey(USER_QUERY_KEYS.itemPresets()), getUserItemPresets, { default: () => [] })

  const userItemPresets = computed(() => {
    const presetsWithAvailability: UserItemPreset[] = presets.value.map((preset) => {
      const slotsWithAvailability: UserItemPresetSlot[] = preset.slots.map((slot) => {
        return {
          ...slot,
          available: true,
          userItemId: slot.item ? getAvailableUserItemByItemId(user.value!.id, userItems.value, slot.item.id) : null,
        }
      })

      return {
        id: preset.id,
        name: preset.name,
        slots: slotsWithAvailability,
      }
    })

    return presetsWithAvailability
  })

  return {
    userItemPresets,
    refreshUserItemPresets,
  }
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

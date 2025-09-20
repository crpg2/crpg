import type { CharacterOverallItemsStats, EquippedItem, EquippedItemId } from '~/models/character'
import type { UserItemsBySlot } from '~/models/user'

import { useAsyncCallback } from '~/composables/utils/use-async-callback'
import { CHARACTER_QUERY_KEYS } from '~/queries'
import {
  checkUpkeepIsHigh,
  computeLongestWeaponLength,
  computeOverallArmor,
  computeOverallAverageRepairCostByHour,
  computeOverallPrice,
  computeOverallWeight,
  getCharacterItems,
  updateCharacterItems,
} from '~/services/character-service'

import { useAsyncDataCustom } from '../utils/use-async-data-custom'
import { useCharacter } from './use-character'

export const useCharacterItemsProvider = () => {
  const { characterId } = useCharacter()
  return useAsyncDataCustom(
    CHARACTER_QUERY_KEYS.items(characterId.value),
    () => getCharacterItems(characterId.value),
    {
      default: () => [],
    },
  )
}

export const useCharacterItems = () => {
  const userStore = useUserStore()
  const { characterId } = useCharacter()

  const _key = CHARACTER_QUERY_KEYS.items(characterId.value)
  const characterItems = getAsyncData<EquippedItem[]>(_key)
  const loadCharacterItems = refreshAsyncData(_key)

  const equippedItemsBySlot = computed(() => {
    return characterItems.value.reduce((out, ei) => {
      out[ei.slot] = ei.userItem
      return out
    }, {} as UserItemsBySlot)
  })

  const [onUpdateCharacterItems, updatingCharacterItems] = useAsyncCallback(
    async (itemIds: EquippedItemId[]) => {
      await updateCharacterItems(toValue(characterId), itemIds)
      await loadCharacterItems()
    },
    {
      pageLoading: true,
    },
  )

  const itemsOverallStats = computed<CharacterOverallItemsStats>(() => {
    const items = characterItems.value.map(ei => ei.userItem.item)
    return {
      averageRepairCostByHour: computeOverallAverageRepairCostByHour(items),
      longestWeaponLength: computeLongestWeaponLength(items),
      price: computeOverallPrice(items),
      weight: computeOverallWeight(items),
      ...computeOverallArmor(items),
    }
  })

  const equippedItemIds = computed(() => characterItems.value.map(ei => ei.userItem.id))

  const upkeepIsHigh = computed(() => checkUpkeepIsHigh(userStore.user!.gold, itemsOverallStats.value.averageRepairCostByHour))

  return {
    characterItems,
    equippedItemsBySlot,
    itemsOverallStats,
    equippedItemIds,

    loadCharacterItems,
    onUpdateCharacterItems,
    updatingCharacterItems,

    upkeepIsHigh,
  }
}

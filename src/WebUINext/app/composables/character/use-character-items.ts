import type { CharacterOverallItemsStats, EquippedItemId } from '~/models/character'
import type { UserItemsBySlot } from '~/models/user'

import { useAsyncCallback } from '~/composables/utils/use-async-callback'
import { useAsyncStateWithPoll } from '~/composables/utils/use-async-state'
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
import { pollCharacterItemsSymbol } from '~/symbols'

import { useCharacter } from './use-character'

export const useCharacterItems = () => {
  const { characterId } = useCharacter()
  const userStore = useUserStore()

  const {
    state: characterItems,
    execute: loadCharacterItems,
    isLoading: loadingCharacterItems,
  } = useAsyncState(
    () => getCharacterItems(toValue(characterId)),
    [],
    {
      // pollKey: pollCharacterItemsSymbol,
      // pageLoading: true,
    },
  )

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
      // pageLoading: true,
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
    loadingCharacterItems,
    onUpdateCharacterItems,
    updatingCharacterItems,

    upkeepIsHigh,
  }
}

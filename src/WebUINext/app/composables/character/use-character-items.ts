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

export const useCharacterItems = (characterId: MaybeRefOrGetter<number>) => {
  const userStore = useUserStore()

  const {
    state: characterItems,
    execute: loadCharacterItems,
    isLoading: loadingCharacterItems,
  } = useAsyncStateWithPoll(
    () => getCharacterItems(toValue(characterId)),
    [],
    { pollKey: pollCharacterItemsSymbol, pageLoading: true },
  )

  const {
    execute: onUpdateCharacterItems,
    isLoading: updatingCharacterItems,
  } = useAsyncCallback(
    async (itemIds: EquippedItemId[]) => {
      characterItems.value = await updateCharacterItems(toValue(characterId), itemIds)
    },
    {
      pageLoading: true,
    },
  )

  const equippedItemsBySlot = computed<UserItemsBySlot>(() =>
    characterItems.value.reduce((out, ei) => {
      out[ei.slot] = ei.userItem
      return out
    }, {} as UserItemsBySlot),
  )

  const itemsOverallStats = computed((): CharacterOverallItemsStats => {
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

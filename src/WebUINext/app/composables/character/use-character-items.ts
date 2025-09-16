import type { CharacterOverallItemsStats, EquippedItem, EquippedItemId } from '~/models/character'
import type { UserItemsBySlot } from '~/models/user'

import { usePageLoading } from '~/composables/app/use-page-loading'
import { useAsyncCallback } from '~/composables/utils/use-async-callback'
import { usePollInterval } from '~/composables/utils/use-poll-interval'
import {
  updateCharacterItems as _updateCharacterItems,
  checkUpkeepIsHigh,
  computeLongestWeaponLength,
  computeOverallArmor,
  computeOverallAverageRepairCostByHour,
  computeOverallPrice,
  computeOverallWeight,
  getCharacterItems,
} from '~/services/character-service'
import { pollCharacterItemsSymbol } from '~/symbols'

interface CharacterItemsContext {
  characterItems: Ref<EquippedItem[]>
  loadCharacterItems: () => Promise<EquippedItem[]>
  loadingCharacterItems: Ref<boolean>
  updateCharacterItems: (data: EquippedItemId[]) => Promise<void>
  updatingCharacterItems: Ref<boolean>
}

const characterItemsKey: InjectionKey<CharacterItemsContext> = Symbol('CharacterItems')

export const useCharacterItemsProvider = (characterId: MaybeRefOrGetter<number>) => {
  const {
    state: characterItems,
    execute: loadCharacterItems,
    isLoading: loadingCharacterItems,
  } = useAsyncState(
    () => getCharacterItems(toValue(characterId)),
    [],
    { immediate: true },
  )

  usePollInterval(
    {
      key: pollCharacterItemsSymbol,
      fn: loadCharacterItems,
    },
  )

  usePageLoading({
    watch: [loadingCharacterItems],
  })

  const {
    execute: updateCharacterItems,
    isLoading: updatingCharacterItems,
  } = useAsyncCallback(
    async (itemIds: EquippedItemId[]) => {
      characterItems.value = await _updateCharacterItems(toValue(characterId), itemIds)
    },
  )

  provide(characterItemsKey, {
    characterItems,
    loadCharacterItems,
    loadingCharacterItems,
    updateCharacterItems,
    updatingCharacterItems,
  })

  return {
    loadCharacterItems,
    updateCharacterItems,
    loadingCharacterItems,
  }
}

export const useCharacterItems = () => {
  const userStore = useUserStore()

  const {
    characterItems,
    loadCharacterItems,
    updateCharacterItems,
    loadingCharacterItems,
    updatingCharacterItems,
  } = injectStrict(characterItemsKey)

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
    updateCharacterItems,
    updatingCharacterItems,

    upkeepIsHigh,
  }
}

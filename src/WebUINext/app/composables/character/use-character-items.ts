import type { CharacterOverallItemsStats, EquippedItem, EquippedItemId } from '~/models/character'
import type { UserItemsBySlot } from '~/models/user'

import {
  computeLongestWeaponLength,
  computeOverallArmor,
  computeOverallAverageRepairCostByHour,
  computeOverallPrice,
  computeOverallWeight,
} from '~/services/character-service'

interface CharacterItemsContext {
  characterItems: Ref<EquippedItem[]>
  loadCharacterItems: () => Promise<EquippedItem[]>
  loadingCharacterItems: Ref<boolean>
  updateCharacterItems: (data: EquippedItemId[]) => Promise<void>
  updatingCharacterItems: Ref<boolean>
}

const characterItemsKey: InjectionKey<CharacterItemsContext> = Symbol('CharacterItems')

export const useCharacterItemsProvider = (ctx: CharacterItemsContext) => {
  provide(characterItemsKey, ctx)
}

export const useCharacterItems = () => {
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

  return {
    characterItems,
    equippedItemsBySlot,
    itemsOverallStats,
    equippedItemIds,

    loadCharacterItems,
    loadingCharacterItems,
    updateCharacterItems,
    updatingCharacterItems,
  }
}

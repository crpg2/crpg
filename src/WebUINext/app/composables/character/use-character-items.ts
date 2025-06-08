import type { CharacterOverallItemsStats, EquippedItem } from '~/models/character'
import type { UserItemsBySlot } from '~/models/user'

import { computeLongestWeaponLength, computeOverallArmor, computeOverallAverageRepairCostByHour, computeOverallPrice, computeOverallWeight } from '~/services/character-service'

const characterItemsKey: InjectionKey<Ref<EquippedItem[]>> = Symbol('CharacterItems')

export const useCharacterItemsProvider = (characterItems: Ref<EquippedItem[]>) => {
  provide(characterItemsKey, characterItems)
}

export const useCharacterItems = () => {
  const characterItems = injectStrict(characterItemsKey)

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

  return {
    characterItems,
    equippedItemsBySlot,
    itemsOverallStats,
  }
}

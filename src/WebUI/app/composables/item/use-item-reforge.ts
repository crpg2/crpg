import { computed, toValue } from 'vue'

import { useUser } from '~/composables/user/use-user'
import { getReforgeCostByRank, reforgeCostByRank } from '~/services/item-service'

export interface ReforgeCost {
  points: number
  cost: number
}

export const useItemReforge = (itemRank: MaybeRefOrGetter<number>) => {
  const { user } = useUser()

  const reforgeCost = computed(() => getReforgeCostByRank(toValue(itemRank)))

  const reforgeCostTable = computed(() => Object.entries(reforgeCostByRank)
    .slice(1) // + 0 not needed
    .map<ReforgeCost>(([points, cost]) => ({
      points: Number(points),
      cost,
    })))

  const validation = computed(() => ({
    gold: user.value!.gold > (reforgeCost.value ?? 0),
    rank: itemRank !== 0,
  }))

  const canReforge = computed(() => validation.value.rank && validation.value.gold)

  return {
    canReforge,
    reforgeCost,
    reforgeCostTable,
    validation,
  }
}

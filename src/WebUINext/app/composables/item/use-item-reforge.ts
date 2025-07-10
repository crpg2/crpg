import { reforgeCostByRank } from '~/services/item-service'
import { useUserStore } from '~/stores/user'

export interface ReforgeCost {
  points: number
  cost: number
}

export const useItemReforge = (itemRank: number) => {
  const userStore = useUserStore()

  const reforgeCost = reforgeCostByRank[itemRank]

  const reforgeCostTable = computed(() => Object.entries(reforgeCostByRank)
    .slice(1) // + 0 not needed
    .map<ReforgeCost>(([points, cost]) => ({
      points: Number(points),
      cost,
    })))

  const validation = computed(() => ({
    gold: userStore.user!.gold > (reforgeCost ?? 0),
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

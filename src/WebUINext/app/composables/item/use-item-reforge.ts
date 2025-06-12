import type { ItemFlat } from '~/models/item'

import { reforgeCostByRank } from '~/services/item-service'
import { useUserStore } from '~/stores/user'

export const useItemReforge = (item: ItemFlat) => {
  const userStore = useUserStore()

  const reforgeCost = computed(() => reforgeCostByRank[item.rank])

  const reforgeCostTable = computed(() => Object.entries(reforgeCostByRank).slice(1))

  const validation = computed(() => ({
    gold: userStore.user!.gold > reforgeCost.value,
    rank: item.rank !== 0,
    // exist: !userStore.userItems.some(ui => ui.item.baseId === item.baseId && ui.item.rank === 0),
  }))

  const canReforge = computed(() => validation.value.rank && validation.value.gold)

  return {
    canReforge,
    reforgeCost,
    reforgeCostTable,
    validation,
  }
}

import { clamp } from 'es-toolkit'

import type { AggregationConfig } from '~/services/item-search-service/aggregations'

import { getItemUpgrades, getRelativeEntries } from '~/services/item-service'
import { useUserStore } from '~/stores/user'

export interface ItemUpgradesOptions {
  item: {
    id: string
    baseId: string
    rank: number
  }
  aggregationConfig: AggregationConfig
}

export const useItemUpgrades = ({
  item,
  aggregationConfig,
}: ItemUpgradesOptions) => {
  const userStore = useUserStore()

  // TODO:
  const { state: itemUpgrades, isLoading } = useAsyncState(() => getItemUpgrades(item.baseId), [])

  const baseItem = computed(() => itemUpgrades.value.find(iu => iu.rank === 0))

  const nextItem = computed(() => itemUpgrades.value[clamp(itemUpgrades.value.findIndex(iu => iu.id === item.id) + 1, 0, 3)])

  // TODO: Что это
  const relativeEntries = computed(() => baseItem.value ? getRelativeEntries(baseItem.value, aggregationConfig) : {})

  const validation = computed(() => ({
    maxRank: item.rank !== 3,
    points: userStore.user!.heirloomPoints > 0,
  }))

  const canUpgrade = computed(() => validation.value.points && validation.value.maxRank)

  return {
    baseItem,
    canUpgrade,
    isLoading,
    itemUpgrades,
    nextItem,
    relativeEntries,
    validation,
  }
}

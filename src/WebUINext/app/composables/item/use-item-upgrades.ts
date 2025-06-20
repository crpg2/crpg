import { clamp } from 'es-toolkit'

import type { ItemFlat } from '~/models/item'
import type { AggregationConfig } from '~/models/item-search'

import { getItemUpgrades, getRelativeEntries } from '~/services/item-service'
import { useUserStore } from '~/stores/user'

export const useItemUpgrades = (item: ItemFlat, cols: AggregationConfig) => {
  const userStore = useUserStore()

  const { isLoading, state: itemUpgrades } = useAsyncState(() => getItemUpgrades(item), [])

  const baseItem = computed(() => itemUpgrades.value.find(iu => iu.rank === 0)!)

  const nextItem = computed(
    () => itemUpgrades.value[clamp(itemUpgrades.value.findIndex(iu => iu.id === item.id) + 1, 0, 3)],
  )

  const relativeEntries = computed(() => getRelativeEntries(baseItem.value, cols))

  const validation = computed(() => ({
    maxRank: item.rank !== 3,
    points: userStore.user!.heirloomPoints > 0,
    // exist: !userStore.userItems.some(
    //   ui => ui.item.baseId === nextItem.value?.baseId && ui.item.rank === nextItem.value?.rank
    // ),
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

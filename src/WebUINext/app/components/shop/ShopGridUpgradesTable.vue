<script setup lang="ts">
import type { ItemFlat } from '~/models/item'
import type { AggregationConfig } from '~/services/item-search-service/aggregations'

import { useItemUpgrades } from '~/composables/item/use-item-upgrades'

const {
  item,
  aggregationConfig,
} = defineProps<{
  item: ItemFlat
  aggregationConfig: AggregationConfig
}>()

const {
  isLoadingitemUpgrades,
  itemUpgrades,
  relativeEntries,
} = useItemUpgrades({
  item: {
    baseId: item.baseId,
    id: item.id,
    rank: item.rank,
  },
  aggregationConfig,
})
</script>

<template>
  <div class="relative min-h-[280px]">
    <UiLoading :active="isLoadingitemUpgrades" />
    <ItemTableUpgrades
      v-if="!isLoadingitemUpgrades && itemUpgrades.length"
      :items="itemUpgrades.toSpliced(0, 1)"
      :aggregation-config
      :compare-items-result="relativeEntries"
    />
  </div>
</template>

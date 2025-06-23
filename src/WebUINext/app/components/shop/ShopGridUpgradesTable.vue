<script setup lang="ts">
import type { TableColumn } from '@nuxt/ui'
// import type { AggregationConfig } from '~/models/item-search'

import { ItemParam, ShopGridItemMedia } from '#components'

import type { ItemFlat } from '~/models/item'
import type { AggregationConfig, AggregationOptions } from '~/services/item-search-service/aggregations'

import { useItemUpgrades } from '~/composables/item/use-item-upgrades'
import { ItemCompareMode } from '~/models/item'

const {
  item,
  aggregationConfig,
} = defineProps<{
  item: ItemFlat
  aggregationConfig: AggregationConfig
}>()

const { t, n } = useI18n()

const {
  isLoading,
  itemUpgrades,
  // relativeEntries
} = useItemUpgrades(item, // cols
)

function createTableColumn(key: keyof ItemFlat, options: AggregationOptions): TableColumn<ItemFlat> {
  return {
    accessorKey: key,
    header: '',
    cell: ({ row }) => h(ItemParam, {
      field: key,
      item: row.original,
      // bestValue: compareItemsResult !== null ? compareItemsResult[field] : undefined,
      // isCompare: isCompareMode.value
    }),
    meta: {
      class: {
        td: 'min-w-[140px]',
        th: 'min-w-[140px]',
      },
    },
  }
}

const columns = computed<TableColumn<ItemFlat>[]>(() => [
  {
    id: 'fill',
    meta: {
      class: {
        td: 'min-w-[60px]',
      },
    },
  },
  {
    accessorKey: 'name',
    cell: ({ row }) => h(ShopGridItemMedia, {
      item: row.original,
      showTier: true,
    }),
    meta: {
      class: {
        td: 'max-w-[320px]',
        th: 'max-w-[320px]',
      },
    },
  },
  ...Object.entries(aggregationConfig).map(([key, config]) => createTableColumn(key as keyof ItemFlat, config)),
],
)
</script>

<template>
  <div class="relative">
    <UiLoading :active="isLoading" />

    <UTable
      :data="itemUpgrades.slice(1)"
      :columns
      :ui="{
        thead: 'hidden',
      }"
    >
      <template #empty>
        <UiResultNotFound />
      </template>
    </UTable>
  </div>
</template>

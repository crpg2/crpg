<script setup lang="ts">
import type { TableColumn } from '@nuxt/ui'
import type { RowSelectionState } from '@tanstack/vue-table'

import { AppCoin, ItemParam, ItemTableMedia } from '#components'

import type { CompareItemsResult, ItemFlat } from '~/models/item'
import type { AggregationConfig } from '~/services/item-search-service/aggregations'

import { ITEM_COMPARE_MODE } from '~/models/item'
import { getColumnVisibility } from '~/services/item-search-service'

const {
  items,
  aggregationConfig,
  compareItemsResult,
  loading = false,
  currentRank,
} = defineProps<{
  items: ItemFlat[]
  aggregationConfig: AggregationConfig
  compareItemsResult: CompareItemsResult
  loading?: boolean
  currentRank?: number
}>()

const { t, n } = useI18n()

function createTableColumn(key: keyof ItemFlat): TableColumn<ItemFlat> {
  return {
    accessorKey: key,
    header: ({ header }) => t(`item.aggregations.${header.id}.title`),
    cell: ({ row }) => h(ItemParam, {
      field: key,
      item: row.original,
      isCompare: true,
      compareMode: ITEM_COMPARE_MODE.Relative,
      relativeValue: compareItemsResult[key]!,
    }, {
      ...(key === 'upkeep' && { default: ({ rawBuckets }: { rawBuckets: number }) => h(AppCoin, { value: t('item.format.upkeep', { upkeep: n(rawBuckets) }) }) }),
      ...(key === 'price' && { default: ({ rawBuckets }: { rawBuckets: number }) => h(AppCoin, { value: rawBuckets }) }),
    }),
  }
}

const columns = computed<TableColumn<ItemFlat>[]>(() => [
  {
    accessorKey: 'name',
    header: '',
    cell: ({ row }) => h(ItemTableMedia, {
      item: row.original,
      showTier: true,
    }),
  },
  ...objectKeys(aggregationConfig).map(createTableColumn),
])

const columnVisibility = computed(() => getColumnVisibility(aggregationConfig))

const rowSelection = ref<RowSelectionState>({
  ...(currentRank !== undefined && {
    [currentRank]: true,
  }),
})
</script>

<template>
  <UTable
    v-model:column-visibility="columnVisibility"
    v-model:row-selection="rowSelection"
    :data="items"
    :loading
    :columns
    :ui="{
      root: 'overflow-visible',
    }"
  >
    <template #empty>
      <UiResultNotFound />
    </template>
  </UTable>
</template>

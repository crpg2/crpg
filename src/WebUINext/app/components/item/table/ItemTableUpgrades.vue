<script setup lang="ts">
import type { TableColumn } from '@nuxt/ui'
import type { RowSelectionState, VisibilityState } from '@tanstack/vue-table'

import { ItemParam, ItemTableMedia } from '#components'

import type { CompareItemsResult, ItemFlat } from '~/models/item'
import type { AggregationConfig, AggregationOptions } from '~/services/item-search-service/aggregations'

import { ITEM_COMPARE_MODE } from '~/models/item'

const {
  items,
  aggregationConfig,
  withHeader = false,
  compareItemsResult,
  loading = false,
  currentRank,
  withFiller = true,
} = defineProps<{
  items: ItemFlat[]
  aggregationConfig: AggregationConfig
  compareItemsResult: CompareItemsResult
  loading?: boolean
  withHeader?: boolean
  withFiller?: boolean // offset
  currentRank?: number
}>()

const { t } = useI18n()

function createTableColumn(key: keyof ItemFlat, options: AggregationOptions): TableColumn<ItemFlat> {
  const widthPx = options.width || 160
  return {
    accessorKey: key,
    meta: {
      style: {
        th: {
          width: `${widthPx}px`,
        },
        td: {
          width: `${widthPx}px`,
        },
      },
    },
    header: ({ header }) => t(`item.aggregations.${header.id}.title`),
    cell: ({ row }) => h(ItemParam, {
      field: key,
      item: row.original,
      isCompare: true,
      compareMode: ITEM_COMPARE_MODE.Relative,
      relativeValue: compareItemsResult[key]!,
    }),
  }
}

const columns = computed<TableColumn<ItemFlat>[]>(() => [
  ...(withFiller
    ? [
        {
          id: 'fill',
          meta: {
            class: {
              td: 'px-0 w-[32px]',
              th: 'px-0 w-[32px]',
            },
          },
        },
        {
          id: 'fill2',
          meta: {
            class: {
              td: 'px-0 w-[32px]',
              th: 'px-0 w-[32px]',
            },
          },
        },
      ]
    : []),
  {
    accessorKey: 'name',
    header: '',
    cell: ({ row }) => h(ItemTableMedia, {
      item: row.original,
      showTier: true,
    }),
  },
  ...Object.entries(aggregationConfig).map(([key, config]) => createTableColumn(key as keyof ItemFlat, config)),
])

const columnVisibility = computed<VisibilityState>(() => {
  return {
    ...Object.entries(aggregationConfig)
      .filter(([_, value]) => value.hidden)
      .reduce((out, [key]) => {
        out[key] = false
        return out
      }, {} as VisibilityState),
  }
})

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
      ...(!withHeader && {
        thead: 'hidden',
      }),
    }"
  >
    <template #empty>
      <UiResultNotFound />
    </template>
  </UTable>
</template>

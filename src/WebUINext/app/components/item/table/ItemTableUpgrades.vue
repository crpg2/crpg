<script setup lang="ts">
import type { TableColumn } from '@nuxt/ui'
import type { RowSelectionState, VisibilityState } from '@tanstack/vue-table'

import { AppCoin, ItemParam, ItemTableMedia } from '#components'

import type { CompareItemsResult, ItemFlat } from '~/models/item'
import type { AggregationConfig } from '~/services/item-search-service/aggregations'

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

const { t, n } = useI18n()

function createTableColumn(key: keyof ItemFlat): TableColumn<ItemFlat> {
  return {
    accessorKey: key,
    meta: {
      class: {
        td: 'min-w-[140px]',
        th: 'min-w-[140px]',
      },
      style: {},
    },
    header: ({ header }) => h('div', { class: 'w-[140px]' }, t(`item.aggregations.${header.id}.title`)),
    cell: ({ row }) => h(ItemParam, {
      field: key,
      item: row.original,
      class: 'w-[140px]',
      isCompare: true,
      compareMode: ITEM_COMPARE_MODE.Relative,
      relativeValue: compareItemsResult[key]!,
    }, {
      ...(key === 'upkeep' && {
        default: ({ rawBuckets }: { rawBuckets: number }) => h(AppCoin, { value: t('item.format.upkeep', { upkeep: n(rawBuckets) }) }),
      }),
      ...(key === 'price' && {
        default: ({ rawBuckets }: { rawBuckets: number }) => h(AppCoin, { value: rawBuckets }),
      }),
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
              td: 'px-0 w-[72px]',
              th: 'px-0 w-[72px]',
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
      class: 'w-[328px]',
    }),
    meta: {
      class: {
        th: 'w-[328px]',
        td: 'w-[328px]',
      },
    },
  },
  ...Object.keys(aggregationConfig).map(key => createTableColumn(key as keyof ItemFlat)),
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

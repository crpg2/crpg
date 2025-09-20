<script setup lang="ts">
import type { TableColumn } from '@nuxt/ui'
import type { VisibilityState } from '@tanstack/vue-table'

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
} = defineProps<{
  items: ItemFlat[]
  aggregationConfig: AggregationConfig
  compareItemsResult: CompareItemsResult
  loading?: boolean
  withHeader?: boolean
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
      ...(key === 'upkeep' && {
        default: ({ rawBuckets }: { rawBuckets: number }) => h(AppCoin, null, {
          default: () => t('item.format.upkeep', { upkeep: n(rawBuckets) }),
        }),
      }),
      ...(key === 'price' && {
        default: ({ rawBuckets }: { rawBuckets: number }) => h(AppCoin, { value: rawBuckets }),
      }),
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
  // {
  //   id: 'fill',
  //   meta: {
  //     class: {
  //       td: 'w-[80px]',
  //     },
  //   },
  // },
  {
    accessorKey: 'name',
    header: '',
    cell: ({ row }) => h(ItemTableMedia, {
      item: row.original,
      showTier: true,
    }),
    meta: {
      class: {
        td: 'w-[40%]',
        th: 'w-[40%]',
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
</script>

<template>
  <UTable
    v-model:column-visibility="columnVisibility"
    class="rounded-md border border-muted"
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

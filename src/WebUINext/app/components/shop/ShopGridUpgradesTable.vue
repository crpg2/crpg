<script setup lang="ts">
import type { TableColumn } from '@nuxt/ui'
import type { VisibilityState } from '@tanstack/vue-table'

import { AppCoin, ItemParam, ShopGridItemMedia } from '#components'

import type { ItemFlat } from '~/models/item'
import type { AggregationConfig } from '~/services/item-search-service/aggregations'

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
  relativeEntries,
} = useItemUpgrades(item, aggregationConfig)

function createTableColumn(key: keyof ItemFlat): TableColumn<ItemFlat> {
  return {
    accessorKey: key,
    header: '',
    cell: ({ row }) => h(ItemParam, {
      field: key,
      item: row.original,
      isCompare: true,
      compareMode: ItemCompareMode.Relative,
      relativeValue: relativeEntries.value[key]!,
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
  {
    id: 'fill',
    meta: {
      class: {
        td: 'w-[80px]',
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
        td: 'w-[360px]',
        th: 'w-[360px]',
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
  <div class="relative">
    <UiLoading :active="isLoading" />
    <UTable
      v-model:column-visibility="columnVisibility"
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

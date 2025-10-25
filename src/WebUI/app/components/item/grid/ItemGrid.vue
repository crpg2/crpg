<script setup lang="ts" generic="T extends { item: Item }">
import type { SelectItem, TableColumn } from '@nuxt/ui'
import type { ColumnFiltersState } from '@tanstack/vue-table'

import { functionalUpdate, getCoreRowModel, getFilteredRowModel, getPaginationRowModel, getSortedRowModel, useVueTable } from '@tanstack/vue-table'

import type { Item, ItemType } from '~/models/item'
import type { SortingConfig } from '~/services/item-search-service'

import { useMainHeader } from '~/composables/app/use-main-header'
import { useStickySidebar } from '~/composables/use-sticky-sidebar'
import { ITEM_TYPE } from '~/models/item'
import { getFacetsByItemType, getFilterFn } from '~/services/item-search-service'
import { aggregationsConfig } from '~/services/item-search-service/aggregations'

const { sortingConfig, items, withPagination = true, loading = false } = defineProps<{
  items: T[]
  sortingConfig: SortingConfig
  withPagination?: boolean
  loading?: boolean
}>()

const { t } = useI18n()

const { mainHeaderHeight } = useMainHeader()
const aside = useTemplateRef('aside')
const { top: stickySidebarTop } = useStickySidebar(aside, mainHeaderHeight.value + 16, 16 /** 1rem */)

const itemType = ref<ItemType>(ITEM_TYPE.Undefined)
const itemTypes = computed(() => getFacetsByItemType(items.map(wrapper => wrapper.item.type)))

watch(itemType, () => {
  window.scrollTo({ behavior: 'smooth', top: 0 })
})

const { pagination, setPagination } = usePagination({ pageSize: 20 })

const sortingItems = computed(() => Object.keys(sortingConfig).map<SelectItem>(key => ({
  label: t(`item.sort.${key}`),
  value: key,
})))
const sortingModel = defineModel<string>('sorting', { default: '' })
const sorting = computed(() => {
  const cfg = sortingConfig[sortingModel.value]
  return cfg ? [{ id: cfg.field, desc: cfg.order === 'desc' }] : []
})

const filterByNameModel = ref<string | undefined>(undefined)

const columnFilters = computed<ColumnFiltersState>(() => [
  ...(itemType.value !== ITEM_TYPE.Undefined ? [{ id: 'type', value: itemType.value }] : []),
])

const columns: TableColumn<T>[] = [
  {
    accessorFn: row => row.item.id,
    id: 'id',
  },
  {
    accessorFn: row => row.item.type,
    id: 'type',
    filterFn: getFilterFn(aggregationsConfig.type!),
  },
  {
    accessorFn: row => row.item.price,
    id: 'price',
  },
  {
    accessorFn: row => row.item.rank,
    id: 'rank',
  },
  {
    accessorFn: row => row.item.name,
    id: 'name',
  },
]

const grid = useVueTable({
  get data() {
    return toRef(() => items)
  },
  columns,
  getCoreRowModel: getCoreRowModel(),
  getFilteredRowModel: getFilteredRowModel(),
  getSortedRowModel: getSortedRowModel(),
  filterFns: {
    includesSome,
  },
  state: {
    get sorting() {
      return sorting.value
    },
    get globalFilter() {
      return filterByNameModel.value
    },
    get columnFilters() {
      return columnFilters.value
    },
    get pagination() {
      return pagination.value
    },
  },
  ...(withPagination && {
    getPaginationRowModel: getPaginationRowModel(),
    onPaginationChange: (updater) => {
      setPagination(functionalUpdate(updater, pagination.value))
    },
  }),
})

watch(() => items, () => {
  // For example, if a product has been sold, you need to reset the filter by type.
  if (!grid.getRowModel().rows.length) {
    itemType.value = ITEM_TYPE.Undefined
  }
})

const filteredItemsCost = computed(() => grid.getRowModel().rows.reduce((out, row) => out + row.original.item.price, 0))
</script>

<template>
  <div class="relative">
    <UiLoading :active="loading" />

    <div v-if="items.length" class="itemGrid grid h-full items-start gap-x-3 gap-y-4">
      <div
        ref="aside"
        style="grid-area: aside"
        class="sticky top-0 left-0 flex flex-col items-center justify-center space-y-2"
        :style="{ top: `${stickySidebarTop}px` }"
      >
        <ItemSearchFilterByType
          v-model:item-type="itemType"
          :item-types="itemTypes"
          orientation="vertical"
          with-all-categories
        />
      </div>

      <div style="grid-area: topbar" class="grid grid-cols-5 gap-3">
        <UInput
          v-model="filterByNameModel"
          :placeholder="$t('action.search')"
          icon="crpg:search"
          variant="subtle"
          class="col-span-3 w-full"
          size="xl"
        >
          <template v-if="filterByNameModel?.length" #trailing>
            <UiInputClear @click="filterByNameModel = ''" />
          </template>
        </UInput>

        <div class="col-span-2 flex gap-3">
          <USelect
            v-model="sortingModel"
            class="flex-1"
            :items="sortingItems"
            size="xl"
          />

          <slot name="filter-leading" />
        </div>
      </div>

      <div
        style="grid-area: result"
        class="
          grid grid-cols-3 gap-2
          2xl:grid-cols-4
        "
      >
        <template v-for="item in grid.getRowModel().rows" :key="item.id">
          <slot name="item" v-bind="item.original" />
        </template>
      </div>

      <div style="grid-area: footer" class="sticky bottom-4 z-10 space-y-3">
        <UiGridPagination
          v-if="withPagination"
          :table-api="toRef(() => grid)"
        />

        <slot
          name="footer"
          v-bind="{
            filteredItemsCost,
            filteredItemsCount: grid.getRowModel().rows.length,
            totalItemsCount: grid.getFilteredRowModel().rows.length }"
        />
      </div>
    </div>

    <UCard v-else-if="!loading">
      <UiResultNotFound :message="$t('character.inventory.empty')" />
    </UCard>
  </div>
</template>

<style lang="css">
.itemGrid {
  grid-template-areas:
    'topbar topbar'
    'aside result'
    'aside footer';
  grid-template-columns: auto 1fr;
  grid-template-rows: auto 1fr auto;
}
</style>

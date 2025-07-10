<script setup lang="ts" generic="T extends { item: Item }">
import type { SelectItem, TableColumn } from '@nuxt/ui'
import type { ColumnFiltersState } from '@tanstack/vue-table'

import { getCoreRowModel, getFilteredRowModel, getPaginationRowModel, getSortedRowModel, useVueTable } from '@tanstack/vue-table'

import type { Item, ItemType } from '~/models/item'
import type { SortingConfig } from '~/services/item-search-service'

import { useMainHeader } from '~/composables/app/use-main-header'
import { useStickySidebar } from '~/composables/use-sticky-sidebar'
import { ITEM_TYPE } from '~/models/item'
import {
  getFacetsByItemType,
} from '~/services/item-search-service'

const { sortingConfig, items, withPagination = true } = defineProps<{
  items: T[]
  sortingConfig: SortingConfig
  withPagination?: boolean
}>()

const { t } = useI18n()

const { mainHeaderHeight } = useMainHeader()
const aside = useTemplateRef('aside')
const { top: stickySidebarTop } = useStickySidebar(aside, mainHeaderHeight.value + 16, 16)

const itemTypes = computed(() => getFacetsByItemType(items.map(wrapper => wrapper.item.type)))
const itemType = ref<ItemType>(ITEM_TYPE.Undefined)

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
  },
  {
    accessorFn: row => row.item.price,
    id: 'price',
  },
  {
    accessorFn: row => row.item.rank,
    id: 'rank',
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

  ...(withPagination && {
    getPaginationRowModel: getPaginationRowModel(),
    initialState: {
      pagination: {
        pageSize: 11,
      },
    },
  }),
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
  },
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
  <div class="itemGrid grid h-full items-start gap-x-3 gap-y-4">
    <div
      ref="aside"
      style="grid-area: filter"
      class="sticky top-0 left-0 flex flex-col items-center justify-center space-y-2"
      :style="{ top: `${stickySidebarTop}px` }"
    >
      <slot name="filter-leading" />

      <ItemSearchFilterByType
        v-model:item-type="itemType"
        :item-types="itemTypes"
        orientation="vertical"
        with-all-categories
      />
    </div>

    <div
      style="grid-area: sort"
      class="
        grid grid-cols-3 gap-4
        2xl:grid-cols-4
      "
    >
      <div
        class="
          col-span-2
          2xl:col-span-3
        "
      >
        <UInput
          v-model="filterByNameModel"
          :placeholder="$t('action.search')"
          icon="crpg:search"
          variant="subtle"
          class="w-full"
        >
          <template v-if="filterByNameModel?.length" #trailing>
            <UButton
              color="neutral"
              variant="link"
              icon="crpg:close"
              @click="filterByNameModel = ''"
            />
          </template>
        </UInput>
      </div>

      <USelect
        v-model="sortingModel"
        :items="sortingItems"
        trailing-icon="crpg:arrow-up-down"
      />
    </div>

    <div
      style="grid-area: items"
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
      <UPagination
        v-if="withPagination && grid.getCanNextPage() || grid.getCanPreviousPage()"
        :ui="{
          list: 'justify-center',
        }"
        :page="grid.getState().pagination.pageIndex + 1"
        variant="soft"
        active-variant="solid"
        active-color="primary"
        :show-controls="false"
        show-edges
        :total="grid.getFilteredRowModel().rows.length"
        :default-page="(grid.initialState.pagination.pageIndex || 0) + 1"
        :items-per-page="grid.initialState.pagination.pageSize"
        @update:page="(p) => grid.setPageIndex(p - 1)"
      />

      <slot name="footer" v-bind="{ filteredItemsCost, filteredItemsCount: grid.getRowModel().rows.length, totalItemsCount: grid.getFilteredRowModel().rows.length }" />
    </div>
  </div>
</template>

<style lang="css">
.itemGrid {
  grid-template-areas:
    '... sort'
    'filter items'
    'filter footer';
  grid-template-columns: auto 1fr;
  grid-template-rows: auto 1fr auto;
}
</style>

<script setup lang="ts" generic="T extends { item: Item }">
import type { SelectItem, TableColumn } from '@nuxt/ui'
import type { ColumnFiltersState } from '@tanstack/vue-table'
import type { ValueOf } from 'type-fest'

import { getCoreRowModel, getFilteredRowModel, getSortedRowModel, useVueTable } from '@tanstack/vue-table'

import type { GroupedCompareItemsResult, Item, ItemType } from '~/models/item'
import type { SortingConfig } from '~/services/item-search-service'

import { useItemDetail } from '~/composables/item/use-item-detail'
// import { useMainHeader } from '~/composables/app/use-main-header'
// import { useStickySidebar } from '~/composables/use-sticky-sidebar'
import { ITEM_TYPE } from '~/models/item'
import { getAggregationsConfig, getFacetsByItemType, getFilterFn } from '~/services/item-search-service'
import { aggregationsConfig } from '~/services/item-search-service/aggregations'
import { createItemIndex } from '~/services/item-search-service/indexator'
import { extractItem, getCompareItemsResult, groupItemsByTypeAndWeaponClass } from '~/services/item-service'

const {
  sortingConfig,
  itemsA,
  itemsB,
  loading = false,
  size = 'xl',
} = defineProps<{
  itemsA: T[]
  itemsB: T[]
  sortingConfig: SortingConfig
  loading?: boolean
  size?: 'md' | 'xl'
}>()

const { t } = useI18n()

const ITEM_GROUP = {
  GroupA: 'GroupA',
  GroupB: 'GroupB',
} as const

type ItemGroup = ValueOf<typeof ITEM_GROUP>
type GroupedItem = T & {
  group: ItemGroup
}

const items = computed<GroupedItem[]>(() => [
  ...itemsA.map(item => ({ ...item, group: ITEM_GROUP.GroupA })),
  ...itemsB.map(item => ({ ...item, group: ITEM_GROUP.GroupB })),
])
const itemType = ref<ItemType>(ITEM_TYPE.Undefined)
const itemTypes = computed(() => getFacetsByItemType(items.value.map(wrapper => wrapper.item.type)))

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

const columns: TableColumn<GroupedItem>[] = [
  {
    accessorFn: row => row.group,
    id: 'group',
  },
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
    return toRef(() => items.value)
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
  },
})

const findedItemsA = computed(() => grid.getRowModel().rows.filter(row => row.original.group === ITEM_GROUP.GroupA))
const findedItemsB = computed(() => grid.getRowModel().rows.filter(row => row.original.group === ITEM_GROUP.GroupB))

watch(() => items.value, () => {
  // For example, if a product has been sold, you need to reset the filter by type.
  if (!grid.getRowModel().rows.length) {
    itemType.value = ITEM_TYPE.Undefined
  }
})

const { isOpen } = useItemDetail()

const compareItemsResult = computed<GroupedCompareItemsResult[]>(() => {
  return groupItemsByTypeAndWeaponClass(
    // find the open items
    createItemIndex(items.value.filter(wrapper => isOpen(wrapper.item.id))
      .map(extractItem)),
  )
    .filter(group => group.items.length >= 2) // there is no point in comparing 1 item
    .map(group => ({
      compareResult: getCompareItemsResult(group.items, getAggregationsConfig(group.type, group.weaponClass)),
      type: group.type,
      weaponClass: group.weaponClass,
    }))
})
</script>

<template>
  <div class="relative space-y-4">
    <UiLoading :active="loading" />

    <div class="grid grid-cols-5 gap-3">
      <UInput
        v-model="filterByNameModel"
        :placeholder="$t('action.search')"
        icon="crpg:search"
        variant="subtle"
        class="col-span-3 w-full"
        :size
      >
        <template v-if="filterByNameModel?.length" #trailing>
          <UiInputClear @click="filterByNameModel = ''" />
        </template>
      </UInput>

      <div class="col-span-2 flex items-center gap-3">
        <USelect
          v-model="sortingModel"
          class="flex-1"
          :items="sortingItems"
          :size
        />

        <slot name="filter-trailing" />
      </div>
    </div>

    <div class="grid grid-cols-[1fr_auto_1fr] gap-4">
      <div v-if="findedItemsA.length">
        <slot name="left-side-header" />
        <div
          class="
            grid auto-rows-min grid-cols-3 gap-2
            2xl:grid-cols-2
          "
        >
          <TransitionGroup name="item-card" tag="div" class="contents">
            <template v-for="item in findedItemsA" :key="item.original.item.id">
              <slot name="item" v-bind="item.original" />
            </template>
          </TransitionGroup>
        </div>
      </div>

      <UiResultNotFound v-else :message="$t('character.inventory.empty')" />

      <div
        class="sticky top-0"
      >
        <ItemSearchFilterByType
          v-model:item-type="itemType"
          :item-types="itemTypes"
          orientation="vertical"
          with-all-categories
          :size
        />
      </div>

      <div v-if="findedItemsB.length">
        <slot name="right-side-header" />
        <div
          class="
            grid auto-rows-min grid-cols-3 gap-2
            2xl:grid-cols-2
          "
        >
          <TransitionGroup name="item-card" tag="div" class="contents">
            <template v-for="item in findedItemsB" :key="item.original.item.id">
              <slot name="item" v-bind="item.original" />
            </template>
          </TransitionGroup>
        </div>
      </div>
      <UiResultNotFound v-else :message="$t('character.inventory.empty')" />
    </div>

    <!-- TODO: -->
    <!-- <UCard v-else-if="!loading">
      <UiResultNotFound :message="$t('character.inventory.empty')" />
    </UCard> -->

    <ItemDetailGroup>
      <template #default="item">
        <slot name="item-detail" v-bind="{ item, compareItemsResult }" />
      </template>
    </ItemDetailGroup>
  </div>
</template>

<!-- TODO: to reusable component -->
<style>
.item-card-enter-active {
  transition: opacity 0.3s cubic-bezier(0.4, 0, 0.2, 1), transform 0.3s cubic-bezier(0.4, 0, 0.2, 1);
}
.item-card-leave-active {
  transition: opacity 0.25s cubic-bezier(0.4, 0, 1, 1), transform 0.25s cubic-bezier(0.4, 0, 1, 1);
}
.item-card-leave-to {
  opacity: 0;
  transform: scale(0.95);
}
</style>

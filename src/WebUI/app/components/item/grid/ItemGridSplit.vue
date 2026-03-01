<script setup lang="ts" generic="T extends { item: Item }">
import type { TableOptions } from '@tanstack/vue-table'
import type { ValueOf } from 'type-fest'

import type { GroupedCompareItemsResult, Item } from '~/models/item'
import type { SortingConfig } from '~/services/item-search-service'

import { useItemGrid } from '~/composables/item/use-item-grid'

type GroupedItem = T & { group: ItemGroup }

const {
  sortingConfig,
  itemsA,
  itemsB,
  loading = false,
  size = 'xl',
  additionalColumns,
} = defineProps<{
  itemsA: T[]
  itemsB: T[]
  sortingConfig?: SortingConfig<string>
  additionalColumns?: TableOptions<GroupedItem>['columns']
  loading?: boolean
  size?: 'md' | 'xl'
}>()

defineSlots<{
  ['item-detail']: {
    item: Item
    compareItemsResult: GroupedCompareItemsResult[]
  }
  ['GroupA-header']: {
    filteredItemIds: string[]
  }
  ['GroupB-header']: {
    filteredItemIds: string[]
  }
  ['item']: T
  ['filter-trailing']: any
}>()

const ITEM_GROUP = {
  GroupA: 'GroupA',
  GroupB: 'GroupB',
} as const

type ItemGroup = ValueOf<typeof ITEM_GROUP>

const items = computed<GroupedItem[]>(() => [
  ...itemsA.map(item => ({ ...item, group: ITEM_GROUP.GroupA })),
  ...itemsB.map(item => ({ ...item, group: ITEM_GROUP.GroupB })),
])

const sortingModel = defineModel<string>('sorting', { default: 'rank_desc' })

const {
  itemType,
  itemTypes,
  sortingItems,
  filterByNameModel,
  grid,
  compareItemsResult,
} = useItemGrid({
  items,
  sortingConfig,
  sortingModel,
  additionalColumns,
})

const groupedItems = computed(() => ({
  [ITEM_GROUP.GroupA]: grid.getRowModel().rows.filter(row => row.original.group === ITEM_GROUP.GroupA),
  [ITEM_GROUP.GroupB]: grid.getRowModel().rows.filter(row => row.original.group === ITEM_GROUP.GroupB),
}))
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
      <template
        v-for="(group, idx) in Object.values(ITEM_GROUP)"
        :key="group"
      >
        <div v-if="idx === 1" class="sticky top-0">
          <ItemSearchFilterByType
            v-model:item-type="itemType"
            :item-types="itemTypes"
            orientation="vertical"
            with-all-categories
            :size
          />
        </div>

        <div>
          <slot
            :name="`${group}-header`"
            v-bind="{ filteredItemIds: groupedItems[group].map(item => item.original.item.id) }"
          />

          <div
            v-if="groupedItems[group].length"
            class="
              grid auto-rows-min grid-cols-2 gap-2
              2xl:grid-cols-3
            "
          >
            <template v-for="item in groupedItems[group]" :key="item.original.item.id">
              <slot name="item" v-bind="item.original" />
            </template>
          </div>

          <UiResultNotFound v-else :message="$t('character.inventory.empty')" />
        </div>
      </template>
    </div>

    <ItemDetailGroup>
      <template #default="item">
        <slot name="item-detail" v-bind="{ item, compareItemsResult }" />
      </template>
    </ItemDetailGroup>
  </div>
</template>

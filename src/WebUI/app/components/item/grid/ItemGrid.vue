<script setup lang="ts" generic="T extends { item: Item }">
import type { TableOptions } from '@tanstack/vue-table'

import type { Item } from '~/models/item'
import type { SortingConfig } from '~/services/item-search-service'

import { useItemGrid } from '~/composables/item/use-item-grid'
// import { useMainHeader } from '~/composables/app/use-main-header'
// import { useStickySidebar } from '~/composables/use-sticky-sidebar'

const {
  sortingConfig,
  items,
  withPagination = true,
  loading = false,
  size = 'xl',
  additionalColumns,
} = defineProps<{
  items: T[]
  sortingConfig?: SortingConfig<string>
  additionalColumns?: TableOptions<T>['columns']
  withPagination?: boolean
  loading?: boolean
  size?: 'md' | 'xl'
}>()

const { t } = useI18n()

// TODO: FIXME: либо выпилить, либо активировать условно, потому что в модалках это не нужно
// const { mainHeaderHeight } = useMainHeader()
// const aside = useTemplateRef('aside')
// const { top: stickySidebarTop } = useStickySidebar(aside, mainHeaderHeight.value + 16, 16 /** 1rem */)

const sortingModel = defineModel<string>('sorting', { default: 'rank_desc' })

const {
  itemType,
  itemTypes,
  sortingItems,
  filterByNameModel,
  grid,
  showPagination,
  filteredItemsCost,
  compareItemsResult,
} = useItemGrid({
  items: toRef(() => items),
  sortingConfig,
  withPagination,
  sortingModel,
  additionalColumns,
})
</script>

<template>
  <div class="relative">
    <UiLoading :active="loading" />

    <div class="itemGrid grid h-full items-start gap-x-3 gap-y-4">
      <!-- ref="aside"
      :style="{ top: `${stickySidebarTop}px` }" -->
      <div
        v-if="items.length"
        style="grid-area: aside"
        class="sticky top-0 left-0 flex flex-col items-center justify-center space-y-2"
      >
        <ItemSearchFilterByType
          v-model:item-type="itemType"
          :item-types="itemTypes"
          orientation="vertical"
          with-all-categories
          :size
        />
      </div>

      <div style="grid-area: topbar" class="grid grid-cols-5 gap-3">
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

      <div
        v-if="items.length"
        style="grid-area: result"
        class="
          grid grid-cols-3 gap-2
          2xl:grid-cols-4
        "
      >
        <template v-for="item in grid.getRowModel().rows" :key="item.original.item.id">
          <slot name="item" v-bind="item.original" />
        </template>
      </div>

      <UCard v-else-if="!loading" style="grid-area: result">
        <UiResultNotFound :message="$t('character.inventory.empty')" />
      </UCard>

      <div style="grid-area: footer" class="sticky bottom-4 z-10 space-y-3">
        <UiGridPagination
          v-if="withPagination && showPagination"
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

    <ItemDetailGroup>
      <template #default="item">
        <slot name="item-detail" v-bind="{ item, compareItemsResult }" />
      </template>
    </ItemDetailGroup>
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

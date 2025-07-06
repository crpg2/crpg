<script setup lang="ts" generic="T extends { item: Item }">
import type { SelectItem } from '@nuxt/ui'

import type { Item, ItemType } from '~/models/item'
import type { SortingConfig } from '~/services/item-search-service'

import { useMainHeader } from '~/composables/app/use-main-header'
import { useStickySidebar } from '~/composables/character/use-sticky-sidebar'
import { ITEM_TYPE } from '~/models/item'
import {
  filterItemsByName,
  filterItemsByType,
  getFacetsByItemType,
  sortItemsByField,
} from '~/services/item-search-service'

const { sortingConfig, items } = defineProps<{
  items: T[]
  sortingConfig: SortingConfig
}>()

const { t } = useI18n()

const { mainHeaderHeight } = useMainHeader()
const aside = useTemplateRef('aside')
const { top: stickySidebarTop } = useStickySidebar(aside, mainHeaderHeight.value + 16, 16)

const filterByNameModel = ref<string>('')
const sortModel = defineModel<string>('sorting', { default: '' })

const sortingItems = computed(() => Object.keys(sortingConfig).map<SelectItem>(key => ({
  label: t(`item.sort.${key}`),
  value: key,
})))

const itemType = ref<ItemType>(ITEM_TYPE.Undefined)
const itemTypes = computed(() => getFacetsByItemType(items.map(wrapper => wrapper.item.type)))

const filteredItems = computed(() => {
  return sortItemsByField(
    filterItemsByName(
      filterItemsByType(items, itemType.value),
      filterByNameModel.value,
    ),
    sortingConfig[sortModel.value]!,
  )
})

watch(filteredItems, () => {
  // For example, if a product has been sold, you need to reset the filter by type.
  if (!filterByNameModel.value && !filteredItems.value.length) {
    itemType.value = ITEM_TYPE.Undefined
  }
})

const filteredItemsCost = computed(() => filteredItems.value.reduce((out, wrapper) => out + wrapper.item.price, 0))
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
      class="
        grid grid-cols-3 gap-4
        2xl:grid-cols-4
      "
      style="grid-area: sort"
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
        v-model="sortModel"
        :items="sortingItems"
        trailing-icon="crpg:arrow-up-down"
      />
    </div>

    <div
      class="
        grid grid-cols-3 gap-2
        2xl:grid-cols-4
      " style="grid-area: items"
    >
      <template v-for="item in filteredItems" :key="item.id">
        <slot name="item" v-bind="item" />
      </template>
    </div>

    <slot name="footer" v-bind="{ filteredItemsCost, filteredItemsCount: filteredItems.length }" />
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

<script setup lang="ts" generic="TItem extends ItemFlat, TSelected">
// TODO: FIXME:
/* eslint-disable vue/no-unused-refs -- ref="tree" is used by useTemplateRef inside composable */
import type { ItemFlat } from '~/models/item'

import { useItemSelectTree } from '~/composables/item/use-item-select-tree'

const { fetchItems, getItemKey, mapToSelected, isSelectedEqual, supportsUpgrades = false, isItemDisabled } = defineProps<{
  fetchItems: () => Promise<TItem[]>
  getItemKey: (item: TItem) => string
  mapToSelected: (item: TItem) => TSelected
  isSelectedEqual: (selected: TSelected, item: TItem) => boolean
  supportsUpgrades?: boolean
  isItemDisabled?: (item: TItem) => boolean
}>()

const selectedItem = defineModel<TSelected | null>('modelValue', { default: () => null })

const {
  searchModel,
  loadingItems,
  treeItems,
  expandedNodes,
  selectedNode,
  estimateTreeItemSize,
  loadingUpgradesNodeKeys,
  onToggle,
  onChange,
  onSelect,
} = useItemSelectTree<TItem, TSelected>({
  selectedItem,
  fetchItems,
  getItemKey,
  mapToSelected,
  isSelectedEqual,
  supportsUpgrades,
  isItemDisabled,
})
</script>

<template>
  <div class="space-y-4">
    <UInput
      v-model="searchModel"
      :placeholder="$t('action.search')"
      icon="i-lucide-search"
      class="w-full"
      size="xl"
    >
      <template v-if="searchModel.length" #trailing>
        <UiInputClear @click="searchModel = ''" />
      </template>
    </UInput>

    <UiLoading :active="loadingItems" :text="$t('common.loading')" />

    <UTree
      v-if="treeItems.length"
      ref="tree"
      v-model:expanded="expandedNodes"
      :model-value="selectedNode"
      :as="{ link: 'div' }"
      :items="treeItems"
      :get-key="(item) => item.key"
      label-key="label"
      class="h-100"
      :ui="{
        link: 'cursor-pointer',
      }"
      :virtualize="{
        overscan: 12,
        estimateSize: estimateTreeItemSize,
      }"
      :nested="false"
      @toggle="onToggle"
      @update:model-value="onChange"
      @select="onSelect"
    >
      <template #item="{ item: node, expanded, selected, handleSelect }">
        <ItemSelectTreeItem
          :node
          :expanded
          :selected
          :supports-upgrades
          :loading-upgrade="loadingUpgradesNodeKeys.has(node.item?.baseId ?? '')"
          @select="handleSelect"
        >
          <template #item-top-right>
            <slot name="item-top-right" :node="node" />
          </template>
        </ItemSelectTreeItem>
      </template>
    </UTree>

    <UiResultNotFound v-else-if="!loadingItems" />
  </div>
</template>

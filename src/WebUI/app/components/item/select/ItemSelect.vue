<script lang="ts" setup>
import type { ItemFlat, SelectedItem } from '~/models/item'

import { createItemIndex } from '~/services/item-search-service/indexator'
import { getItems } from '~/services/item-service'

const { isItemDisabled, disabled = false } = defineProps<{
  label: string
  disabled?: boolean
  isItemDisabled?: (item: ItemFlat) => boolean
}>()

const selectedItem = defineModel<SelectedItem | null>('modelValue', { default: null })
const localSelectedItem = ref<SelectedItem | null>(selectedItem.value)

const fetchItems = async () => createItemIndex(await getItems())

const getItemKey = (item: ItemFlat) => item.id

const mapToSelected = (item: ItemFlat): SelectedItem => ({
  id: item.id,
  baseId: item.baseId,
  rank: item.rank,
  name: item.name,
})

const isSelectedEqual = (selected: SelectedItem, item: ItemFlat) => selected.id === item.id

const onClear = () => {
  selectedItem.value = null
  localSelectedItem.value = null
}

const onCancel = () => {
  localSelectedItem.value = null
}

const onSubmit = () => {
  selectedItem.value = localSelectedItem.value
}
</script>

<template>
  <ItemSelectDrawer
    :label
    @cancel="onCancel"
    @submit="onSubmit"
  >
    <ItemSelectTrigger
      :item="selectedItem"
      :label
      :disabled
      @clear="onClear"
    />

    <template #body>
      <ItemSelectTree
        v-model="localSelectedItem"
        class="w-lg"
        :fetch-items="fetchItems"
        :get-item-key="getItemKey"
        :map-to-selected="mapToSelected"
        :is-selected-equal="isSelectedEqual"
        :is-item-disabled="isItemDisabled"
        supports-upgrades
      >
        <template #item-top-right="{ node }">
          <slot name="item-top-right" :node />
        </template>
      </ItemSelectTree>
    </template>
  </ItemSelectDrawer>
</template>

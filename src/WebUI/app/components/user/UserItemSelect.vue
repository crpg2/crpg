<script setup lang="ts">
import type { SelectedUserItem, UserItemFlat } from '~/models/item'

import { createItemIndex } from '~/services/item-search-service/indexator'
import { getUserItems } from '~/services/user-service'

const { isItemDisabled } = defineProps<{
  label: string
  isItemDisabled?: (item: UserItemFlat) => boolean
}>()

const selectedItem = defineModel<SelectedUserItem | null>('modelValue', { default: () => null })
const localSelectedItem = ref<SelectedUserItem | null>(selectedItem.value)

const fetchItems = async () => createItemIndex(await getUserItems(), {
  writeMeta: userItem => ({
    userItemId: userItem.id,
    isBroken: userItem.isBroken,
    isPersonal: userItem.isPersonal,
    clanArmoryLender: userItem.clanArmoryLender,
  }),
})

const getItemKey = (item: UserItemFlat) => [item.id, item.meta.userItemId].join('::')

const mapToSelected = (item: UserItemFlat): SelectedUserItem => ({
  id: item.id,
  baseId: item.baseId,
  rank: item.rank,
  name: item.name,
  userItemId: item.meta.userItemId,
})

const isSelectedEqual = (selected: SelectedUserItem, item: UserItemFlat) =>
  selected.id === item.id && selected.userItemId === item.meta.userItemId

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
      :label="label"
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
      >
        <template #item-top-right="{ node }">
          <slot name="item-top-right" :node />
        </template>
      </ItemSelectTree>
    </template>
  </ItemSelectDrawer>
</template>

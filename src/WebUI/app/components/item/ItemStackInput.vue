<script setup lang="ts">
import type { Item } from '~/models/item'

defineProps<{
  item: Item
  max: number
}>()

defineEmits<{
  toggleItemDetail: [target: HTMLElement, itemId: string]
}>()

const count = defineModel<number>({ default: 0 })
</script>

<template>
  <div class="flex flex-col">
    <ItemCard
      class="cursor-pointer"
      :item
      @click="(e: Event) => $emit('toggleItemDetail', e.target as HTMLElement, item.id)"
    />
    <UInputNumber
      v-model="count"
      class="w-full"
      :min="0"
      size="sm"
      :max
    />
    <USlider
      :model-value="count"
      class="px-2"
      :min="0"
      :max
      size="sm"
      @update:model-value="(value) => {
        // TODO: Strange behavior of the nuxt/ui Slider component. If you're not too lazy, please  create an issue/PR
        if (value !== undefined && typeof value === 'number') {
          count = value
        }
      }"
    />
  </div>
</template>

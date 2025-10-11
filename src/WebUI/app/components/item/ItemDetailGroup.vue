<script setup lang="ts">
import { UseDraggable as Draggable } from '@vueuse/components'

import { useItemDetail } from '~/composables/item/use-item-detail'

const {
  closeItemDetail,
  computeDetailCardYPosition,
  openedItems,
} = useItemDetail(true)
</script>

<template>
  <Teleport to="body">
    <Draggable
      v-for="item in openedItems"
      :key="item.id"
      :initial-value="{
        x: item.bound.x + item.bound.width + 8,
        y: computeDetailCardYPosition(item.bound.y),
      }"
      class="fixed cursor-move rounded-md bg-default shadow-lg select-none"
    >
      <UButton
        class="absolute top-0 right-0 z-10 translate-x-1/2 -translate-y-1/2 cursor-pointer"
        icon="crpg:close"
        size="xl"
        color="neutral"
        variant="subtle"
        @click="closeItemDetail(item)"
      />

      <slot v-bind="item" />
    </Draggable>
  </Teleport>
</template>

<script setup lang="ts">
import { UseDraggable as Draggable } from '@vueuse/components'

import { useItemDetail } from '~/composables/character/inventory/use-item-detail'

const {
  closeItemDetail,
  computeDetailCardYPosition,
  openedItems,
} = useItemDetail(true)
</script>

<template>
  <Teleport to="body">
    <Draggable
      v-for="oi in openedItems"
      :key="oi.id"
      :initial-value="{
        x: oi.bound.x + oi.bound.width + 8,
        y: computeDetailCardYPosition(oi.bound.y),
      }"
      class="
        fixed cursor-move rounded-md bg-default shadow-lg ring-info select-none
        active:ring-2
      "
    >
      <UButton
        class="!absolute -top-3 -right-3 z-10 cursor-pointer"
        icon="crpg:close"
        size="sm"
        color="secondary"
        variant="solid"
        @click="closeItemDetail(oi)"
      />
      <slot v-bind="oi" />
    </Draggable>
  </Teleport>
</template>

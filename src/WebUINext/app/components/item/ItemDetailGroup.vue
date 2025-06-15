<script setup lang="ts">
import { UseDraggable as Draggable } from '@vueuse/components'

import { useItemDetail } from '~/composables/character/use-item-detail'

const { closeItemDetail, computeDetailCardYPosition, openedItems } = useItemDetail(true)
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
      class="fixed z-50 cursor-move select-none rounded-lg bg-base-300 shadow-lg active:ring-1"
    >
      <OButton
        class="!absolute -right-3 -top-3 z-10 cursor-pointer"
        icon-right="close"
        rounded
        size="xs"
        variant="secondary"
        @click="closeItemDetail(oi)"
      />
      <slot v-bind="oi" />
    </Draggable>
  </Teleport>
</template>

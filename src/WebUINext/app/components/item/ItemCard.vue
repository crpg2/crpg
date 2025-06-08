<script setup lang="ts">
import type { Item } from '~/models/item'

import { useItem } from '~/composables/item/use-item'

const { item } = defineProps<{
  item: Item
}>()

const { thumb } = useItem(() => item)
</script>

<template>
  <article
    class="min-h-20 items-center justify-center space-y-1 rounded-lg bg-base-200 ring ring-transparent hover:ring-border-200"
  >
    <div class="relative h-full">
      <img
        :src="thumb"
        :alt="item.name"
        class="h-full object-contain select-none"
        @onContextMenu.prevent
      >

      <div class="absolute top-1 left-1 z-10 flex items-center gap-1">
        <ItemRankIcon
          v-if="item.rank > 0"
          :rank="item.rank"
          class="cursor-default opacity-80 hover:opacity-100"
        />
        <slot name="badges-top-left" />
      </div>

      <div class="absolute top-1 right-1 z-10 flex items-center gap-1">
        <slot name="badges-top-right" />
      </div>

      <div class="absolute bottom-1 left-0 z-10 flex items-center gap-1">
        <slot name="badges-bottom-left" />
      </div>

      <div class="absolute right-0 bottom-1 z-10 flex items-center gap-1">
        <slot name="badges-bottom-right" />
      </div>
    </div>
  </article>
</template>

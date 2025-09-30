<script setup lang="ts">
import type { Item } from '~/models/item'

import { useItem } from '~/composables/item/use-item'
import { getRankColor } from '~/services/item-service'

const { item } = defineProps<{
  item: Item
}>()

const { thumb } = useItem(() => item)
const rankColor = computed(() => getRankColor(item.rank))
</script>

<template>
  <div
    class="
      relative h-24 items-center justify-center rounded-md bg-elevated ring-2 ring-transparent
      hover:ring-accented
    "
    :style="[
      {
        ...(item.rank > 0 && {
          backgroundColor: `color-mix(in srgb, #000 100%, ${rankColor} 25%)`,
        }),
      },
    ]"
  >
    <ItemThumb :thumb :name="item.name" />

    <div class="absolute top-1 left-1 z-10 flex items-center gap-1">
      <ItemRankIcon
        v-if="item.rank > 0"
        :rank="item.rank"
        class="cursor-default"
      />
      <slot name="badges-top-left" />
    </div>

    <div class="absolute top-1 right-1 z-10 flex items-center gap-1">
      <slot name="badges-top-right" />
    </div>

    <div class="absolute bottom-1 left-1 z-10 flex items-center gap-1">
      <slot name="badges-bottom-left" />
    </div>

    <div class="absolute right-1 bottom-1 z-10 flex items-center gap-1">
      <slot name="badges-bottom-right" />
    </div>
  </div>
</template>

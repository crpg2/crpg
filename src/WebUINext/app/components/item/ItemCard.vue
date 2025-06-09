<script setup lang="ts">
import type { Item } from '~/models/item'

import { useItem } from '~/composables/item/use-item'

const { item } = defineProps<{
  item: Item
}>()

const { thumb } = useItem(() => item)
</script>

<template>
  <article class="h-24 items-center justify-center space-y-1 rounded-md bg-base-200 ring-2 ring-transparent hover:ring-border-200">
    <div class="relative h-full">
      <NuxtImg
        v-slot="{ src, isLoaded, imgAttrs }"
        :src="thumb"
        :alt="item.name"
        :custom="true"
      >
        <img
          v-if="isLoaded"
          v-bind="imgAttrs"
          class="size-full object-contain select-none"
          :src="src"
        >
        <!-- placeholder -->
        <div v-else class="flex size-full flex-col items-center justify-center gap-1 overflow-hidden p-2 text-center  text-dimmed">
          <UIcon
            name="crpg:error"
            class="size-10"
          />
          <div class="w-full truncate text-2xs">
            {{ item.name }}{{ item.name }}{{ item.name }}{{ item.name }}
          </div>
        </div>
      </NuxtImg>

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
  </article>
</template>

<script setup lang="ts">
import { useItem } from '~/composables/item/use-item'

const { baseId, rank, name } = defineProps<{
  baseId: string
  rank: number
  name: string
}>()

const { rankColor, thumb } = useItem(() => ({ baseId, rank }))
</script>

<template>
  <div class="flex items-center gap-2.5">
    <div class="relative h-16 w-32 min-w-32">
      <UTooltip
        :content="{ side: 'right' }"
        :ui="{ content: 'p-5 h-auto w-[512px] flex-col gap-2' }"
      >
        <div class="size-full">
          <ItemThumb :thumb :name />
        </div>

        <template #content>
          <div class="h-[240px] w-full">
            <ItemThumb :thumb :name />
          </div>
          <div :style="{ color: rankColor }">
            {{ name }}
          </div>
        </template>
      </UTooltip>

      <!-- top-left -->
      <div class="absolute top-0 left-0 flex items-center gap-1.5">
        <ItemRankIcon
          v-if="rank > 0"
          class="
            cursor-default opacity-80
            hover:opacity-100
          "
          :rank
        />

        <slot name="top-left" />
      </div>

      <!-- bottom-right -->
      <div class="absolute right-0 bottom-0 flex items-center gap-1.5">
        <slot name="bottom-right" />
      </div>

      <!-- top-right -->
      <div class="absolute top-0 right-0 flex items-center gap-1.5">
        <slot name="top-right" />
      </div>
    </div>

    <div class="space-y-2">
      <UiTextView
        variant="p-sm"
        :style="{ color: rankColor }"
        class="truncate font-medium whitespace-pre-wrap"
      >
        {{ name }}
      </UiTextView>

      <slot name="name-caption" />
    </div>
  </div>
</template>

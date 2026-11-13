<script setup lang="ts">
import type { ItemTheme } from '~/models/item'

import { UBadge } from '#components'

const { themes, max = 1 } = defineProps<{
  themes: ItemTheme[]
  max?: number
}>()

const list = computed(() => themes ?? [])
const limit = computed(() => max ?? 1)
const visible = computed(() => list.value.slice(0, limit.value))
const overflowCount = computed(() => Math.max(0, list.value.length - limit.value))
</script>

<template>
  <div v-if="list.length" class="flex flex-wrap items-center gap-1">
    <UBadge
      v-for="theme in visible"
      :key="theme.id"
      :label="theme.name"
      color="neutral"
      variant="subtle"
      size="sm"
    />

    <UTooltip v-if="overflowCount">
      <UBadge
        :label="`+${overflowCount}`"
        color="neutral"
        variant="soft"
        size="sm"
        class="cursor-default"
      />
      <template #content>
        <div class="flex flex-col gap-1 p-1">
          <span v-for="theme in list" :key="theme.id">{{ theme.name }}</span>
        </div>
      </template>
    </UTooltip>
  </div>
</template>

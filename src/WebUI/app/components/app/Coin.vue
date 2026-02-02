<script setup lang="ts">
import { isString } from 'es-toolkit'

import type { DataMediaSize } from '~/components/ui/data/DataMedia.vue'

const { compact = false } = defineProps<{
  value?: number | string
  size?: DataMediaSize
  compact?: boolean
}>()

defineSlots<{
  default: (props: { classes: () => string }) => any
}>()
</script>

<template>
  <UiDataMedia :size class="font-bold text-gold">
    <template #icon="{ classes }">
      <UiSpriteSymbol name="coin" viewBox="0 0 18 18" :class="classes()" />
    </template>

    <template #default="{ classes }">
      <slot v-bind="{ classes }">
        <span
          v-if="value !== undefined"
          :class="classes()"
        >
          {{ isString(value) ? value : $n(value, compact ? 'compact' : '') }}
        </span>
      </slot>
    </template>
  </UiDataMedia>
</template>

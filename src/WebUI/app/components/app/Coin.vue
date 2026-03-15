<script setup lang="ts">
import { isString } from 'es-toolkit'

import type { DataMediaSize } from '~/components/ui/data/DataMedia.vue'

const { compact = false, hiddenIcon = false } = defineProps<{
  value?: number | string
  size?: DataMediaSize
  compact?: boolean
  hiddenIcon?: boolean
}>()
</script>

<template>
  <UiDataMedia :size class="font-bold text-gold">
    <template v-if="!hiddenIcon" #icon="{ classes }">
      <UiSpriteSymbol name="coin" viewBox="0 0 18 18" :class="classes()" />
    </template>

    <template #default="{ classes }">
      <slot v-bind="{ classes }">
        <span
          v-if="value !== undefined"
          :class="classes()"
        >
          {{ isString(value) ? value : $n(value, compact ? 'compact' : '') }}
          <slot name="trailing" />
        </span>
      </slot>
    </template>
  </UiDataMedia>
</template>

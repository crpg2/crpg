<script setup lang="ts">
import { isString } from 'es-toolkit'

import type { DataMediaSize } from '~/components/ui/data/DataMedia.vue'

defineProps<{
  value?: number | string
  size?: DataMediaSize
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
          {{ isString(value) ? value : $n(value) }}
        </span>
      </slot>
    </template>
  </UiDataMedia>
</template>

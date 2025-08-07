<script setup lang="ts">
import type { DataMediaSize } from '~/components/ui/DataMedia.vue'

defineProps<{
  point?: number
  size?: DataMediaSize
}>()
</script>

<template>
  <UiDataMedia class="font-bold text-primary" icon="crpg:blacksmith">
    <template #default="{ classes }">
      <slot v-bind="{ classes }">
        <span
          v-if="point !== undefined"
          :class="classes()"
        >
          {{ $n(point) }}
        </span>
      </slot>
    </template>
  </UiDataMedia>
</template>

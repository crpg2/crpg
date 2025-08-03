<script setup lang="ts">
import type { DataMediaSize } from '~/components/ui/DataMedia.vue'

defineProps<{
  value?: number
  size?: DataMediaSize
}>()
</script>

<template>
  <UiDataMedia :size>
    <template #icon="{ classes }">
      <UIcon
        name="crpg:experience"
        :class="classes()"
      />
    </template>
    <template #default="{ classes }">
      <slot>
        <span v-if="value !== undefined" :class="classes()">
          {{ $n(value) }}
        </span>
      </slot>
    </template>
  </UiDataMedia>
</template>

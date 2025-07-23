<script setup lang="ts">
import type { DataMediaSize } from '~/components/ui/DataMedia.vue'

defineProps<{
  point?: number
  size?: DataMediaSize
}>()
</script>

<template>
  <UTooltip :text="$t('user.field.heirloom')" class="font-bold text-primary">
    <UiDataMedia icon="crpg:blacksmith">
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
  </UTooltip>
</template>

<script setup lang="ts">
import { argbIntToRgbHexColor } from '~/utils/color'

defineProps<{
  color: number
}>()
</script>

<template>
  <UIcon name="crpg:clan-tag" class="size-6" :style="{ color: argbIntToRgbHexColor(color) }" />

  <!-- <OIcon
    icon="tag"
    size="sm"
    :style="{
      'color': '#fff',
      '--fa-primary-opacity': 0.15,
      '--fa-secondary-color': argbIntToRgbHexColor(color),
    }"
  /> -->
</template>

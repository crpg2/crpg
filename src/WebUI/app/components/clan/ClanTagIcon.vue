<script setup lang="ts">
import { argbIntToRgbHexColor } from '~/utils/color'

defineProps<{
  color: number
}>()
</script>

<template>
  <UIcon
    name="crpg:clan-tag"
    :style="{ color: argbIntToRgbHexColor(color) }"
  />
</template>

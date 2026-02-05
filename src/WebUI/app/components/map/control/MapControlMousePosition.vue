<script setup lang="ts">
import type { LatLng, LeafletMouseEvent, Map } from 'leaflet'

import { LControl } from '@vue-leaflet/vue-leaflet'

const map = ref<Map | null>(null)
const mousePosition = ref<LatLng | null>(null)

const onMouseMove = (e: LeafletMouseEvent) => {
  mousePosition.value = e.latlng
}

const onReady = (leafletObject: typeof LControl) => {
  map.value = leafletObject._map as Map
  map.value.on('mousemove', onMouseMove)
}

onBeforeUnmount(() => {
  map.value!.off('mousemove', onMouseMove)
})

const formatNumber = (n: number): string => {
  // TODO: SPEC
  const whole = Math.trunc(n)
  const decimal = Math.trunc(Math.abs(n % 1) * 1000)

  const wholeStr = (whole < 0 ? '-' : '') + Math.abs(whole).toString().padStart(3, '0')
  const decimalStr = decimal.toString().padStart(3, '0')
  return `${wholeStr}.${decimalStr}`
}

const mousePositionText = computed(() => {
  if (mousePosition.value === null) {
    return '0.0 0.0'
  }

  return `${formatNumber(mousePosition.value.lng)} ${formatNumber(mousePosition.value.lat)}`
})
</script>

<template>
  <LControl @ready="onReady">
    <div
      class="rounded-xl bg-default p-2 text-highlighted"
    >
      {{ mousePositionText }}
    </div>
  </LControl>
</template>

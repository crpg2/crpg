<script setup lang="ts">
import type { LatLngLiteral, LayerGroup } from 'leaflet'

import { LLayerGroup, LPopup } from '@vue-leaflet/vue-leaflet'

import type { MovementType } from '~/models/strategus/movement'

const { latLng } = defineProps<{ latLng: LatLngLiteral, movementTypes: MovementType[] }>()

const emit = defineEmits<{
  cancel: []
  confirm: [movementType: MovementType]
}>()

const layerGroup = ref<typeof LLayerGroup | null>(null)

const onCancel = () => emit('cancel')

onMounted(() => {
  (layerGroup.value!.leafletObject as LayerGroup).on('popupclose', onCancel)
})

onBeforeUnmount(() => {
  (layerGroup.value!.leafletObject as LayerGroup).off('popupclose', onCancel)
})

watch(
  () => latLng,
  () => {
    nextTick().then(() => {
      (layerGroup.value!.leafletObject as LayerGroup).openPopup(latLng)
    })
  },
  { immediate: true },
)
</script>

<template>
  <LLayerGroup ref="layerGroup">
    <LPopup
      :lat-lng="latLng"
      :options="{ className: 'move-popup', offset: [0, -8] }"
    >
      <div class="mt-4 flex flex-col gap-1 p-2">
        <button
          v-for="mt in movementTypes"
          :key="mt"
          class="
            border-base-500 rounded-sm border px-4 py-2
            hover:ring
          "
          @click="emit('confirm', mt)"
        >
          {{ $t(`strategus.movementType.${mt}`) }}
        </button>
      </div>
    </LPopup>
  </LLayerGroup>
</template>

<style>
/* .move-popup .leaflet-popup-content {
  @apply m-0;
} */
</style>

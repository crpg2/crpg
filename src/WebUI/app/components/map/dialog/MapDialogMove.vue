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
      :options="{ className: 'move-popup', offset: [0, -16] }"
    >
      <div class="flex w-full flex-col items-center justify-center gap-2">
        <UButton
          v-for="mt in movementTypes"
          :key="mt"
          block
          color="neutral"
          variant="subtle"
          @click="emit('confirm', mt)"
        >
          {{ $t(`strategus.movementType.${mt}`) }}
        </UButton>
      </div>
    </LPopup>
  </LLayerGroup>
</template>

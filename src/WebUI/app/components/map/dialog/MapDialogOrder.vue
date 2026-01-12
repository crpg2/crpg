<script setup lang="ts">
import type { LatLngLiteral, LayerGroup } from 'leaflet'

import { LLayerGroup, LPopup } from '@vue-leaflet/vue-leaflet'

import type { PartyOrderType } from '~/models/strategus/party'

const { latLng } = defineProps<{
  latLng: LatLngLiteral
  orders: PartyOrderType[]
}>()

const emit = defineEmits<{
  cancel: []
  confirm: [order: PartyOrderType]
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
      :options="{ offset: [0, -16] }"
    >
      <div class="flex w-full flex-col items-center justify-center gap-2">
        <UButton
          v-for="(order, idx) in orders"
          :key="`${order}_${idx}`"
          block
          color="neutral"
          variant="subtle"
          @click="emit('confirm', order)"
        >
          {{ $t(`strategus.partyOrderType.${order}`) }}
        </UButton>
      </div>
    </LPopup>
  </LLayerGroup>
</template>

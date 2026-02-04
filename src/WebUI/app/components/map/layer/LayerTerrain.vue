<script setup lang="ts">
import type { GeoJSON } from 'leaflet'
import type L from 'leaflet'

import { LGeoJson } from '@vue-leaflet/vue-leaflet'

import type { TerrainFeature, TerrainFeatureCollection } from '~/models/strategus/terrain'

import { terrainColorByType } from '~/services/strategus/terrain-service'

const { data } = defineProps<{
  data: TerrainFeatureCollection
}>()

const emit = defineEmits<{
  update: [e: L.PM.UpdateEventHandler]
}>()

const { t } = useI18n()
const geoJSON = ref<typeof LGeoJson | null>(null)

const terrainGeoJSONStyle = (feature: TerrainFeature) => ({
  color: terrainColorByType[feature.properties.type],
})

const onEachFeatureFunction = (feature: TerrainFeature, layer: L.Polygon) => {
  // @ts-expect-error TODO:
  layer.internalId = feature.id
  // @ts-expect-error TODO:
  layer.properties = feature.properties

  // @ts-expect-error TODO:
  layer.on('pm:update', event => emit('update', event))
  // @ts-expect-error TODO:
  layer.on('pm:dragend', event => emit('update', event))

  // @ts-expect-error TODO:
  layer.on('pm:edit', event => emit('update', event))

  // TODO: style
  layer.bindTooltip(
    `<div>${t(`strategus.terrainType.${feature.properties.type}`)} - TODO: (ex. penalty)</div>`,
    {
      permanent: false,
      sticky: true,
      direction: 'bottom',
      offset: [0, 16],
    },
  )
}

const terrainGeoJSONOptions = {
  onEachFeature: onEachFeatureFunction,
}

const onReady = () => {
  if (geoJSON.value === null) {
    return
  }
  (geoJSON.value.leafletObject as GeoJSON).bringToBack() // TODO:
}
</script>

<template>
  <LGeoJson
    ref="geoJSON"
    :geojson="data"
    :options-style="(feature) => feature ? terrainGeoJSONStyle(feature as TerrainFeature) : {}"
    :options="terrainGeoJSONOptions"
    @ready="onReady"
  />
</template>

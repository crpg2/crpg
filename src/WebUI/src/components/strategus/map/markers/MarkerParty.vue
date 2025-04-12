<script setup lang="ts">
import type { CircleMarker } from 'leaflet'

import { LCircleMarker, LTooltip } from '@vue-leaflet/vue-leaflet'
import { strategusMaxPartyTroops, strategusMinPartyTroops } from '~root/data/constants.json'

import type { PartyCommon } from '~/models/strategus/party'

import { positionToLatLng } from '~/utils/geometry'

const { isSelf = false, party } = defineProps<{ party: PartyCommon, isSelf?: boolean }>()

const circleMarker = useTemplateRef('circleMarker')

const minRadius = 4
const maxRadius = 10

// TODO: FIXME: tweak
const markerRadius = computed(() => {
  const troopsRange = strategusMaxPartyTroops - strategusMinPartyTroops // strategusMaxPartyTroops = 300?
  const sizeFactor = party.troops / troopsRange
  return minRadius + sizeFactor * (maxRadius - minRadius)
})

// TODO: clan mates
const markerColor = computed(() => (isSelf ? '#34d399' : '#ef4444')) // TODO: colors

const onReady = () => {
  if (circleMarker.value === null) { return }

  (circleMarker.value.leafletObject as CircleMarker).bringToFront()
}
</script>

<template>
  <LCircleMarker
    ref="circleMarker"
    :lat-lng="positionToLatLng(party.position.coordinates)"
    :radius="markerRadius"
    :color="markerColor"
    :fill-color="markerColor"
    :fill-opacity="1.0"
    :bubbling-mouse-events="false"
    @ready="onReady"
  >
    <LTooltip :options="{ direction: 'top', offset: [0, -8] }">
      {{ party.user.name }} ({{ party.troops }})
    </LTooltip>
  </LCircleMarker>
</template>

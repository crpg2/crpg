<script setup lang="ts">
import type { CircleMarker, Marker } from 'leaflet'

import { LCircleMarker, LIcon, LMarker, LTooltip } from '@vue-leaflet/vue-leaflet'
import { strategusMaxPartyTroops, strategusMinPartyTroops } from '~root/data/constants.json'

import type { PartyVisible } from '~/models/strategus/party'

import { positionToLatLng } from '~/utils/geometry'

const { isSelf = false, party } = defineProps<{ party: PartyVisible, isSelf?: boolean }>()

const minRadius = 4
const maxRadius = 10

// TODO: FIXME: tweak
const markerRadius = computed(() => {
  const troopsRange = strategusMaxPartyTroops - strategusMinPartyTroops // strategusMaxPartyTroops = 300?
  const sizeFactor = party.troops / troopsRange
  return minRadius + sizeFactor * (maxRadius - minRadius)
})

// TODO: FIXME: clan mates,
const markerColor = computed(() => (isSelf ? '#34d399' : '#ef4444')) // TODO: colors

const onReady = (circleMarker: CircleMarker) => {
//   setTimeout(() => {
//     isSelf && circleMarker.bringToFront()
//   }, 1100)
}
</script>

<template>
  <LMarker
    :lat-lng="positionToLatLng(party.position.coordinates)"
    :options="{ bubblingMouseEvents: false }"
  >
    <LIcon
      :icon-size="[32, 32]"
      class-name="!flex justify-center items-center"
    >
      <!-- <svg width="32" height="32" viewBox="0 0 24 24">
        <circle
          cx="12" cy="12"
          r="6"
          fill="orange"
        >
          <animate
            attributeName="r" values="6;10;6"
            dur="1.5s"
            repeatCount="indefinite"
          />
          <animate
            attributeName="opacity" values="1;0.5;1"
            dur="1.5s"
            repeatCount="indefinite"
          />
        </circle>
      </svg> -->

      <UserAvatar
        :avatar="party?.user.avatar || ''"
        :name="party?.user.name || ''"
        size="xl"
        :is-self
      />
    </LIcon>

    <LTooltip :options="{ direction: 'top', offset: [0, -8] }">
      {{ party.user.name }} ({{ party.troops }})
    </LTooltip>
  </LMarker>

  <!--
   <LCircleMarker
    :lat-lng="positionToLatLng(party.position.coordinates)"
    :radius="markerRadius"
    :color="markerColor"
    :fill-color="markerColor"
    :fill-opacity="1.0"
    pane="partyPane"
    :bubbling-mouse-events="false"
    @ready="onReady"
  >
    <LTooltip :options="{ direction: 'top', offset: [0, -8] }">
      {{ party.user.name }} ({{ party.troops }})
    </LTooltip>
  </LCircleMarker> -->
</template>

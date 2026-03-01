<script setup lang="ts">
import type { LatLng } from 'leaflet'

import { LPolyline, LTooltip } from '@vue-leaflet/vue-leaflet'

import type { Party } from '~/models/strategus/party'

import { PARTY_ORDER_TYPE } from '~/models/strategus/party'
import { positionToLatLng } from '~/utils/geometry'

const { party } = defineProps<{ party: Party }>()
const { n } = useI18n()
// TODO: colors
const attackColor = '#f14668'
const followColor = '#10b981'
const moveColor = '#485fc7'

interface PathSegmentWithMeta {
  latLngs: LatLng[]
  color: string
  distance: number
  speed: number
  speedMultiplier: number
  tooltip: string
}

const partyMovementSegments = computed<PathSegmentWithMeta[]>(() => {
  if (!party.orders.length) {
    return []
  }

  const segments: PathSegmentWithMeta[] = []

  for (const order of party.orders.toSorted((a, b) => a.orderIndex - b.orderIndex)) {
    let color = moveColor

    switch (order.type) {
      case PARTY_ORDER_TYPE.MoveToPoint:
        color = moveColor
        break

      case PARTY_ORDER_TYPE.FollowParty:
      case PARTY_ORDER_TYPE.TransferOfferParty:
        color = followColor
        break

      case PARTY_ORDER_TYPE.AttackParty:
      case PARTY_ORDER_TYPE.AttackSettlement:
        color = attackColor
        break

      case PARTY_ORDER_TYPE.MoveToSettlement:
      case PARTY_ORDER_TYPE.JoinBattle:
        color = moveColor
        break
    }

    if (order.pathSegments && order.pathSegments.length > 0) {
      for (const segment of order.pathSegments) {
        const latLngs = [
          positionToLatLng(segment.startPoint.coordinates),
          positionToLatLng(segment.endPoint.coordinates),
        ]

        const tooltip = `Distance: ${segment.distance.toFixed(2)}\nSpeed: ${segment.speed.toFixed(2)}\nMultiplier: ${segment.speedMultiplier.toFixed(2)}x`

        segments.push({
          latLngs,
          color,
          distance: segment.distance,
          speed: segment.speed,
          speedMultiplier: segment.speedMultiplier,
          tooltip,
        })
      }
    }
  }

  return segments
})

const onReady = (line: typeof LPolyline) => {
  const { speed, speedMultiplier, distance } = line.options.segment
  line.setText(`${n(distance, { maximumFractionDigits: 2 })}m / ${n(speed, { maximumFractionDigits: 2 })}m/s`, {
    offset: 16,
    center: true,
    attributes: {
      'fill': 'white',
      'font-size': '10',
    },
  })
}
</script>

<template>
  <template v-for="(segment, index) in partyMovementSegments" :key="`segment-${index}`">
    <LPolyline
      :lat-lngs="segment.latLngs"
      :color="segment.color"
      dash-array="10, 10"
      dash-offset="10"
      line-cap="round"
      line-join="bevel"
      :options="{ pmIgnore: true, segment }"
      @ready="onReady"
      @mouseover="(e:any) => {
        // console.log(e.target);
        // e.target.setStyle({
        //   color: 'blue',
        //   opacity: 1,
        //   weight: 5,
        // });
      }"
    >
      <LTooltip :options="{ permanent: false, direction: 'center', className: 'bg-elevated/90 border-transparent shadow-md' }">
        <div class="text-xs whitespace-nowrap">
          <div><strong>Distance:</strong> {{ segment.distance.toFixed(2) }}</div>
          <div><strong>Speed:</strong> {{ segment.speed.toFixed(2) }}</div>
          <div><strong>×{{ segment.speedMultiplier.toFixed(2) }}</strong></div>
        </div>
      </LTooltip>
    </LPolyline>
  </template>
</template>

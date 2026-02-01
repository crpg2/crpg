<script setup lang="ts">
import type { Position } from 'geojson'

import { LPolyline } from '@vue-leaflet/vue-leaflet'

import type { Party } from '~/models/strategus/party'

import { PARTY_ORDER_TYPE } from '~/models/strategus/party'
import { positionToLatLng } from '~/utils/geometry'

const { party } = defineProps<{ party: Party }>()

// TODO: colors
const attackColor = '#f14668'
const followColor = '#10b981'
const moveColor = '#485fc7'

const partyMovementLine = computed(() => {
  if (!party.orders.length) {
    return null
  }
  const positions: Position[] = []

  let color = moveColor

  for (const order of party.orders.toSorted((a, b) => a.orderIndex - b.orderIndex)) {
    switch (order.type) {
      case PARTY_ORDER_TYPE.MoveToPoint:
        positions.push(...order.waypoints.coordinates)
        color = moveColor
        break

      case PARTY_ORDER_TYPE.FollowParty:
      case PARTY_ORDER_TYPE.TransferOfferParty:
        if (order.targetedParty) {
          positions.push(order.targetedParty.position.coordinates)
          color = followColor
        }
        break

      case PARTY_ORDER_TYPE.AttackParty:
        if (order.targetedParty) {
          positions.push(order.targetedParty.position.coordinates)
          color = attackColor
        }
        break

      case PARTY_ORDER_TYPE.MoveToSettlement:
        if (order.targetedSettlement) {
          positions.push(order.targetedSettlement.position.coordinates)
          color = moveColor
        }
        break

      case PARTY_ORDER_TYPE.AttackSettlement:
        if (order.targetedSettlement) {
          positions.push(order.targetedSettlement.position.coordinates)
          color = attackColor
        }
        break

      case PARTY_ORDER_TYPE.JoinBattle:
        if (order.targetedBattle) {
          positions.push(order.targetedBattle.position.coordinates)
          color = moveColor
        }
        break
    }
  }

  return {
    color,
    dashArray: '10, 10',
    dashOffset: '10',
    lineCap: 'round',
    lineJoin: 'bevel',
    latLngs: [party.position.coordinates, ...positions].map(positionToLatLng),
    options: {
      pmIgnore: true,
    },
  }
})
</script>

<template>
  <LPolyline
    v-if="partyMovementLine !== null"
    v-bind="partyMovementLine"
  />
</template>

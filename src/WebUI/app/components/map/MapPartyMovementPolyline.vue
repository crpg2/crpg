<script setup lang="ts">
import type { Position } from 'geojson'

import { LPolyline } from '@vue-leaflet/vue-leaflet'

import type { Party } from '~/models/strategus/party'

import { PARTY_STATUS } from '~/models/strategus/party'
import { positionToLatLng } from '~/utils/geometry'

const { party } = defineProps<{ party: Party }>()

// TODO: colors
// TODO: to service
const attackColor = '#f14668'
const followColor = '#10b981'
const moveColor = '#485fc7'

const partyMovementLine = computed(() => {
  let color: string
  const positions: Position[] = []

  switch (party.status) {
    case PARTY_STATUS.MovingToPoint:
      positions.push(...party.waypoints.coordinates)
      color = moveColor
      break
    case PARTY_STATUS.FollowingParty:
      if (party.targetedParty) {
        positions.push(party.targetedParty.position.coordinates)
      }
      color = followColor
      break
    case PARTY_STATUS.MovingToSettlement:
      if (party.targetedSettlement) {
        positions.push(party.targetedSettlement.position.coordinates)
      }
      color = moveColor
      break
    case PARTY_STATUS.MovingToAttackParty:
      if (party.targetedParty) {
        positions.push(party.targetedParty.position.coordinates)
      }
      color = attackColor
      break
    case PARTY_STATUS.MovingToAttackSettlement:
      if (party.targetedSettlement) {
        positions.push(party.targetedSettlement.position.coordinates)
      }
      color = attackColor
      break
    case PARTY_STATUS.MovingToBattle:
      if (party.targetedBattle) {
        positions.push(party.targetedBattle.position.coordinates)
      }
      color = moveColor
      break
    default:
      return null
  }

  return {
    // TODO: ts
    color,
    dashArray: '10, 10',
    dashOffset: '10',
    latLngs: [party.position.coordinates, ...positions].map(positionToLatLng),
    // TODO:
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

<script setup lang="ts">
import type { LeafletMouseEvent } from 'leaflet'

import { LCircle, LIcon, LMarker, LTooltip } from '@vue-leaflet/vue-leaflet'
import { strategusMaxPartyTroops, strategusMinPartyTroops } from '~root/data/constants.json'

import type { PartyVisible } from '~/models/strategus/party'

import { positionToLatLng } from '~/utils/geometry'

const { isSelf = false, party } = defineProps<{ party: PartyVisible, isSelf?: boolean }>()

defineEmits<{ click: [LeafletMouseEvent] }>()

// const minRadius = 4
// const maxRadius = 10

// // TODO: FIXME: tweak
// const markerRadius = computed(() => {
//   const troopsRange = strategusMaxPartyTroops - strategusMinPartyTroops // strategusMaxPartyTroops = 300?
//   const sizeFactor = party.troops / troopsRange
//   return minRadius + sizeFactor * (maxRadius - minRadius)
// })

// TODO: FIXME: clan mates,
const markerColor = computed(() => (isSelf ? '#34d399' : '#ef4444')) // TODO: colors
</script>

<template>
  <div>
    <LMarker
      :lat-lng="positionToLatLng(party.position.coordinates)"
      :options="{ bubblingMouseEvents: false }"
      @click="(e: LeafletMouseEvent) => $emit('click', e)"
    >
      <LIcon
        :icon-size="[32, 32]"
        class-name="flex! justify-center items-center"
      >
        <UserAvatar
          :avatar="party?.user.avatar || ''"
          :name="party?.user.name || ''"
          size="xl"
          :is-self
        />
      </LIcon>

      <LTooltip :options="{ direction: 'top', offset: [0, -24] }">
        <UiDataCell>
          <template #leftContent>
            <UserMedia
              :user="party.user"
              :is-self
            />
          </template>
          <UiDataMedia icon="crpg:member" :label="$n(party.troops)" />
        </UiDataCell>
      </LTooltip>
    </LMarker>

    <LCircle
      v-if="isSelf"
      :lat-lng="positionToLatLng(party.position.coordinates)"
      :radius="8"
      :opacity="0"
      :interactive="false"
      :fill-opacity="0.25"
      fill-color="#ccc"
    />
  </div>
</template>

<script setup lang="ts">
import type { Marker } from 'leaflet'

import { LIcon, LMarker, LTooltip } from '@vue-leaflet/vue-leaflet'
import { clsx } from 'clsx'

import type { SettlementPublic } from '~/models/strategus/settlement'

import { SettlementType } from '~/models/strategus/settlement'
import { settlementIconByType } from '~/services/strategus-service/settlement'
import { argbIntToRgbHexColor } from '~/utils/color'
// import { hexToRGBA } from '~/utils/color'
import { positionToLatLng } from '~/utils/geometry'

const { settlement } = defineProps<{ settlement: SettlementPublic }>()

const settlementMarkerStyle = computed(() => {
  const output = {
    ...settlementIconByType[settlement.type],
    baseClass: '',
    baseStyle: '',
  }

  switch (settlement.type) {
    case SettlementType.Town:
      output.baseClass = clsx('gap-2 px-2 py-1.5 text-sm')
      break
    case SettlementType.Castle:
      output.baseClass = clsx('gap-1.5 px-1.5 py-1 text-xs')
      break
    case SettlementType.Village:
      output.baseClass = clsx('gap-1 p-1 text-2xs')
      break
  }

  if (settlement?.owner?.clanMembership) {
    output.baseStyle = `background-color: ${argbIntToRgbHexColor(settlement.owner.clanMembership.clan.primaryColor)}50;` // 50% opacity
  }

  return output
})

const onReady = (marker: Marker) => {
}
</script>

<template>
  <LMarker
    :lat-lng="positionToLatLng(settlement.position.coordinates)"
    :options="{ bubblingMouseEvents: false }"
    @ready="onReady"
  >
    <LIcon class-name="!flex justify-center items-center">
      <div
        :style="settlementMarkerStyle.baseStyle"
        class="flex items-center whitespace-nowrap rounded-md bg-base-100/50 text-white hover:ring"
        :class="settlementMarkerStyle.baseClass"
        :title="$t(`strategus.settlementType.${settlement.type}`)"
      >
        <OIcon :icon="settlementMarkerStyle.icon" :size="settlementMarkerStyle.iconSize" />
        <div class="leading-snug">
          {{ settlement.name }}
        </div>

        <div v-if="settlement?.owner?.clanMembership" class="flex items-center">
          <ClanTagIcon :color="settlement.owner.clanMembership.clan.primaryColor" size="xl" />
          [{{ settlement.owner.clanMembership.clan.tag }}]
        </div>
      </div>
    </LIcon>

    <LTooltip :options="{ direction: 'top', offset: [0, -16] }">
      <div>
        <div class="flex min-w-80 flex-col gap-2 p-2">
          <SettlementMedia :settlement="settlement!" />

          <div v-tooltip.bottom="`Troops`" class="flex items-center gap-1.5">
            <OIcon icon="member" size="lg" />
            {{ settlement!.troops }}
          </div>

          <Coin :value="10000" />

          <div v-if="settlement.owner" class="flex flex-col gap-1">
            <span class="text-3xs text-content-300">Owner</span>
            <UserMedia :user="settlement.owner" />
          </div>
        </div>
      </div>
    </LTooltip>
  </LMarker>
</template>

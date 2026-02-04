<script setup lang="ts">
import { LCircle, LIcon, LMarker, LTooltip } from '@vue-leaflet/vue-leaflet'
// import { clsx } from 'clsx'

import type { SettlementPublic } from '~/models/strategus/settlement'

import { SETTLEMENT_TYPE } from '~/models/strategus/settlement'
// import { SettlementType } from '~/models/strategus/settlement'
import { settlementIconByType } from '~/services/strategus/settlement-service'
import { argbIntToRgbHexColor } from '~/utils/color'
// import { hexToRGBA } from '~/utils/color'
import { positionToLatLng } from '~/utils/geometry'

const { settlement } = defineProps<{ settlement: SettlementPublic }>()

defineEmits<{ click: [] }>()

const settlementMarkerStyle = computed(() => {
  const output = {
    baseClass: '',
    baseStyle: '',
  }

  switch (settlement.type) {
    case SETTLEMENT_TYPE.Town:
      output.baseClass = tw`gap-2 px-3.5 py-1.5 text-lg`
      break
    case SETTLEMENT_TYPE.Castle:
      output.baseClass = tw`gap-1.5 px-1.5 py-1 text-sm`
      break
    case SETTLEMENT_TYPE.Village:
      output.baseClass = tw`text-xs gap-1 p-1`
      break
  }

  if (settlement?.owner?.clanMembership) {
    output.baseStyle = `background-color: color-mix(in srgb, #fff 5%, ${argbIntToRgbHexColor(settlement.owner.clanMembership.clan.primaryColor)} 25%)`
  }

  return output
})

const settlementAreaRadius = computed(() => {
  switch (settlement.type) {
    case SETTLEMENT_TYPE.Town:
      return 1.375
    case SETTLEMENT_TYPE.Castle:
      return 0.825
    case SETTLEMENT_TYPE.Village:
    default:
      return 0.375
  }
})
</script>

<template>
  <div>
    <LMarker
      :lat-lng="positionToLatLng(settlement.position.coordinates)"
      :options="{ bubblingMouseEvents: false }"
      @click="$emit('click')"
    >
      <LIcon class-name="!flex justify-center items-center">
        <div
          :style="settlementMarkerStyle.baseStyle"
          class="
            flex items-center rounded-md bg-muted/75 whitespace-nowrap text-highlighted
            hover:ring hover:ring-inverted
          "
          :class="settlementMarkerStyle.baseClass"
          :title="$t(`strategus.settlementType.${settlement.type}`)"
        >
          <UIcon :name="`crpg:${settlementIconByType[settlement.type]}`" class="size-6" />
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
        <div class="flex min-w-80 flex-col gap-2 p-2">
          <SettlementMedia :settlement />

          <!-- v-tooltip.bottom="`Troops`" -->
          <div class="flex items-center gap-1.5">
            <!-- <OIcon icon="member" size="lg" /> -->
            {{ settlement.troops }}
          </div>

          <div v-if="settlement.owner" class="flex flex-col gap-1">
            <span class="">Owner</span>
            <UserMedia :user="settlement.owner" />
          </div>
        </div>
      </LTooltip>
    </LMarker>

    <!-- :visible="hovered" -->
    <LCircle
      :lat-lng="positionToLatLng(settlement.position.coordinates)"
      :radius="settlementAreaRadius"
      :opacity="0"
      :fill-opacity="0.25"
      fill-color="tomato"
      :options="{
        pmIgnore: true,
      }"
    />
  </div>
</template>

<script setup lang="ts">
import { LCircle, LIcon, LMarker, LTooltip } from '@vue-leaflet/vue-leaflet'

import { positionToLatLng } from '~/utils/geometry'

const { battle } = defineProps<{ battle: any }>()

defineEmits<{ click: [] }>()
</script>

<template>
  <div>
    <LMarker
      :lat-lng="positionToLatLng(battle.position.coordinates)"
      :options="{ bubblingMouseEvents: false }"
      @click="$emit('click')"
    >
      <LIcon class-name="!flex justify-center items-center">
        <div>
          <UIcon name="crpg:game-mode-battle" class="size-12" />
        </div>
      </LIcon>

      <!-- <LTooltip :options="{ direction: 'top', offset: [0, -16] }">
        <div class="flex min-w-80 flex-col gap-2 p-2">
          <SettlementMedia :settlement />

          <div class="flex items-center gap-1.5">
            {{ settlement.troops }}
          </div>

          <AppCoin :value="10000" />

          <div v-if="settlement.owner" class="flex flex-col gap-1">
            <span class="text-3xs text-content-300">Owner</span>
            <UserMedia :user="settlement.owner" />
          </div>
        </div>
      </LTooltip> -->
    </LMarker>

    <!-- :visible="hovered" -->

    <!-- <LCircle
      :lat-lng="positionToLatLng(settlement.position.coordinates)"
      :radius="settlementAreaRadius"
      :opacity="0"
      :fill-opacity="0.25"
      fill-color="tomato"
    /> -->
  </div>
</template>

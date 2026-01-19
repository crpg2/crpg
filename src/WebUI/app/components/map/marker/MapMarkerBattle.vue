<script setup lang="ts">
import { LCircle, LIcon, LMarker, LPopup, LTooltip } from '@vue-leaflet/vue-leaflet'

import type { BattleSide } from '~/models/strategus/battle'

import { useMapContext } from '~/composables/strategus/use-map'
import { positionToLatLng } from '~/utils/geometry'

const { battle } = defineProps<{ battle: any }>()

defineEmits<{ join: [side: BattleSide] }>()

const { zoom } = useMapContext()

const showDetail = computed(() => zoom.value > 5)
</script>

<template>
  <LMarker
    :lat-lng="positionToLatLng(battle.position.coordinates)"
    :options="{ bubblingMouseEvents: false }"
  >
    <LIcon class-name="!flex justify-center items-center">
      <div
        class="
          flex items-center justify-center gap-2 rounded-md bg-error/66 px-3 py-2 text-highlighted
          hover:ring hover:ring-inverted
        "
      >
        <div v-if="showDetail" class="flex whitespace-nowrap">
          pipisculus (100)
        </div>

        <UIcon name="crpg:game-mode-duel" :class="zoom > 5 ? 'size-8' : 'size-5'" />

        <div v-if="showDetail" class="flex whitespace-nowrap">
          droob (200)
        </div>
      </div>
    </LIcon>

    <LPopup :options="{ direction: 'top', offset: [0, -16] }">
      <div class="flex min-w-80 flex-col gap-2 p-2">
        TODO: список участников
      </div>
    </LPopup>
  </LMarker>
</template>

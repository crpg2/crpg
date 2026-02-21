<script setup lang="ts">
import type { PartySpeed } from '~/models/strategus/party'

import { terrainIconByType } from '~/services/strategus/terrain-service'

const { speed } = defineProps<{
  speed: PartySpeed
}>()

const { n } = useI18n()

const speedBreakdown = computed(() => {
  const base = speed.baseSpeed
  const afterTroops = base * speed.troopInfluence
  const afterMounts = afterTroops * speed.mountInfluence
  const afterTerrain = afterMounts * speed.terrainInfluence

  return {
    base,
    afterTroops,
    afterMounts,
    afterTerrain,
    deltaTroops: afterTroops - base,
    deltaMounts: afterMounts - afterTroops,
    deltaTerrain: afterTerrain - afterMounts,
  }
})

function formatDelta(value: number) {
  const sign = value >= 0 ? '+' : '-'
  return `${sign}${n(Math.abs(value))}`
}

function deltaClass(value: number) {
  if (value > 0) {
    return tw`text-success`
  }
  if (value < 0) {
    return tw`text-error`
  }
  return tw`text-muted`
}
</script>

<template>
  <div class="flex flex-col gap-1">
    <UiDataCell>
      Base
      <template #rightContent>
        <span class="tabular-nums">{{ $n(speedBreakdown.base) }}</span>
      </template>
    </UiDataCell>

    <UiDataCell>
      Troops
      <template #rightContent>
        <div class="flex items-center gap-1">
          <span class="tabular-nums" :class="[deltaClass(speedBreakdown.deltaTroops)]">
            {{ formatDelta(speedBreakdown.deltaTroops) }}
          </span>
          <span class="text-muted">({{ $n(speedBreakdown.base) }} -> {{ $n(speedBreakdown.afterTroops) }})</span>
        </div>
      </template>
    </UiDataCell>

    <UiDataCell>
      Cavalry
      <template #rightContent>
        <div class="flex items-center gap-1">
          <span class="tabular-nums" :class="[deltaClass(speedBreakdown.deltaMounts)]">
            {{ formatDelta(speedBreakdown.deltaMounts) }}
          </span>
          <span class="text-muted">({{ $n(speedBreakdown.afterTroops) }} -> {{ $n(speedBreakdown.afterMounts) }})</span>
        </div>
      </template>
    </UiDataCell>

    <UiDataCell v-if="speed.currentTerrainType">
      <UiDataMedia label="Terrain" layout="reverse" :icon="terrainIconByType[speed.currentTerrainType]" />
      <template #rightContent>
        <div class="flex items-center gap-1">
          <span class="tabular-nums" :class="[deltaClass(speedBreakdown.deltaTerrain)]">
            {{ formatDelta(speedBreakdown.deltaTerrain) }}
          </span>
          <span class="text-muted">({{ $n(speedBreakdown.afterMounts) }} -> {{ $n(speedBreakdown.afterTerrain) }})</span>
        </div>
      </template>
    </UiDataCell>

    <UiDataCell>
      Final
      <template #rightContent>
        <span class="tabular-nums">{{ $n(speedBreakdown.afterTerrain) }}</span>
      </template>
    </UiDataCell>
  </div>
</template>

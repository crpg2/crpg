<script setup lang="ts">
import type { Party } from '~/models/strategus/party'

const { party } = defineProps<{ party: Party }>()

defineEmits<{
  locate: []
  startMove: []
}>()
</script>

<template>
  <!-- TODO: base cmp - Strategus Map island island -->
  <div
    class="w-[22rem] rounded-3xl bg-base-100/90 p-6 text-content-200 backdrop-blur-sm"
  >
    <div class="flex flex-col gap-2">
      <div class="flex items-center gap-2">
        <div v-tooltip.bottom="party.position.coordinates.join(' ')" @click="$emit('locate')">
          <OIcon icon="crosshair" size="lg" class="cursor-pointer" />
        </div>

        <UserMedia
          :user="party.user"
          hidden-platform
          class="max-w-48"
        />
      </div>

      <Divider class="text-content-500" />

      <div class="flex items-center gap-1.5">
        <Coin :value="10000" />
        <Media icon="member" :label="String(party.troops)" />
      </div>

      <div>Status: {{ party.status }}</div>
      <div>Targeted Settlement: {{ party.targetedSettlement?.name }}</div>
      <div>Targeted Party: {{ party.targetedParty?.name }}</div>

      <!-- TODO: add terrain to party -->
      <div>Terrain: Plain TODO:</div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { UiDataMedia } from '#components'

import type { Party } from '~/models/strategus/party'

const { party } = defineProps<{ party: Party }>()

defineEmits<{
  locate: []
  startMove: []
}>()
</script>

<template>
  <UCard
    variant="subtle"
    class="w-[22rem]"
  >
    <div class="flex flex-col gap-2">
      <div class="flex items-center gap-2">
        <div @click="$emit('locate')">
          <UIcon name="i-lucide-locate-fixed" class="size-6 cursor-pointer" />
        </div>

        <UserMedia
          :user="party.user"
          hidden-platform
          class="max-w-48"
        />
      </div>

      <div class="flex items-center gap-1.5">
        <AppCoin :value="10000" />
        <UiDataMedia icon="member" :label="String(party.troops)" />
      </div>

      <div>Status: {{ party.status }}</div>
      <div>Targeted Settlement: {{ party.targetedSettlement?.name }}</div>
      <div>Targeted Party: {{ party.targetedParty?.user.name }}</div>

      <!-- TODO: add terrain to party -->
      <div>Terrain: Plain TODO:</div>
    </div>
  </UCard>
</template>

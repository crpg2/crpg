<script setup lang="ts">
import { useRouteQuery } from '@vueuse/router'

import type { BattlePhase } from '~/models/strategus/battle'

import { useRegionQuery } from '~/composables/use-region'
import { BATTLE_PHASE } from '~/models/strategus/battle'
import { getBattles } from '~/services/strategus/battle-service'

const { regionModel } = useRegionQuery()

const battlePhaseModel = useRouteQuery<BattlePhase[]>('battlePhases', [BATTLE_PHASE.Scheduled, BATTLE_PHASE.Hiring])

const {
  state: battles,
  // execute: loadBattles,
} = useAsyncState(
  () => getBattles(regionModel.value, battlePhaseModel.value),
  [],
)
</script>

<template>
  <UContainer class="space-y-8 py-12">
    <div
      class="mx-auto max-w-4xl"
    >
      <UiHeading class="mb-14" title="Upcoming Battles" />
    </div>

    <BattleCalendar v-if="battles.length" :battles />
  </UContainer>
</template>

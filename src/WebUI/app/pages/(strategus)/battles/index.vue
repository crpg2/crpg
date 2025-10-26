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
  <div>
    <div>
      wdwd
      {{ battles }}
    </div>
  </div>
</template>

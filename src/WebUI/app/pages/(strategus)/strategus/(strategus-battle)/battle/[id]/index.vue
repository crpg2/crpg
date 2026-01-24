<script setup lang="ts">
import { useClipboard } from '@vueuse/core'
import { groupBy } from 'es-toolkit'

import { useBattleFighterApplications, useBattleFighters, useMapBattle } from '~/composables/strategus/map/use-map-battle'
import { usePartyState } from '~/composables/strategus/use-party'
import { BATTLE_SIDE } from '~/models/strategus/battle'

const toast = useToast()

const { battleFighters, battleFighterAttackers, battleFighterDefenders, loadingBattleFighters } = useBattleFighters()
</script>

<template>
  <div>
    <div class="grid grid-cols-2 gap-8">
      <!-- :can-manage="side === BATTLE_SIDE.Attacker ? canManageAttackerSide : canManageDefenderSide" -->
      <BattleFightersTable
        v-for="side in [BATTLE_SIDE.Attacker, BATTLE_SIDE.Defender]"
        :key="side"
        :fighters="side === BATTLE_SIDE.Attacker ? battleFighterAttackers : battleFighterDefenders"
        :loading="loadingBattleFighters"
        :can-manage="true"
        :can-manage-by-fighter="() => true"
        @kick="() => {}"
      />
    </div>
  </div>
</template>

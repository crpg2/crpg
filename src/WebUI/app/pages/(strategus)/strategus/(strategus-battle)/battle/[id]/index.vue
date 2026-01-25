<script setup lang="ts">
import type { BattleFighter, BattleSide } from '~/models/strategus/battle'

import { useBattleFighters } from '~/composables/strategus/map/use-map-battle'
import { BATTLE_SIDE } from '~/models/strategus/battle'
// import { usePartyState } from '~/composables/strategus/use-party'

const toast = useToast()

// const {} = usePartyState

const {
  battleFighterAttackers,
  battleFighterDefenders,
  loadingBattleFighters,
  isSelfBattleFighterCommander,
  selfBattleFighter,
  removeBattleFigter,
  removingBattleFigter,
} = useBattleFighters()

const checkCanKick = (side: BattleSide, fighter: BattleFighter) => {
  // other side or yourself
  if (side !== selfBattleFighter.value?.side || fighter.id === selfBattleFighter.value?.id) {
    return false
  }

  // Only commander can kick
  return Boolean(isSelfBattleFighterCommander.value)
}

const checkCanLeave = (side: BattleSide, fighter: BattleFighter) => {
  // other side or yourself-commander
  if (side !== selfBattleFighter.value?.side || isSelfBattleFighterCommander.value) {
    return false
  }

  // yourself
  return selfBattleFighter.value.id === fighter.id
}
</script>

<template>
  <div>
    <div class="grid grid-cols-2 gap-8">
      <BattleFightersTable
        v-for="side in [BATTLE_SIDE.Attacker, BATTLE_SIDE.Defender]"
        :key="side"
        :fighters="side === BATTLE_SIDE.Attacker ? battleFighterAttackers : battleFighterDefenders"
        :loading="loadingBattleFighters"
        :can-kick-by-fighter="(fighter) => checkCanKick(side, fighter)"
        :can-leave-by-fighter="(fighter) => checkCanLeave(side, fighter)"
        :show-actions="side === selfBattleFighter?.side"
        @kick="removeBattleFigter"
        @leave="removeBattleFigter"
      />
    </div>
  </div>
</template>

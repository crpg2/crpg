<script setup lang="ts">
import type { BattleFighter, BattleSide } from '~/models/strategus/battle'

import { useBattleFighters } from '~/composables/strategus/map/use-map-battle'
import { useParty } from '~/composables/strategus/use-party'
import { BATTLE_SIDE } from '~/models/strategus/battle'

const toast = useToast()

const { updateParty } = useParty()

// TODO: to parent?
const {
  battleFighterAttackers,
  battleFighterDefenders,
  isSelfBattleFighterCommander,
  selfBattleFighter,
  removeBattleFigter,
  removingBattleFigter,
} = useBattleFighters()

const onLeaveBattle = async (fighterId: number) => {
  await removeBattleFigter(fighterId, 'leave')
  await updateParty()
}

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
        :can-kick-by-fighter="(fighter) => checkCanKick(side, fighter)"
        :can-leave-by-fighter="(fighter) => checkCanLeave(side, fighter)"
        @kick="(fighterId) => removeBattleFigter(fighterId, 'kick')"
        @leave="onLeaveBattle"
      />
    </div>
  </div>
</template>

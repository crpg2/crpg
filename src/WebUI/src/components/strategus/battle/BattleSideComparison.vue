<script setup lang="ts">
import type { Battle } from '~/models/strategus/battle'

import { BattleSide } from '~/models/strategus/battle'

const props = defineProps<{
  battle: Battle
  mySide: BattleSide | null
  canViewMercenaries: boolean
  attackerMercenaryCount: number
  defenderMercenaryCount: number
}>()

const attackerTroopPercentage = computed (() => {
  if (props.battle) {
    return (props.battle.attackerTotalTroops / (props.battle.attackerTotalTroops + props.battle.defenderTotalTroops) * 100).toFixed(2)
  }
  else { return '0' }
})

const defenderTroopPercentage = computed (() => {
  if (props.battle) {
    return (props.battle.defenderTotalTroops / (props.battle.attackerTotalTroops + props.battle.defenderTotalTroops) * 100).toFixed(2)
  }
  else { return '0' }
})
</script>

<template>
  <div class="grid grid-cols-2">
    <div class="flex flex-row text-base text-white">
      <ClanTagIcon
        v-if="battle.attacker.party?.clan"
        :color="battle.attacker.party?.clan.primaryColor"
        size="4x"
      />

      <div class="flex grow flex-col">
        <span v-if="battle.attacker.party?.clan">{{ battle.attacker.party?.clan.name }} </span>
        <span v-else>{{ battle.attacker.party?.user.name }} </span>
        <div class="text-2xs">
          <OIcon v-tooltip.bottom="$t('strategus.battle.tooltip.troops')" icon="child" size="sm" />{{ battle.attackerTotalTroops }} <span class="text-base-500">({{ attackerTroopPercentage }} %)</span>
        </div>
      </div>
    </div>

    <div class="flex flex-row text-right text-base text-white">
      <div class="flex grow flex-col">
        <span v-if="battle.defender?.settlement?.owner?.clanMembership?.clan">{{ battle.defender.settlement.owner.clanMembership.clan.name }}</span>
        <span v-else-if="battle.defender?.settlement">{{ battle.defender?.settlement.name }}</span>
        <span v-else>{{ battle.defender?.party?.user.name }}</span>
        <div class="text-2xs">
          <span class="text-base-500">({{ defenderTroopPercentage }} %) </span>{{ battle.defenderTotalTroops }}<OIcon v-tooltip.bottom="$t('strategus.battle.tooltip.troops')" icon="child" size="sm" />
        </div>
      </div>

      <ClanTagIcon
        v-if="battle.attacker.party?.clan"
        :color="battle.attacker.party?.clan.primaryColor"
        size="4x"
      />
    </div>
  </div>

  <div class="my-4 h-2.5 w-full rounded-full bg-base-400">
    <div class="h-2.5 rounded-full bg-base-500" :style="{ width: attackerTroopPercentage.concat('%') }" />
  </div>

  <div class="grid grid-cols-2">
    <div class="inline-flex flex-row gap-1.5 text-base text-white">
      <OIcon
        v-tooltip.bottom="$t('strategus.battle.tooltip.mercenaries')"
        icon="game-mode-captain"
        class="text-content-100"
      />
      {{ (mySide === BattleSide.Attacker || canViewMercenaries) ? attackerMercenaryCount : '?' }}
    </div>

    <div class="inline-flex flex-row-reverse gap-1.5 text-base text-white">
      <OIcon
        v-tooltip.bottom="$t('strategus.battle.tooltip.mercenaries')"
        icon="game-mode-captain"
        class="text-content-100"
      />
      {{ (mySide === BattleSide.Defender || canViewMercenaries) ? defenderMercenaryCount : '?' }}
    </div>
  </div>
</template>

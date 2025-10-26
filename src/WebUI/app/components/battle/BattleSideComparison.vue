<script setup lang="ts">
import type { Battle, BattleSide } from '~/models/strategus/battle'

import { BATTLE_SIDE } from '~/models/strategus/battle'
import { argbIntToRgbHexColor } from '~/utils/color'

const { battle } = defineProps<{
  battle: Battle
}>()

const attackerPercentage = computed(() => {
  const total = battle.attacker.totalTroops + battle.defender.totalTroops
  return total > 0 ? (battle.attacker.totalTroops / total) * 100 : 50
})

const colorBySide = (side: BattleSide) => {
  if (side === BATTLE_SIDE.Attacker) {
    const clanPrimaryColor = battle.attacker.commander.party?.user.clanMembership?.clan.primaryColor
    return clanPrimaryColor ? argbIntToRgbHexColor(clanPrimaryColor) : 'var(--ui-color-primary-400)'
  }

  const clanPrimaryColor = battle.defender.commander.party?.user.clanMembership?.clan.primaryColor
  return clanPrimaryColor ? argbIntToRgbHexColor(clanPrimaryColor) : 'var(--ui-color-neutral-400)'
}
</script>

<template>
  <div class="flex gap-4">
    <UTooltip text="Tickets count">
      <UiDataMedia icon="crpg:member" :label="String(battle.attacker.totalTroops)" />
    </UTooltip>
    <!-- TODO: realtime data -->
    <USlider
      :min="0"
      :max="100"
      :ui="{
        thumb: 'hidden',
        range: 'bg-(--attacker-color) rounded-none border-r-2 border-default',
        track: 'h-5 bg-(--defender-color)',
        root: 'opacity-100',
      }"
      :style="{
        '--attacker-color': colorBySide(BATTLE_SIDE.Attacker),
        '--defender-color': colorBySide(BATTLE_SIDE.Defender),
      }"
      disabled
      :model-value="attackerPercentage"
    />
    <!-- :tooltip="{
        disableClosingTrigger: true,
        open: true,
        arrow: true,
        ui: {
          content: 'text-highlighted font-bold',
        },
        portal: false,
        text: 'ddd',
        content: {
          side: 'top',
        },
      }" -->
    <UTooltip text="Tickets count">
      <UiDataMedia icon="crpg:member" :label="String(battle.defender.totalTroops)" />
    </UTooltip>
  </div>
</template>

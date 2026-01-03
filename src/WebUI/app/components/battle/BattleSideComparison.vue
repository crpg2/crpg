<script setup lang="ts">
import type { Battle, BattleSide } from '~/models/strategus/battle'

const props = defineProps<{
  battle: Battle
  mySide: BattleSide | null // TODO:
  canViewMercenaries: boolean // TODO:
  attackerMercenaryCount: number
  defenderMercenaryCount: number
}>()

const attackerPercentage = computed(() => {
  const total = props.battle.attacker.totalTroops + props.battle.defender.totalTroops
  return total > 0 ? (props.battle.attacker.totalTroops / total) * 100 : 50
})
</script>

<template>
  <div class="space-y-10">
    <USlider
      :min="0"
      :max="100"
      :ui="{
        thumb: 'hidden',
        track: 'h-3',
        range: '',
        root: 'opacity-100',
      }"
      disabled
      va
      :model-value="attackerPercentage"
      :tooltip="{
        disableClosingTrigger: true,
        open: true,
        arrow: true,
        ui: {
          content: 'text-highlighted font-bold',
        },
        portal: false,
        text: 'ddddddddddddd',
        content: {
          side: 'top',
        },
      }"
    />
  </div>

  <!-- <div class="grid grid-cols-2">
    <div class="inline-flex flex-row gap-1.5 text-base text-white">
      {{ (mySide === BATTLE_SIDE.Attacker || canViewMercenaries) ? attackerMercenaryCount : '?' }}
    </div>

    <div class="inline-flex flex-row-reverse gap-1.5 text-base text-white">
      {{ (mySide === BATTLE_SIDE.Defender || canViewMercenaries) ? defenderMercenaryCount : '?' }}
    </div>
  </div> -->
</template>

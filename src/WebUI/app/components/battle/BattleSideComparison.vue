<script setup lang="ts">
import type { Battle, BattleSide } from '~/models/strategus/battle'

import { BATTLE_SIDE } from '~/models/strategus/battle'
import { battleIconByType } from '~/services/strategus/battle-service'

const props = defineProps<{
  battle: Battle
  mySide: BattleSide | null // TODO:
  canViewMercenaries: boolean // TODO:
  attackerMercenaryCount: number
  defenderMercenaryCount: number
}>()

const attackerPercentage = computed(() => {
  const total = props.battle.attackerTotalTroops + props.battle.defenderTotalTroops
  return total > 0 ? (props.battle.attackerTotalTroops / total) * 100 : 50
})
</script>

<template>
  <div class="space-y-10">
    <div class="flex gap-20">
      <BattleSideView
        :side="BATTLE_SIDE.Attacker"
        :fighter="battle.attacker"
        :total-troops="battle.attackerTotalTroops"
      />

      <UTooltip :text="battle.type" :content="{ side: 'top' }">
        <USeparator
          orientation="vertical"
          class="h-28 self-center"
          size="sm"
          :icon="`crpg:${battleIconByType[battle.type]}`"
          :ui="{
            icon: 'size-7',
          }"
        />
      </UTooltip>

      <BattleSideView
        :side="BATTLE_SIDE.Defender"
        :fighter="battle.defender!"
        :total-troops="battle.defenderTotalTroops"
      />
    </div>

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

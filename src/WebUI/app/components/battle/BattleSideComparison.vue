<script setup lang="ts">
import type { Battle, BattleSide } from '~/models/strategus/battle'

import { useUser } from '~/composables/user/use-user'
import { BATTLE_SIDE } from '~/models/strategus/battle'
import { battleIconByType } from '~/services/strategus/battle-service'

const props = defineProps<{
  battle: Battle
  mySide: BattleSide | null // TODO:
  canViewMercenaries: boolean // TODO:
  attackerMercenaryCount: number
  defenderMercenaryCount: number
}>()

const { user } = useUser()

const attackerPercentage = computed(() => {
  const total = props.battle.attacker.totalTroops + props.battle.defender.totalTroops
  return total > 0 ? (props.battle.attacker.totalTroops / total) * 100 : 50
})
</script>

<template>
  <div class="space-y-10">
    <div class="flex gap-12">
      <BattleSideView
        :side="BATTLE_SIDE.Attacker"
        :side-info="battle.attacker"
        :user-id="user!.id"
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
        :side-info="battle.defender"
        :user-id="user!.id"
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

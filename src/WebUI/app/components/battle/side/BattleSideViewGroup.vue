<script setup lang="ts">
import type { BattleFighter, BattleType } from '~/models/strategus/battle'
import type { SettlementPublic } from '~/models/strategus/settlement'

import { BATTLE_SIDE } from '~/models/strategus/battle'
import { battleIconByType } from '~/services/strategus/battle-service'

interface BattleSideInfo {
  commander: BattleFighter
  settlement: SettlementPublic | null
  totalTroops: number
}

const { attacker, defender } = defineProps<{
  battleType: BattleType
  attacker: BattleSideInfo
  defender: BattleSideInfo
}>()

const sides = computed(() => [
  {
    side: BATTLE_SIDE.Attacker,
    sideInfo: attacker,
  },
  {
    side: BATTLE_SIDE.Defender,
    sideInfo: defender,
  },
])
</script>

<template>
  <div class="grid grid-cols-[1fr_auto_1fr] gap-6">
    <template v-for="({ side, sideInfo: { commander, settlement, totalTroops } }, idx) in sides" :key="side">
      <BattleSideView
        class="overflow-hidden"
        :side
        :commander
        :settlement
        :total-troops
      >
        <template #topbar-prepend>
          <slot name="topbar-prepend" v-bind="{ side }" />
        </template>

        <template #append>
          <slot name="append" v-bind="{ side }" />
        </template>
      </BattleSideView>

      <UTooltip
        v-if="idx === 0"
        :text="$t(`strategus.battle.type.${battleType}`)"
        :content="{ side: 'top' }"
      >
        <USeparator
          orientation="vertical"
          class="h-28 self-center"
          size="sm"
          :icon="`crpg:${battleIconByType[battleType]}`"
          :ui="{ icon: 'size-7' }"
        />
      </UTooltip>
    </template>
  </div>
</template>

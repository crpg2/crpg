<script setup lang="ts">
import type { Battle, BattleSide } from '~/models/strategus/battle'

import { BATTLE_SIDE } from '~/models/strategus/battle'
import { battleIconByType } from '~/services/strategus/battle-service'

defineProps<{
  battle: Battle
  canApply: boolean
}>()

defineEmits<{
  applyToJoin: [BattleSide]
}>()
</script>

<template>
  <div class="flex justify-center gap-6">
    <BattleSideView
      :side="BATTLE_SIDE.Attacker"
      :side-info="battle.attacker"
      :can-apply
      @apply-to-join="$emit('applyToJoin', BATTLE_SIDE.Attacker)"
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
      :can-apply
      @apply-to-join="$emit('applyToJoin', BATTLE_SIDE.Defender)"
    />
  </div>
</template>

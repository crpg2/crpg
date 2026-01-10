<script setup lang="ts">
import type { BattlePhase } from '~/models/strategus/battle'

import { BATTLE_PHASE } from '~/models/strategus/battle'

const { phase } = defineProps<{
  phase: BattlePhase
}>()

const { t } = useI18n()

const label = computed(() => t(`strategus.battle.phase.${phase}`))
</script>

<template>
  <UTooltip :text="$t('strategus.battle.battlePhase')">
    <UBadge
      v-if="phase === BATTLE_PHASE.Hiring"
      variant="soft"
      color="warning"
      size="xl"
      icon="crpg:trumpet"
      :label
    />

    <UBadge
      v-else-if="phase === BATTLE_PHASE.Scheduled"
      variant="soft"
      color="warning"
      size="xl"
      icon="i-lucide-calendar-check"
      :label
    />

    <UBadge
      v-else-if="phase === BATTLE_PHASE.Live"
      variant="soft"
      color="success"
      size="xl"
      :label
    >
      <template #leading>
        <UiPingIcon />
      </template>
    </UBadge>

    <UBadge
      v-else-if="phase === BATTLE_PHASE.End"
      variant="soft"
      color="error"
      size="xl"
      icon="crpg:not-found"
      :label
    />
  </UTooltip>
</template>

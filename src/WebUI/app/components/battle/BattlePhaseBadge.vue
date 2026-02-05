<script setup lang="ts">
import type { BadgeProps } from '@nuxt/ui'

import type { BattlePhase } from '~/models/strategus/battle'

import { BATTLE_PHASE } from '~/models/strategus/battle'

const { phase, size = 'xl' } = defineProps<{
  phase: BattlePhase
  size?: BadgeProps['size']
}>()

const { t } = useI18n()

const label = computed(() => t(`strategus.battle.phase.${phase}`))
</script>

<template>
  <UTooltip :text="$t('strategus.battle.battlePhase')">
    <!-- TODO: FIXME: icon, style -->
    <UBadge
      v-if="phase === BATTLE_PHASE.Preparation"
      :size
      :label
      variant="soft"
      color="neutral"
      icon="crpg:trumpet"
    />

    <UBadge
      v-if="phase === BATTLE_PHASE.Hiring"
      :size
      :label
      variant="soft"
      color="warning"
      icon="crpg:trumpet"
    />

    <UBadge
      v-else-if="phase === BATTLE_PHASE.Scheduled"
      :size
      :label
      variant="soft"
      color="warning"
      icon="i-lucide-calendar-check"
    />

    <UBadge
      v-else-if="phase === BATTLE_PHASE.Live"
      :size
      :label
      variant="soft"
      color="success"
    >
      <template #leading>
        <UiPingIcon />
      </template>
    </UBadge>

    <UBadge
      v-else-if="phase === BATTLE_PHASE.End"
      :size
      :label
      variant="soft"
      color="error"
      icon="crpg:not-found"
    />
  </UTooltip>
</template>

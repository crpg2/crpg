<script setup lang="ts">
import type { BadgeProps } from '@nuxt/ui'

import type { BattleMercenaryApplicationStatus } from '~/models/strategus/battle'

defineProps<{
  applicationStatus: BattleMercenaryApplicationStatus
}>()

const colorByApplicationStatus: Record<BattleMercenaryApplicationStatus, BadgeProps['color']> = {
  Pending: 'info',
  Accepted: 'success',
  Declined: 'error',
}
</script>

<template>
  <UTooltip :text="$t('strategus.battle.mercenaryApplication.statusTitle')">
    <UBadge
      variant="subtle"
      class="cursor-pointer"
      icon="crpg:mercenary"
      :color="colorByApplicationStatus[applicationStatus]"
      :label="$t(`strategus.battle.mercenaryApplication.status.${applicationStatus}`)"
    />
  </UTooltip>
</template>

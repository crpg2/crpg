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
  <UTooltip text="Mercenary application status">
    <UBadge
      variant="subtle"
      class="cursor-pointer"
      icon="crpg:mercenary"
      :color="colorByApplicationStatus[applicationStatus]"
      :label="applicationStatus"
    />
  </UTooltip>
</template>

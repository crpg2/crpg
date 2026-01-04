<script setup lang="ts">
import type { BadgeProps } from '@nuxt/ui'

import type { BattleMercenaryApplicationStatus } from '~/models/strategus/battle'

defineProps<{
  applicationStatus: BattleMercenaryApplicationStatus | null
}>()

const colorByApplicationStatus: Record<BattleMercenaryApplicationStatus, BadgeProps['color']> = {
  Pending: 'info',
  Accepted: 'success',
  Declined: 'error',
}

const iconByApplicationStatus: Record<BattleMercenaryApplicationStatus, string> = {
  Pending: 'i-lucide-clock',
  Accepted: 'i-lucide-check',
  Declined: 'i-lucide-x',
}
</script>

<template>
  <UButton
    v-if="!applicationStatus"
    label="Apply"
    class="cursor-pointer"
    size="sm"
  />

  <UButton
    v-else
    variant="subtle"
    size="sm"
    class="cursor-pointer"
    :icon="iconByApplicationStatus[applicationStatus]"
    :color="colorByApplicationStatus[applicationStatus]"
    :label="applicationStatus"
  />
</template>

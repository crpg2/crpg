<script setup lang="ts">
import type { BadgeProps } from '@nuxt/ui'

import type { BattleMercenaryApplicationStatus } from '~/models/strategus/battle'

defineProps<{
  applicationStatus: BattleMercenaryApplicationStatus | null
}>()

defineEmits<{
  applyToJoin: []
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
  <UButton v-if="!applicationStatus" label="Apply" size="xs" @click="$emit('applyToJoin')" />

  <UBadge
    v-else
    variant="subtle"
    :icon="iconByApplicationStatus[applicationStatus]"
    :color="colorByApplicationStatus[applicationStatus]"
    :label="applicationStatus"
    trailing-icon="i-lucide-x"
    @click="() => {}"
  />
</template>

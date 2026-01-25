<script setup lang="ts">
import type { BattleFighterApplicationStatus } from '~/models/strategus/battle'

defineProps<{
  status: BattleFighterApplicationStatus
}>()
</script>

<template>
  <UTooltip text="Battle fighter application status">
    <AppApplicationStatusBadge :status />
  </UTooltip>
</template>

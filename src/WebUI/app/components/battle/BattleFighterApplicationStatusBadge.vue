<script setup lang="ts">
import type { BattleFighterApplicationStatus } from '~/models/strategus/battle'

import { BATTLE_FIGHTER_APPLICATION_STATUS } from '~/models/strategus/battle'

defineProps<{
  status: BattleFighterApplicationStatus
}>()

defineEmits<{
  delete: []
}>()
</script>

<template>
  <UTooltip text="Battle fighter application status">
    <span v-if="status === BATTLE_FIGHTER_APPLICATION_STATUS.Pending">
      <AppConfirmActionPopover
        @confirm="$emit('delete')"
      >
        <AppApplicationStatusBadge :status trailing-icon="crpg:close" />
      </AppConfirmActionPopover>
    </span>

    <AppApplicationStatusBadge v-else :status trailing-icon="crpg:close" />
  </UTooltip>
</template>

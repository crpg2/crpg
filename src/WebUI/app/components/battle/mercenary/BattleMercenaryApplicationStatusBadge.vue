<script setup lang="ts">
import type { BattleMercenaryApplicationStatus } from '~/models/strategus/battle'

defineProps<{
  applicationStatus: BattleMercenaryApplicationStatus
}>()
</script>

<template>
  <UTooltip :text="$t('strategus.battle.mercenaryApplication.statusTitle')">
    <AppApplicationStatusBadge :status="applicationStatus" icon="crpg:mercenary" />
  </UTooltip>
</template>

<script setup lang="ts">
import type { BattleMercenaryApplicationStatus } from '~/models/campaign/battle'

defineProps<{
  status: BattleMercenaryApplicationStatus
}>()
</script>

<template>
  <UTooltip :text="$t('campaign.battle.mercenaryApplication.statusTitle')">
    <AppApplicationStatusBadge :status icon="crpg:mercenary" />
  </UTooltip>
</template>

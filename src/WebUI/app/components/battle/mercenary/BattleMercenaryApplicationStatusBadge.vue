<script setup lang="ts">
import type { BattleMercenaryApplicationStatus } from '~/models/strategus/battle'

defineProps<{
  status: BattleMercenaryApplicationStatus
}>()
</script>

<template>
  <UTooltip :text="$t('strategus.battle.mercenaryApplication.statusTitle')">
    <AppApplicationStatusBadge :status icon="crpg:mercenary" />
  </UTooltip>
</template>

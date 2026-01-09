<script setup lang="ts">
import type { DataMediaSize } from '~/components/ui/data/DataMedia.vue'
import type { SettlementPublic } from '~/models/strategus/settlement'

import { settlementIconByType } from '~/services/strategus/settlement-service'

const { settlement, size = 'md' } = defineProps<{
  settlement: SettlementPublic
  size?: DataMediaSize

}>()
</script>

<template>
  <UiDataMedia
    :label="settlement.name"
    :icon="`crpg:${settlementIconByType[settlement.type]}`"
    :size
  />
</template>

<script setup lang="ts">
import type { SettlementPublic } from '~/models/strategus/settlement'

import { itemCultureToIcon } from '~/services/item-service' // TODO: cultrue service
import { settlementIconByType } from '~/services/strategus-service/settlement'

const { settlement } = defineProps<{ settlement: SettlementPublic }>()

const settlementIcon = computed(() => settlementIconByType[settlement.type])
const settlementCultureIcon = computed(() => itemCultureToIcon[settlement.culture])
</script>

<template>
  <div class="flex items-center gap-2">
    <OIcon :icon="settlementIcon.icon" size="4x" />

    <div class="flex flex-col">
      <div class="flex items-center gap-1">
        <div class="text-3xs text-content-300">
          {{ $t(`strategus.settlementType.${settlement.type}`) }}
        </div>

        <SvgSpriteImg :name="settlementCultureIcon" viewBox="0 0 48 48" class="w-4" />
      </div>

      <h1 class="text-lg">
        {{ settlement.name }}
      </h1>
    </div>
  </div>
</template>

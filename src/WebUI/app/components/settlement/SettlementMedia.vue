<script setup lang="ts">
import type { SettlementPublic } from '~/models/strategus/settlement'

import { cultureToIcon } from '~/services/culture-service'
import { settlementIconByType } from '~/services/strategus/settlement-service'

const { settlement } = defineProps<{
  settlement: SettlementPublic
}>()

// TODO: SIZE!!!
</script>

<template>
  <UiDataCell>
    <template #leftContent>
      <UIcon
        class="size-12"
        :name="`crpg:${settlementIconByType[settlement.type]}`"
      />
    </template>
    <UiDataContent layout="reverse">
      <template #caption>
        <UiDataMedia
          size="sm"
          :icon="`crpg:${cultureToIcon[settlement.culture]}`"
          :label="$t(`strategus.settlementType.${settlement.type}`)"
        />
      </template>
      <UiTextView variant="h3">
        {{ settlement.name }}
      </UiTextView>
    </UiDataContent>
  </UiDataCell>
</template>

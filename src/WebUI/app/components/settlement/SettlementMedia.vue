<script setup lang="ts">
import type { DataMediaSize } from '~/components/ui/data/DataMedia.vue'
import type { TextViewVariant } from '~/components/ui/text/TextView.vue'
import type { SettlementPublic } from '~/models/strategus/settlement'

import { cultureToIcon } from '~/services/culture-service'
import { settlementIconByType } from '~/services/strategus/settlement-service'

const { settlement, size = 'md' } = defineProps<{
  settlement: SettlementPublic
  size?: DataMediaSize
}>()

const iconSize = computed(() => ({
  xs: 'size-6',
  sm: 'size-8',
  md: 'size-9',
  lg: 'size-10',
  xl: 'size-12',
} satisfies Record<DataMediaSize, string>)[size])

const mediaSize = computed(() => ({
  xs: 'xs',
  sm: 'xs',
  md: 'xs',
  lg: 'md',
  xl: 'md',
} satisfies Record<DataMediaSize, DataMediaSize>)[size])

const nameVariant = computed(() => ({
  xs: 'p-xs',
  sm: 'p-xs',
  md: 'p-sm',
  lg: 'h4',
  xl: 'h3',
} satisfies Record<DataMediaSize, TextViewVariant>)[size])
</script>

<template>
  <UiDataCell>
    <template #leftContent>
      <UIcon
        :class="iconSize"
        :name="`crpg:${settlementIconByType[settlement.type]}`"
      />
    </template>
    <UiDataContent layout="reverse">
      <template #caption>
        <UiDataMedia
          :size="mediaSize"
          :icon="`crpg:${cultureToIcon[settlement.culture]}`"
          :label="$t(`strategus.settlementType.${settlement.type}`)"
        />
      </template>
      <UiTextView :variant="nameVariant">
        {{ settlement.name }}
      </UiTextView>
    </UiDataContent>
  </UiDataCell>
</template>

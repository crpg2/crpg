<script setup lang="ts">
import { useClipboard } from '@vueuse/core'

import type { CompareItemsResult, Item } from '~/models/item'

import { useItem } from '~/composables/item/use-item'
import { ITEM_COMPARE_MODE } from '~/models/item'
import { createItemIndex } from '~/services/item-search-service/indexator'
import { getItemAggregations } from '~/services/item-service'

const { compareResult, item } = defineProps<{
  item: Item
  compareResult?: CompareItemsResult // TODO: hmm // from provide/inject
}>()

const toast = useToast()
const { t } = useI18n()
const { copy } = useClipboard()

const { rankColor, thumb } = useItem(() => item)

const onNameCopy = () => {
  copy(item.name)
  toast.add({
    title: t('action.copied'),
    close: false,
    color: 'success',
  })
}

const flatItem = computed(() => createItemIndex([item])[0]!)
const aggregationConfig = computed(() => getItemAggregations(flatItem.value))
</script>

<template>
  <UCard
    variant="soft"
    :ui="{
      root: 'w-80',
      header: 'relative !p-0 h-40',
      body: '!p-3',
      footer: '!p-3',
    }"
  >
    <template #header>
      <ItemThumb
        :thumb
        :name="item.name"
        class="pointer-events-none h-full"
      />

      <div class="absolute top-4 left-4 z-10 flex items-center gap-1">
        <ItemRankIcon v-if="item.rank > 0" :rank="item.rank" />
        <slot name="badges-top-left" />
      </div>

      <div class="absolute top-4 right-48 z-10 flex items-center gap-1">
        <slot name="badges-top-right" />
      </div>

      <div class="absolute bottom-4 left-4 z-10 flex items-center gap-1">
        <slot name="badges-bottom-left" />
      </div>

      <div class="absolute right-4 bottom-4 z-10 flex items-center gap-1">
        <slot name="badges-bottom-right" />
      </div>
    </template>

    <template #default>
      <UiTextView variant="h4" class="mb-4" :style="{ color: rankColor }">
        {{ item.name }}

        <UTooltip :text="$t('action.copy')">
          <UBadge
            class="cursor-pointer"
            icon="crpg:copy"
            variant="subtle"
            size="sm"
            square
            @click="onNameCopy"
          />
        </UTooltip>
      </UiTextView>

      <div class="grid grid-cols-2 gap-2.5">
        <ItemParam
          v-for="(_agg, field) in aggregationConfig"
          :key="field"
          :item="flatItem"
          :field
          show-label
          :is-compare="compareResult !== undefined"
          :compare-mode="ITEM_COMPARE_MODE.Absolute"
          :best-value="compareResult !== undefined ? compareResult[field]! : undefined"
        >
          <template
            v-if="field === 'price'"
            #default="{ rawBuckets }"
          >
            <AppCoin size="lg" :value="(rawBuckets as number)" />
          </template>

          <template
            v-else-if="field === 'upkeep'"
            #default="{ rawBuckets }"
          >
            <AppCoin size="lg">
              {{ $t('item.format.upkeep', { upkeep: $n((rawBuckets as number)) }) }}
            </AppCoin>
          </template>
        </ItemParam>
      </div>
    </template>

    <template #footer>
      <slot name="actions" />
    </template>
  </UCard>
</template>

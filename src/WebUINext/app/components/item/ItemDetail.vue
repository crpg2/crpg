<script setup lang="ts">
import { useClipboard } from '@vueuse/core'

import type { CompareItemsResult, Item } from '~/models/item'

import { useItem } from '~/composables/item/use-item'
import { ITEM_COMPARE_MODE } from '~/models/item'
import { createItemIndex } from '~/services/item-search-service/indexator'
import { getItemAggregations } from '~/services/item-service'

const { compareResult, item } = defineProps<{
  item: Item
  compareResult?: CompareItemsResult
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

const flatItem = computed(() => createItemIndex([item]).at(0)!) // TODO:
const aggregationConfig = computed(() => getItemAggregations(flatItem.value))
</script>

<template>
  <UCard
    variant="subtle"
    :ui="{
      root: 'w-80 overflow-visible',
      header: 'relative !p-0 h-40 border-0',
      body: '!p-3',
      footer: '!p-3',
    }"
  >
    <template #header>
      <div class="absolute top-8 right-0 translate-x-1/2">
        <slot name="actions" />
      </div>

      <ItemThumb
        :thumb
        :name="item.name"
        class="pointer-events-none"
      />

      <div class="absolute top-3 left-3 z-10 flex items-center gap-1">
        <ItemRankIcon v-if="item.rank > 0" :rank="item.rank" />
        <slot name="badges-top-left" />
      </div>

      <div class="absolute top-3 right-3 z-10 flex items-center gap-1">
        <slot name="badges-top-right" />
      </div>

      <div class="absolute bottom-3 left-3 z-10 flex items-center gap-1">
        <slot name="badges-bottom-left" />
      </div>

      <div class="absolute right-3 bottom-3 z-10 flex items-center gap-1">
        <slot name="badges-bottom-right" />
      </div>
    </template>

    <template #default>
      <div class="mb-4 flex gap-2">
        <UiTextView
          variant="h5"
          :style="{ color: rankColor }"
        >
          {{ item.name }}
        </UiTextView>

        <div>
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
        </div>
      </div>

      <div class="grid grid-cols-2 gap-2.5">
        <!-- TODO: to ItemParamGrid -->
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
            <AppCoin :value="(rawBuckets as number)" />
          </template>

          <template
            v-else-if="field === 'upkeep'"
            #default="{ rawBuckets }"
          >
            <AppCoin>
              <template #default="{ classes }">
                <span :class="classes()">{{ $t('item.format.upkeep', { upkeep: $n((rawBuckets as number)) }) }}</span>
              </template>
            </AppCoin>
          </template>
        </ItemParam>
      </div>
    </template>
  </UCard>
</template>

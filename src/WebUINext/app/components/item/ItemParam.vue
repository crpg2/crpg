<script setup lang="ts">
import type { ItemCompareMode, ItemFieldFormat, ItemFlat } from '~/models/item'

import {
  ITEM_COMPARE_MODE,
  ITEM_FIELD_COMPARE_RULE,
  ITEM_FIELD_FORMAT,
} from '~/models/item'
import { aggregationsConfig } from '~/services/item-search-service/aggregations'
import {
  getItemFieldAbsoluteDiffStr,
  getItemFieldRelativeDiffStr,
  humanizeBucket,
} from '~/services/item-service'

const {
  item,
  field,
  showLabel = false,
  isCompare = false,
  compareMode = ITEM_COMPARE_MODE.Absolute,
  bestValue,
  relativeValue,
} = defineProps<{
  item: ItemFlat
  field: keyof ItemFlat
  showLabel?: boolean
  isCompare?: boolean
  compareMode?: ItemCompareMode
  bestValue?: number
  relativeValue?: number
}>()

const rawBuckets = item[field]
const compareRule = aggregationsConfig[field]?.compareRule || ITEM_FIELD_COMPARE_RULE.Bigger

const isBest = computed(() => (bestValue !== undefined ? rawBuckets === bestValue : false))

const diffStr = computed(() => {
  if (typeof rawBuckets !== 'number' || !isCompare) {
    return null
  }

  if (compareMode === ITEM_COMPARE_MODE.Absolute) {
    if (!isBest.value && bestValue !== undefined) {
      return getItemFieldAbsoluteDiffStr(compareRule, rawBuckets, bestValue)
    }

    return null
  }

  if (relativeValue !== undefined) {
    return getItemFieldRelativeDiffStr(rawBuckets, relativeValue)
  }

  return null
})

const formattedBuckets = computed(() => {
  if (Array.isArray(rawBuckets)) {
    return rawBuckets.map(bucket => humanizeBucket(field, bucket, item))
  }
  return [humanizeBucket(field, rawBuckets, item)]
})

// TODO: to tailwind cfg
const colorPositive = '#34d399'
const colorNegative = '#ef4444'

// TODO: spec, refactor: more readable, to service?
const fieldStyle = computed(() => {
  if (!isCompare || typeof rawBuckets !== 'number') {
    return ''
  }

  if (compareMode === ITEM_COMPARE_MODE.Absolute) {
    if (isBest.value) {
      return `color: ${colorPositive}`
    }
    else { return `color: ${colorNegative}` }
  }

  if (compareMode === ITEM_COMPARE_MODE.Relative) {
    if (relativeValue === undefined || rawBuckets === relativeValue) {
      return ''
    }

    if (compareRule === ITEM_FIELD_COMPARE_RULE.Less) {
      if (relativeValue > rawBuckets) {
        return `color: ${colorPositive}`
      }
    }
    else {
      if (rawBuckets > relativeValue) {
        return `color: ${colorPositive}`
      }
    }

    return `color: ${colorNegative}`
  }

  return ''
})
</script>

<template>
  <div class="space-y-1">
    <UTooltip
      v-if="showLabel" :ui="{
        content: 'max-w-sm',
      }"
    >
      <div class="text-xs text-muted">
        {{ $t(`item.aggregations.${field}.title`) }}
      </div>

      <template #content>
        <div class="space-y-5">
          <h5 class="text-md font-bold text-highlighted">
            {{ $t(`item.aggregations.${field}.title`) }}
          </h5>
          <div v-if="$t(`item.aggregations.${field}.description`)" class="prose">
            <p>
              {{ $t(`item.aggregations.${field}.description`) }}
            </p>
          </div>
        </div>
      </template>
    </UTooltip>

    <div
      :style="fieldStyle"
      class="flex flex-wrap items-center gap-1"
    >
      <template
        v-for="(formattedValue, idx) in formattedBuckets"
        :key="idx"
      >
        <slot v-bind="{ rawBuckets, formattedValue, diffStr }">
          <UTooltip
            :ui="{
              content: 'max-w-sm',
            }"
            :disabled="!formattedValue.tooltip"
          >
            <UIcon
              v-if="formattedValue.icon !== null"
              :name="`crpg:${formattedValue.icon}`"
              class="size-8"
            />

            <UBadge
              v-else-if="
                aggregationsConfig[field]?.format
                  && ([ITEM_FIELD_FORMAT.List, ITEM_FIELD_FORMAT.Damage] as ItemFieldFormat[]).includes(aggregationsConfig[field].format)
                  && !['', '0'].includes(formattedValue.label)
              "
              :label="formattedValue.label"
              color="neutral"
              variant="subtle"
            />

            <div
              v-else
              class="font-bold"
            >
              {{ formattedValue.label }}
            </div>

            <template #content>
              <div class="space-y-5">
                <h5 class="text-md font-bold text-highlighted">
                  {{ formattedValue.tooltip?.title }}
                </h5>
                <div
                  v-if="formattedValue.tooltip?.description"
                  class="prose prose-sm"
                  v-html="formattedValue.tooltip?.description"
                />
              </div>
            </template>
          </UTooltip>
        </slot>

        <div
          v-if="diffStr !== null"
          class="text-xs font-bold"
        >
          {{ diffStr }}
        </div>
      </template>
    </div>
  </div>
</template>

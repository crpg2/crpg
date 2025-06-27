<script setup lang="ts">
import type { ItemFlat } from '~/models/item'

import {
  ItemCompareMode,
  ItemFieldCompareRule,
  ItemFieldFormat,
} from '~/models/item'
import { aggregationsConfig } from '~/services/item-search-service/aggregations'
import {
  getItemFieldAbsoluteDiffStr,
  getItemFieldRelativeDiffStr,

  humanizeBucket,
} from '~/services/item-service'

const {
  bestValue,
  compareMode = ItemCompareMode.Absolute,
  field,
  isCompare = false,
  item,
  relativeValue,
} = defineProps<{
  item: ItemFlat
  field: keyof ItemFlat
  isCompare?: boolean
  compareMode?: ItemCompareMode
  bestValue?: number
  relativeValue?: number
}>()

const rawBuckets = item[field]
const compareRule = aggregationsConfig[field]?.compareRule || ItemFieldCompareRule.Bigger

const isBest = computed(() => (bestValue !== undefined ? rawBuckets === bestValue : false))

const diffStr = computed(() => {
  if (typeof rawBuckets !== 'number' || !isCompare) {
    return null
  }

  if (compareMode === ItemCompareMode.Absolute) {
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

  if (compareMode === ItemCompareMode.Absolute) {
    if (isBest.value) {
      return `color: ${colorPositive}`
    }
    else { return `color: ${colorNegative}` }
  }

  if (compareMode === ItemCompareMode.Relative) {
    if (relativeValue === undefined || rawBuckets === relativeValue) {
      return ''
    }

    if (compareRule === ItemFieldCompareRule.Less) {
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
  <!-- TODO: badge for array without icon, custom style for price -->
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
            content: 'max-w-lg',
          }"
          :disabled="!formattedValue.tooltip"
        >
          <ItemFieldIcon
            v-if="formattedValue.icon !== null"
            :icon="formattedValue.icon"
            size="2xl"
          />

          <UBadge
            v-else-if="
              aggregationsConfig[field]?.format
                && [ItemFieldFormat.List, ItemFieldFormat.Damage].includes(aggregationsConfig[field].format)
                && !['', '0'].includes(formattedValue.label)
            "
            :label="formattedValue.label"
            color="neutral"
            variant="subtle"
          />

          <div
            v-else
            class="text-xs font-bold"
          >
            {{ formattedValue.label }}
          </div>

          <template #content>
            <div class="prose prose-invert">
              <h4>{{ formattedValue.tooltip?.title }}</h4>
              <div v-html="formattedValue.tooltip?.description" />
            </div>
          </template>
        </UTooltip>
      </slot>

      <div
        v-if="diffStr !== null"
        class="text-2xs font-bold"
      >
        {{ diffStr }}
      </div>
    </template>
  </div>
</template>

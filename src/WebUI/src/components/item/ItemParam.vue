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

const rawBuckets = computed(() => item[field])
const compareRule = computed(
  () => aggregationsConfig[field]?.compareRule || ItemFieldCompareRule.Bigger,
)
const isBest = computed(() => (bestValue !== undefined ? rawBuckets.value === bestValue : false))

const diffStr = computed(() => {
  if (typeof rawBuckets.value !== 'number' || !isCompare) {
    return null
  }

  if (compareMode === ItemCompareMode.Absolute) {
    if (!isBest.value && bestValue !== undefined) {
      return getItemFieldAbsoluteDiffStr(compareRule.value, rawBuckets.value, bestValue)
    }

    return null
  }

  if (relativeValue !== undefined) {
    return getItemFieldRelativeDiffStr(rawBuckets.value, relativeValue)
  }

  return null
})

const formattedBuckets = computed(() => {
  if (Array.isArray(rawBuckets.value)) {
    return rawBuckets.value.map(bucket => humanizeBucket(field, bucket, item))
  }

  return [humanizeBucket(field, rawBuckets.value, item)]
})

// TODO: to tailwind cfg
const colorPositive = '#34d399'
const colorNegative = '#ef4444'

// TODO: spec, refactor: more readable
const fieldStyle = computed(() => {
  if (!isCompare || typeof rawBuckets.value !== 'number') { return '' }

  if (compareMode === ItemCompareMode.Absolute) {
    if (isBest.value) { return `color: ${colorPositive}` }
    else { return `color: ${colorNegative}` }
  }

  if (compareMode === ItemCompareMode.Relative) {
    if (relativeValue === undefined || rawBuckets.value === relativeValue) { return '' }

    if (compareRule.value === ItemFieldCompareRule.Less) {
      if (relativeValue > rawBuckets.value) { return `color: ${colorPositive}` }
    }
    else {
      if (rawBuckets.value > relativeValue) { return `color: ${colorPositive}` }
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
        <Tooltip
          v-bind="{
            placement: 'top',
            title: formattedValue.tooltip?.title,
            description: formattedValue.tooltip?.description,
          }"
        >
          <ItemFieldIcon
            v-if="formattedValue.icon !== null"
            :icon="formattedValue.icon"
            size="2xl"
          />

          <Tag
            v-else-if="
              [ItemFieldFormat.List, ItemFieldFormat.Damage].includes(
                aggregationsConfig[field]!.format!,
              ) && formattedValue.label !== ''
            "
            :label="formattedValue.label"
            size="lg"
          />

          <div
            v-else
            class="text-xs font-bold"
          >
            {{ formattedValue.label }}
          </div>
        </Tooltip>
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

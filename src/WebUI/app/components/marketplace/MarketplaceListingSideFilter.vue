<script setup lang="ts">
import type { SelectMenuItem } from '@nuxt/ui'

import type { ItemType } from '~/models/item'
import type { MartetplaceListingsSideFilter } from '~/models/marketplace'

import { ITEM_TYPE } from '~/models/item'
import { getRankColor, itemTypeToIcon } from '~/services/item-service'
import { getDefaultMartetplaceSideFilterState } from '~/services/marketplace-service'

defineProps<{
  label: string
}>()

const CURRENCY_FILTER_MODE = {
  Any: 'Any',
  None: 'None',
  CustomRange: 'CustomRange',
} as const

const { t } = useI18n()

const modelValue = defineModel<MartetplaceListingsSideFilter>({ default: getDefaultMartetplaceSideFilterState })

const DEFAULT_GOLD_RANGE: [number, number] = [0, 10_000_000]
const DEFAULT_HEIRLOOM_POINTS_RANGE: [number, number] = [0, 10]

const isRangeFilter = (value: MartetplaceListingsSideFilter['gold']): value is [number, number] =>
  Array.isArray(value)

const goldMode = computed({
  get: () => isRangeFilter(modelValue.value.gold)
    ? CURRENCY_FILTER_MODE.CustomRange
    : modelValue.value.gold,
  set: (mode: typeof CURRENCY_FILTER_MODE[keyof typeof CURRENCY_FILTER_MODE]) => {
    if (mode === CURRENCY_FILTER_MODE.CustomRange) {
      modelValue.value.gold = isRangeFilter(modelValue.value.gold)
        ? modelValue.value.gold
        : [...DEFAULT_GOLD_RANGE]
      return
    }

    modelValue.value.gold = mode
  },
})

const goldRange = computed<[number, number]>({
  get: () => isRangeFilter(modelValue.value.gold)
    ? modelValue.value.gold
    : DEFAULT_GOLD_RANGE,
  set: (range) => {
    modelValue.value.gold = range
  },
})

const heirloomPointsMode = computed({
  get: () => isRangeFilter(modelValue.value.heirloomPoints)
    ? CURRENCY_FILTER_MODE.CustomRange
    : modelValue.value.heirloomPoints,
  set: (mode: typeof CURRENCY_FILTER_MODE[keyof typeof CURRENCY_FILTER_MODE]) => {
    if (mode === CURRENCY_FILTER_MODE.CustomRange) {
      modelValue.value.heirloomPoints = isRangeFilter(modelValue.value.heirloomPoints)
        ? modelValue.value.heirloomPoints
        : [...DEFAULT_HEIRLOOM_POINTS_RANGE]
      return
    }

    modelValue.value.heirloomPoints = mode
  },
})

const heirloomPointsRange = computed<[number, number]>({
  get: () => isRangeFilter(modelValue.value.heirloomPoints)
    ? modelValue.value.heirloomPoints
    : DEFAULT_HEIRLOOM_POINTS_RANGE,
  set: (range) => {
    modelValue.value.heirloomPoints = range
  },
})

// TODO: to service
const TRADEABLE_ITEM_TYPES = Object.values(ITEM_TYPE)
  .filter(t => !([
    ITEM_TYPE.Undefined,
    ITEM_TYPE.Ranged, // virtual
    ITEM_TYPE.Ammo, // virtual
    ITEM_TYPE.Banner,
    ITEM_TYPE.Pistol,
  ] as ItemType[]).includes(t))

const itemTypeOptions: SelectMenuItem[] = TRADEABLE_ITEM_TYPES.map(type => ({
  label: t(`item.type.${type}`),
  value: type,
  icon: `crpg:${itemTypeToIcon[type]}`,
}))

const itemRankOptions = ['0', '1', '2', '3'].map(rank => ({
  label: `${rank !== '0' ? '+' : ''}${rank}`,
  value: rank,
  color: getRankColor(Number(rank)),
}))

const currencyFilterModeOptions: SelectMenuItem[] = [
  { label: t('marketplace.filter.currencyMode.Any'), value: CURRENCY_FILTER_MODE.Any },
  { label: t('marketplace.filter.currencyMode.None'), value: CURRENCY_FILTER_MODE.None },
  { label: t('marketplace.filter.currencyMode.CustomRange'), value: CURRENCY_FILTER_MODE.CustomRange },
]
</script>

<template>
  <UFormField
    :label
    :ui="{ container: 'mt-4 space-y-5' }"
  >
    <UCard
      variant="subtle"
      :ui="{
        body: 'space-y-4',
        header: 'flex justify-between gap-4 items-start' }"
    >
      <div class="flex gap-4">
        <USelectMenu
          v-model="modelValue.itemType"
          class="flex-1"
          clear
          size="xl"
          value-key="value"
          variant="outline"
          :model-modifiers="{ nullable: true }"
          :placeholder="$t('marketplace.filter.selectItemType')"
          :icon="modelValue.itemType ? `crpg:${itemTypeToIcon[modelValue.itemType]}` : undefined"
          :items="itemTypeOptions"
          :disabled="!!modelValue.item"
          :ui="{ content: 'min-w-fit' }"
        />
        <UCheckboxGroup
          orientation="horizontal"
          color="neutral"
          variant="table"
          indicator="hidden"
          :items="itemRankOptions"
          :disabled="!!modelValue.item"
          :model-value="modelValue.itemRanks.map(String)"
          @update:model-value="(ranks: string[]) => modelValue.itemRanks = ranks.map(Number)"
        >
          <template #label="{ item }">
            <span class="font-bold" :style="{ color: item.color }">
              {{ item.label }}
            </span>
          </template>
        </UCheckboxGroup>
      </div>

      <USeparator :label="$t('marketplace.filter.orSeparator')" />

      <ItemSelect
        v-model="modelValue.item"
        :label="$t('marketplace.filter.chooseItem')"
        :disabled="!!modelValue.itemType || !!modelValue.itemRanks.length"
      />
    </UCard>

    <UCard
      variant="subtle"
      :ui="{
        body: 'space-y-4',
        header: 'flex justify-between gap-4' }"
    >
      <template #header>
        <UiDataMedia :label="$t('marketplace.currency.gold')">
          <template #icon>
            <AppCoin />
          </template>
        </UiDataMedia>
        <URadioGroup
          v-model="goldMode"
          orientation="horizontal"
          variant="table"
          indicator="hidden"
          size="xs"
          color="neutral"
          :items="currencyFilterModeOptions"
        />
      </template>

      <template v-if="goldMode === CURRENCY_FILTER_MODE.CustomRange" #default>
        <UiInputRange
          v-model="goldRange"
          :min="0"
          :max="1_000_000"
          :step="10000"
        />
      </template>
    </UCard>

    <UCard
      variant="subtle"
      :ui="{
        body: 'space-y-4',
        header: 'flex justify-between gap-4' }"
    >
      <template #header>
        <UiDataMedia :label="$t('marketplace.currency.heirloomPoints')">
          <template #icon>
            <AppLoom />
          </template>
        </UiDataMedia>
        <URadioGroup
          v-model="heirloomPointsMode"
          orientation="horizontal"
          variant="table"
          indicator="hidden"
          size="xs"
          color="neutral"
          :items="currencyFilterModeOptions"
        />
      </template>

      <template v-if="heirloomPointsMode === CURRENCY_FILTER_MODE.CustomRange" #default>
        <UiInputRange
          v-model="heirloomPointsRange"
          :min="0"
          :max="10"
          :step="1"
        />
      </template>
    </UCard>
  </UFormField>
</template>

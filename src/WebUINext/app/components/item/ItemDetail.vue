<script setup lang="ts">
import { useClipboard } from '@vueuse/core'

import type { CompareItemsResult, Item } from '~/models/item'

import { useItem } from '~/composables/item/use-item'
import { ItemCompareMode } from '~/models/item'
import { createItemIndex } from '~/services/item-search-service/indexator'
import { getItemAggregations } from '~/services/item-service'
import { notify } from '~/services/notification-service'
import { t } from '~/services/translate-service'

const { compareResult, item } = defineProps<{
  item: Item
  compareResult?: CompareItemsResult // TODO: hmm
}>()

const { rankColor, thumb } = useItem(() => item)

const { copy } = useClipboard()
const onNameCopy = () => {
  copy(item.name)
  notify(t('action.copied'))
}

const flatItem = computed(() => createItemIndex([item])[0])

const aggregationConfig = computed(() => getItemAggregations(flatItem.value))
</script>

<template>
  <article class="w-80 overflow-hidden p-4">
    <div class="relative mb-3">
      <div class="-mx-4 -mt-4">
        <img
          :src="thumb"
          :alt="item.name"
          :title="item.name"
          class="pointer-events-none w-full select-none object-contain"
        >
      </div>

      <div class="absolute left-0 top-4 z-10 flex items-center gap-1">
        <ItemRankIcon
          v-if="item.rank > 0"
          :rank="item.rank"
        />

        <slot name="badges-top-left" />
      </div>

      <div class="absolute right-0 top-4 z-10 flex items-center gap-1">
        <slot name="badges-top-right" />
      </div>

      <div class="absolute bottom-0 left-0 z-10 flex items-center gap-1">
        <slot name="badges-bottom-left" />
      </div>

      <div class="absolute bottom-0 right-0 z-10 flex items-center gap-1">
        <slot name="badges-bottom-right" />
      </div>
    </div>

    <h3
      class="mb-6 font-bold"
      :style="{ color: rankColor }"
    >
      {{ item.name }}

      <Tag
        v-tooltip.bottom="$t('action.copy')"
        icon="popup"
        variant="primary"
        rounded
        size="sm"
        @click="onNameCopy"
      />
    </h3>

    <div class="grid grid-cols-2 gap-4">
      <div class="space-y-1">
        <h6 class="text-2xs text-content-300">
          Type/Class
        </h6>
        <div class="flex flex-wrap gap-2">
          <ItemParam
            :item="flatItem"
            field="type"
          />
          <ItemParam
            v-if="flatItem.weaponClass !== null"
            :item="flatItem"
            field="weaponClass"
          />
        </div>
      </div>

      <div
        v-for="(_agg, field) in aggregationConfig"
        :key="field"
        class="space-y-1"
      >
        <VTooltip :delay="{ show: 600 }">
          <h6 class="text-2xs text-content-300">
            {{ $t(`item.aggregations.${field}.title`) }}
          </h6>

          <template #popper>
            <div class="prose prose-invert">
              <h5 class="text-content-100">
                {{ $t(`item.aggregations.${field}.title`) }}
              </h5>
              <p v-if="$t(`item.aggregations.${field}.description`)">
                {{ $t(`item.aggregations.${field}.description`) }}
              </p>
            </div>
          </template>
        </VTooltip>

        <ItemParam
          :item="flatItem"
          :field="field"
          :is-compare="compareResult !== undefined"
          :compare-mode="ItemCompareMode.Absolute"
          :best-value="compareResult !== undefined ? compareResult[field] : undefined"
        >
          <template
            v-if="field === 'price'"
            #default="{ rawBuckets }"
          >
            <Coin :value="(rawBuckets as number)" />
          </template>

          <template
            v-else-if="field === 'upkeep'"
            #default="{ rawBuckets }"
          >
            <Coin>
              {{ $t('item.format.upkeep', { upkeep: $n((rawBuckets as number)) }) }}
            </Coin>
          </template>
        </ItemParam>
      </div>
    </div>

    <div class="-mx-4 -mb-4 mt-6 flex flex-wrap items-center gap-2 bg-base-400 p-2">
      <slot name="actions" />
    </div>
  </article>
</template>

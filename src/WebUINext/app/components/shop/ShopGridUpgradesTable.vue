<script setup lang="ts">
import type { ItemFlat } from '~/models/item'
// import type { AggregationConfig } from '~/models/item-search'

import { useItemUpgrades } from '~/composables/item/use-item-upgrades'
import { ItemCompareMode } from '~/models/item'

const {
  // cols,
  item,
} = defineProps<{
  item: ItemFlat
  // cols: AggregationConfig
}>()

const {
  isLoading,
  itemUpgrades,
// relativeEntries
} = useItemUpgrades(item,
// cols
)
</script>

<template>
  <div>
    ddd
  </div>
  <!-- <OTable
    :data="itemUpgrades.slice(1)"
    :show-header="false"
    :loading="isLoading"
  >
    <OTableColumn
      v-slot
      :width="78"
    >
      <template />
    </OTableColumn>

    <OTableColumn
      v-slot="{ row: item }: { row: ItemFlat }"
      field="name"
    >
      <ShopGridItemName
        :item="item"
        show-tier
      />
    </OTableColumn>

    <OTableColumn
      v-for="(field, idx) in (Object.keys(cols) as Array<keyof ItemFlat>)"
      :key="idx"
      v-slot="{ row: rowItem }: { row: ItemFlat }"
      :field="field"
      :width="cols[field]?.width ?? 140"
    >
      <ItemParam
        :item="rowItem"
        :field="field"
        is-compare
        :compare-mode="ItemCompareMode.Relative"
        :relative-value="relativeEntries[field]"
      />
    </OTableColumn>

    <template #empty>
      <div class="relative min-h-40">
        <OLoading
          active
          icon-size="xl"
          :full-page="false"
        />
      </div>
    </template>
  </OTable> -->
</template>

<script setup lang="ts">
import { ItemDetail } from '#components'

import type { GroupedCompareItemsResult } from '~/models/item'
import type { ItemStack } from '~/models/strategus/party'
import type { SortingConfig } from '~/services/item-search-service'

import { useItemDetail } from '~/composables/item/use-item-detail'

const { items } = defineProps<{ items: ItemStack[] }>()

const sortingConfig: SortingConfig = {
  rank_desc: { field: 'rank', order: 'desc' },
  type_asc: { field: 'type', order: 'asc' },
  // TODO: FIXME: by count
}

const sortingModel = ref<string>('rank_desc')

const { toggleItemDetail } = useItemDetail()

const renderItemDetail = <T extends { id: string }>(opendeItem: T, compareItemsResult: GroupedCompareItemsResult[]) => {
  const itemStack = items.find(i => i.item.id === opendeItem.id)

  if (!itemStack) {
    return null
  }

  return h(ItemDetail, {
    item: itemStack.item,
    compareResult: compareItemsResult.find(cr => cr.type === itemStack.item.type)?.compareResult,
  })
}
</script>

<template>
  <ItemGrid
    v-model:sorting="sortingModel"
    :sorting-config="sortingConfig"
    :items
  >
    <template #filter-trailing>
      <slot name="filter-trailing" />
    </template>

    <template #item="itemStack">
      <div>
        <ItemCard
          class="cursor-pointer"
          :item="itemStack.item"
          @click="(e: Event) => toggleItemDetail(e.target as HTMLElement, itemStack.item.id)"
        >
          <template #badges-bottom-right>
            <slot name="badges-bottom-right" v-bind="itemStack">
              <UBadge :label="$n(itemStack.count)" color="neutral" variant="subtle" @click.stop />
            </slot>
          </template>
        </ItemCard>
        <slot name="item-trailing" v-bind="itemStack" />
      </div>
    </template>

    <template #item-detail="{ item, compareItemsResult }">
      <component :is="renderItemDetail(item, compareItemsResult)" />
    </template>
  </ItemGrid>
</template>

<script setup lang="ts">
import type { SortingConfig } from '~/services/item-search-service'

import { useParty } from '~/composables/campaign/use-party'
import { useSettlement } from '~/composables/campaign/use-settlements'
import { buySettlementItem } from '~/services/campaign/party-service'
import { getItems } from '~/services/item-service'

const {
  state: _items,
  isLoading: loadingItems,
} = useAsyncState(() => getItems(), [])

const { settlement } = useSettlement()
const { updateParty } = useParty()

const [buyItem] = useAsyncCallback(async (itemId: string, itemCount: number) => {
  await buySettlementItem(settlement.value.id, itemId, itemCount)
  await updateParty()
})

const items = computed(() => _items.value.map(item => ({ item })))

const sortingConfig: SortingConfig = {
  rank_desc: { field: 'rank', order: 'desc' },
  type_asc: { field: 'type', order: 'asc' },
}

const sortingModel = ref<string>('rank_desc')
</script>

<template>
  <div>
    <ItemGrid
      v-model:sorting="sortingModel"
      :items
      :sorting-config
    >
      <template #item="{ item }">
        <div>
          <ItemCard :item />
          <UButton label="buy 10" @click="buyItem(item.id, 10)" />
        </div>
      </template>
    </ItemGrid>
  </div>
</template>

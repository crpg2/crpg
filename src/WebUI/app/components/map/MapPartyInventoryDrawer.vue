<script setup lang="ts">
import { ItemDetail } from '#components'

import type { GroupedCompareItemsResult } from '~/models/item'
import type { SortingConfig } from '~/services/item-search-service'

import { useItemDetail } from '~/composables/item/use-item-detail'
import { getSelfPartyItems } from '~/services/strategus/party-service'

const emit = defineEmits<{
  close: [value: boolean]
}>()

// TODO: from props!
const {
  state: partyItems,
  executeImmediate: loadpartyItems,
  isLoading: loadingPartyItems,
} = useAsyncState(
  () => getSelfPartyItems(),
  [],
  { immediate: true, resetOnExecute: false },
)

const onCancel = () => {
  emit('close', false)
}

const sortingConfig: SortingConfig = {
  rank_desc: { field: 'rank', order: 'desc' },
  type_asc: { field: 'type', order: 'asc' },
  // TODO: FIXME: by count
}

const sortingModel = ref<string>('rank_desc')

const { toggleItemDetail } = useItemDetail()

const renderItemDetail = <T extends { id: string }>(opendeItem: T, compareItemsResult: GroupedCompareItemsResult[]) => {
  const partyItem = partyItems.value.find(i => i.item.id === opendeItem.id)

  if (!partyItem) {
    return null
  }

  // TODO: stack item
  return h(ItemDetail, {
    item: partyItem.item,
    compareResult: compareItemsResult,
  })
}
</script>

<template>
  <UDrawer
    direction="top"
    :handle="false"
    handle-only
    :ui="{
      header: 'flex items-center justify-center gap-4',
      container: 'w-full max-w-3xl mx-auto',
      footer: 'flex flex-row justify-end',
    }"
  >
    <template #header>
      <div class="flex flex-1 items-center justify-center gap-4">
        <UiTextView variant="h2">
          Inventory
        </UiTextView>
      </div>

      <div class="mr-0 ml-auto">
        <UButton color="neutral" variant="ghost" icon="i-lucide-x" @click="onCancel" />
      </div>
    </template>

    <template #body>
      <ItemGrid
        v-model:sorting="sortingModel"
        :items="partyItems"
        :sorting-config="sortingConfig"
        size="md"
      >
        <template #item="battleItem">
          <div class="flex flex-col">
            <ItemCard
              class="cursor-pointer"
              :item="battleItem.item"
              @click="(e: Event) => toggleItemDetail(e.target as HTMLElement, battleItem.item.id)"
            >
              <template #badges-bottom-right>
                <UBadge variant="subtle" color="neutral">
                  {{ $n(battleItem.count) }}
                </UBadge>
              </template>
            </ItemCard>
          </div>
        </template>

        <template #item-detail="{ item, compareItemsResult }">
          <component :is="renderItemDetail(item, compareItemsResult)" />
        </template>
      </ItemGrid>
    </template>
  </UDrawer>
</template>

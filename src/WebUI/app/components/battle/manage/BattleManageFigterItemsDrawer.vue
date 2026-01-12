<script setup lang="ts">
import { ItemDetail } from '#components'

import type { GroupedCompareItemsResult } from '~/models/item'
import type { PartyPublic } from '~/models/strategus/party'
import type { SortingConfig } from '~/services/item-search-service'

import { useItemDetail } from '~/composables/item/use-item-detail'
import { getPartyItems } from '~/services/strategus/party-service'

const { party } = defineProps<{
  party: PartyPublic
}>()

const emit = defineEmits<{
  close: [boolean]
}>()

const onCancel = () => {
  emit('close', false)
}

const {
  state: partyItems,
  executeImmediate: loadpartyItems,
  isLoading: loadingPartyItems,
} = useAsyncState(
  () => getPartyItems(party.id),
  [],
  { immediate: true, resetOnExecute: false },
)

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
      header: 'mb-6 flex items-center justify-center gap-4',
      container: 'w-full max-w-3xl mx-auto',
      footer: 'flex flex-row justify-end',
    }"
  >
    <template #header>
      <div class="flex flex-1 items-center justify-center gap-4">
        <UserMedia :user="party.user" />
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
        v-if="Boolean(partyItems.length)"
        v-model:sorting="sortingModel"
        :items="partyItems"
        :sorting-config="sortingConfig"
      >
        <template #item="battleItem">
          <ItemCard
            class="cursor-pointer"
            :item="battleItem.item"
            @click="(e: Event) => toggleItemDetail(e.target as HTMLElement, battleItem.item.id)"
          >
            <template #badges-bottom-right>
              <UBadge :label="$n(battleItem.count)" variant="subtle" />
            </template>
          </ItemCard>
        </template>

        <template #item-detail="{ item, compareItemsResult }">
          <component :is="renderItemDetail(item, compareItemsResult)" />
        </template>
      </ItemGrid>
    </template>
  </UDrawer>
</template>

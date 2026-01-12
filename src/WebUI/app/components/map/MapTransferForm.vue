<script setup lang="ts">
import { ItemDetail } from '#components'

import type { GroupedCompareItemsResult } from '~/models/item'
import type { PartyVisible } from '~/models/strategus/party'
import type { SortingConfig } from '~/services/item-search-service'

import { useItemDetail } from '~/composables/item/use-item-detail'
import { useParty } from '~/composables/strategus/use-party'
import { getSelfPartyItems } from '~/services/strategus/party-service'

interface TransferOffer {
  troops: number
  gold: number
  items: Record<string, number> // itemId/count
}

const { targetParty } = defineProps<{
  targetParty: PartyVisible
}>()

const emit = defineEmits<{
  close: [value: boolean, offer?: TransferOffer]
}>()

const { partyState } = useParty()

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

const offerModel = ref<TransferOffer>({
  gold: 0,
  troops: 0,
  items: {},
})

const isEmptyOffer = computed(() => offerModel.value.gold === 0 && offerModel.value.troops === 0 && Object.keys(offerModel.value.items).length === 0)
const allSelected = ref(false)

watch(allSelected, () => {
  offerModel.value = {
    ...offerModel.value,
    items: allSelected.value
      ? partyItems.value.reduce<TransferOffer['items']>((out, partyItem) => {
          out[partyItem.item.id] = partyItem.count
          return out
        }, {})
      : {},
  }
})

const onCancel = () => {
  emit('close', false)
}

const onSubmit = () => {
  emit('close', true, offerModel.value)
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
  <div class="space-y-8">
    <div class="grid grid-cols-2 gap-4">
      <UFormField>
        <template #label>
          <UiDataMedia label="Gold">
            <template #icon="{ classes }">
              <UiSpriteSymbol
                name="coin"
                viewBox="0 0 18 18"
                :class="classes()"
              />
            </template>
          </UiDataMedia>
        </template>

        <template #hint>
          <UiInputCounter
            :current="offerModel.gold"
            :max="partyState.party.gold"
          />
        </template>

        <UInputNumber
          v-model="offerModel.gold"
          :max="partyState.party.gold"
          :min="0"
          class="w-full"
        />
        <USlider
          v-model="offerModel.gold"
          :min="0"
          class="px-2.5"
          :max="partyState.party.gold"
        />
      </UFormField>

      <UFormField>
        <template #label>
          <UiDataMedia label="Troops" icon="crpg:member" />
        </template>

        <template #hint>
          <UiInputCounter
            :current="offerModel.troops"
            :max="partyState.party.troops"
          />
        </template>

        <UInputNumber
          v-model="offerModel.troops"
          :max="partyState.party.troops"
          :min="0"
          class="w-full"
        />
        <USlider
          v-model="offerModel.troops"
          :min="0"
          class="px-2.5"
          :max="partyState.party.troops"
        />
      </UFormField>
    </div>

    <ItemGrid
      v-model:sorting="sortingModel"
      :items="partyItems"
      :sorting-config="sortingConfig"
      size="md"
    >
      <template #filter-trailing>
        <USwitch v-model="allSelected" label="Select all" />
      </template>
      <template #item="battleItem">
        <div class="flex flex-col">
          <ItemCard
            class="cursor-pointer"
            :item="battleItem.item"
            @click="(e: Event) => toggleItemDetail(e.target as HTMLElement, battleItem.item.id)"
          >
            <template #badges-bottom-right>
              <UBadge variant="subtle" color="neutral">
                {{ $n(battleItem.count - (offerModel.items[battleItem.item.id] || 0)) }}
                <template v-if="offerModel.items[battleItem.item.id]">
                  <UIcon name="i-lucide-chevron-right" />
                  {{ $n(offerModel.items[battleItem.item.id] || 0) }}
                </template>
              </UBadge>
            </template>
          </ItemCard>

          <USlider
            :min="0"
            class="px-2"
            :max="battleItem.count"
            :model-value="offerModel.items[battleItem.item.id] || 0"
            @update:model-value="(count) => {
              offerModel.items[battleItem.item.id] = count || 0
            }"
          />
        </div>
      </template>

      <template #item-detail="{ item, compareItemsResult }">
        <component :is="renderItemDetail(item, compareItemsResult)" />
      </template>
    </ItemGrid>
  </div>
</template>

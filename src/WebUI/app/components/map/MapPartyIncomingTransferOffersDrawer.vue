<script setup lang="ts">
import type { TabsItem } from '@nuxt/ui'

import { ItemDetail } from '#components'

import type { GroupedCompareItemsResult } from '~/models/item'
import type { TransferOfferParty } from '~/models/strategus/party'
import type { SortingConfig } from '~/services/item-search-service'

import { useItemDetail } from '~/composables/item/use-item-detail'
import { useParty } from '~/composables/strategus/use-party'
import { getSelfPartyItems } from '~/services/strategus/party-service'

const { transferOffers } = defineProps<{
  transferOffers: TransferOfferParty[]
}>()

const emit = defineEmits<{
  close: [value: boolean ] // TODO:
}>()

// const offerModel = ref<TransferOffer>({
//   gold: 0,
//   troops: 0,
//   items: {},
// })

// const isEmptyOffer = computed(() => offerModel.value.gold === 0 && offerModel.value.troops === 0 && Object.keys(offerModel.value.items).length === 0)
// const allSelected = ref(false)

// watch(allSelected, () => {
//   offerModel.value = {
//     ...offerModel.value,
//     items: allSelected.value
//       ? partyItems.value.reduce<TransferOffer['items']>((out, partyItem) => {
//           out[partyItem.item.id] = partyItem.count
//           return out
//         }, {})
//       : {},
//   }
// })

const onCancel = () => {
  emit('close', false)
}

const onSubmit = () => {
  emit('close', true)
}

// const sortingConfig: SortingConfig = {
//   rank_desc: { field: 'rank', order: 'desc' },
//   type_asc: { field: 'type', order: 'asc' },
//   // TODO: FIXME: by count
// }

// const sortingModel = ref<string>('rank_desc')

// const { toggleItemDetail } = useItemDetail()

// const renderItemDetail = <T extends { id: string }>(opendeItem: T, compareItemsResult: GroupedCompareItemsResult[]) => {
//   const partyItem = partyItems.value.find(i => i.item.id === opendeItem.id)

//   if (!partyItem) {
//     return null
//   }

//   // TODO: stack item
//   return h(ItemDetail, {
//     item: partyItem.item,
//     compareResult: compareItemsResult,
//   })
// }

const activetransferOfferId = ref<number>(transferOffers[0]?.id ?? 0)

const activetransferOffer = computed(() => transferOffers.find(offer => offer.id === activetransferOfferId.value))

type TransferOfferTabItem = TabsItem & {
  party: TransferOfferParty['party']
}

const transferOffersTabs = computed<TransferOfferTabItem[]>(() => {
  return transferOffers.map(offer => ({
    value: offer.id,
    party: offer.party,
  }))
})
</script>

<template>
  <UDrawer
    direction="top"
    :handle="false"
    handle-only
    :dismissible="false"
    :ui="{
      header: 'flex items-center justify-center gap-4',
      container: 'w-full max-w-3xl mx-auto',
      footer: 'flex flex-row justify-end',
    }"
  >
    <template #header>
      <div class="flex flex-1 flex-wrap items-center justify-center gap-4">
        <UiTextView variant="h2">
          Incoming transfer offers
        </UiTextView>

        <UTabs
          v-model="activetransferOfferId"
          :items="transferOffersTabs"
        >
          <template #default="{ item }">
            <UserMedia :user="item.party.user" />
          </template>
        </UTabs>
      </div>

      <div class="mr-0 ml-auto">
        <UButton
          color="neutral" variant="ghost" icon="i-lucide-x"
          @click="onCancel"
        />
      </div>
    </template>

    <template v-if="activetransferOffer" #body>
      <UCard
        :ui="{
          body: 'space-y-8',
        }"
      >
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
                :current="activetransferOffer.gold"
                :max="activetransferOffer.gold"
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

          <!-- <UFormField>
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
          </UFormField> -->
        </div>

        <!-- <ItemGrid
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
        </ItemGrid> -->
      </UCard>
    </template>

    <template #footer>
      <UButton
        :label="$t('action.cancel')"
        block
        color="neutral"
        variant="soft"
        @click="onCancel"
      />

      <!-- <UButton
        :disabled="isEmptyOffer"
        :label="$t('action.confirm')"
        block
        color="primary"
        variant="soft"
        @click="onSubmit"
      /> -->
    </template>
  </UDrawer>
</template>

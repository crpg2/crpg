<script setup lang="ts">
import { ItemDetail } from '#components'

import type { GroupedCompareItemsResult } from '~/models/item'
import type { ItemStack, TransferOfferPartyUpdate } from '~/models/strategus/party'
import type { SortingConfig } from '~/services/item-search-service'

import { useItemDetail } from '~/composables/item/use-item-detail'

interface TransferOfferModel {
  troops: number
  gold: number
  items: Record<string, number> // itemId/count
}

interface PublicApi {
  submit: () => void
}

const {
  maxGold,
  maxTroops,
  items,
  transferOffer,
  readonly = false,
} = defineProps<{
  maxGold: number
  maxTroops: number
  items: ItemStack[]
  readonly?: boolean
  transferOffer?: TransferOfferPartyUpdate
}>()

const emit = defineEmits<{
  submit: [offer: TransferOfferPartyUpdate]
}>()

const toast = useToast()

const offerModel = ref<TransferOfferModel>(
  transferOffer
    ? {
        gold: transferOffer.gold,
        troops: transferOffer.troops,
        items: transferOffer.items.reduce<Record<string, number>>((out, item) => {
          out[item.itemId] = item.count
          return out
        }, {}),
      }
    : {
        gold: 0,
        troops: 0,
        items: {},
      },
)

const isEmptyOffer = computed(() =>
  offerModel.value.gold === 0
  && offerModel.value.troops === 0
  && Object.keys(offerModel.value.items).length === 0)

const onSubmit = () => {
  if (isEmptyOffer.value) {
    // TODO: i18n
    toast.add({
      title: 'Offer is empty',
      description: 'Please specify at least one of gold, troops or items to transfer.',
      color: 'warning',
    })
    return
  }

  emit('submit', {
    gold: offerModel.value.gold,
    troops: offerModel.value.troops,
    items: Object.entries(offerModel.value.items).map(([itemId, count]) => ({ itemId, count })),
  })
}

defineExpose<PublicApi>({
  submit: onSubmit,
})

const allItemsSelected = ref(false)

watch(allItemsSelected, () => {
  offerModel.value = {
    ...offerModel.value,
    items: allItemsSelected.value
      ? items.reduce<TransferOfferModel['items']>((out, partyItem) => {
          out[partyItem.item.id] = partyItem.count
          return out
        }, {})
      : {},
  }
})

const sortingConfig: SortingConfig = {
  rank_desc: { field: 'rank', order: 'desc' },
  type_asc: { field: 'type', order: 'asc' },
  // TODO: FIXME: by count
}

const sortingModel = ref<string>('rank_desc')

const { toggleItemDetail } = useItemDetail()

const renderItemDetail = <T extends { id: string }>(opendeItem: T, compareItemsResult: GroupedCompareItemsResult[]) => {
  const stackItem = items.find(i => i.item.id === opendeItem.id)

  if (!stackItem) {
    return null
  }

  // TODO: stack item detail
  return h(ItemDetail, {
    item: stackItem.item,
    compareResult: compareItemsResult.find(cr => cr.type === stackItem.item.type)?.compareResult,
  })
}
</script>

<template>
  <UCard
    :ui="{
      header: 'grid grid-cols-2 gap-4',
    }"
  >
    <template #header>
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
            :max="maxGold"
          />
        </template>

        <UInputNumber
          v-model="offerModel.gold"
          :min="0"
          :max="maxGold"
          :readonly
          class="w-full"
        />
        <USlider
          v-model="offerModel.gold"
          :min="0"
          :max="maxGold"
          :disabled="readonly"
          class="px-2.5"
        />
      </UFormField>

      <UFormField>
        <template #label>
          <UiDataMedia label="Troops" icon="crpg:member" />
        </template>

        <template #hint>
          <UiInputCounter
            :current="offerModel.troops"
            :max="maxTroops"
          />
        </template>

        <UInputNumber
          v-model="offerModel.troops"
          :min="0"
          :max="maxTroops"
          :readonly
          class="w-full"
        />
        <USlider
          v-model="offerModel.troops"
          :min="0"
          :max="maxTroops"
          :disabled="readonly"
          class="px-2.5"
        />
      </UFormField>
    </template>

    <ItemGrid
      v-model:sorting="sortingModel"
      :items
      :sorting-config="sortingConfig"
      size="md"
    >
      <template v-if="!readonly" #filter-trailing>
        <USwitch v-model="allItemsSelected" label="Select all" />
      </template>

      <template #item="itemStack">
        <div class="flex flex-col">
          <ItemCard
            class="cursor-pointer"
            :item="itemStack.item"
            @click="(e: Event) => toggleItemDetail(e.target as HTMLElement, itemStack.item.id)"
          >
            <template #badges-bottom-right>
              <UBadge variant="subtle" color="neutral">
                {{ $n(itemStack.count - (offerModel.items[itemStack.item.id] || 0)) }}
                <template v-if="offerModel.items[itemStack.item.id]">
                  <UIcon name="i-lucide-chevron-right" />
                  {{ $n(offerModel.items[itemStack.item.id] || 0) }}
                </template>
              </UBadge>
            </template>
          </ItemCard>

          <!-- TODO: -->
          <UInputNumber
            :min="0"
            :max="itemStack.count"
            :model-value="offerModel.items[itemStack.item.id] || 0"
            :readonly
            class="w-full"
            @update:model-value="(count) => { offerModel.items[itemStack.item.id] = count || 0 }"
          />
          <USlider
            class="px-2"
            :min="0"
            :max="itemStack.count"
            :disabled="readonly"
            :model-value="offerModel.items[itemStack.item.id] || 0"
            @update:model-value="(count) => { offerModel.items[itemStack.item.id] = count || 0 }"
          />
        </div>
      </template>

      <template #item-detail="{ item, compareItemsResult }">
        <component :is="renderItemDetail(item, compareItemsResult)" />
      </template>
    </ItemGrid>

    <template v-if="$slots.footer" #footer>
      <slot name="footer" v-bind="{ submit: onSubmit }" />
    </template>
  </UCard>
</template>

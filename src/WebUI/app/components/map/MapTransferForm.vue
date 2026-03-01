<!-- TODO: RENAME -->
<script setup lang="ts">
import type { ItemStack, TransferOfferPartyUpdate } from '~/models/strategus/party'

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

defineExpose<PublicApi>({
  submit: onSubmit,
})
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

    <ItemStackGrid :items>
      <template #badges-bottom-right="itemStack">
        <UBadge variant="subtle" color="neutral">
          {{ $n(itemStack.count - (offerModel.items[itemStack.item.id] || 0)) }}
          <template v-if="offerModel.items[itemStack.item.id]">
            <UIcon name="i-lucide-chevron-right" />
            {{ $n(offerModel.items[itemStack.item.id] || 0) }}
          </template>
        </UBadge>
      </template>

      <template v-if="!readonly" #item-trailing="itemStack">
        <UiInputNumberSlider
          :min="0"
          :max="itemStack.count"
          :model-value="offerModel.items[itemStack.item.id] || 0"
          @update:model-value="(count) => { offerModel.items[itemStack.item.id] = count || 0 }"
        />
      </template>
    </ItemStackGrid>

    <template v-if="$slots.footer" #footer>
      <slot name="footer" v-bind="{ submit: onSubmit }" />
    </template>
  </UCard>
</template>

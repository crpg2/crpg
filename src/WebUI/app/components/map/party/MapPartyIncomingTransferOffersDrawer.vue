<script setup lang="ts">
import type { TransferOfferParty, TransferOfferPartyUpdate } from '~/models/strategus/party'

const { transferOffers } = defineProps<{
  transferOffers: TransferOfferParty[]
}>()

const emit = defineEmits<{
  close: [
    value: boolean,
    offer?: {
      id: number
      accept: boolean
      accepted?: TransferOfferPartyUpdate
    },
  ]
}>()

const onCancel = () => {
  emit('close', false)
}

const onDecline = (offerId: number) => {
  emit('close', true, { accept: false, id: offerId })
}

const onAccept = (offerId: number, accepted: TransferOfferPartyUpdate) => {
  emit('close', true, { accept: true, id: offerId, accepted })
}

const offers = computed(() => transferOffers.map(offer => ({ ...offer, value: String(offer.id) })))
const openedOffers = ref([offers.value[0]!.value])
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
      </div>

      <div class="mr-0 ml-auto">
        <UButton
          color="neutral" variant="ghost" icon="i-lucide-x"
          @click="onCancel"
        />
      </div>
    </template>

    <template #body>
      <UAccordion
        v-model="openedOffers"
        type="multiple"
        :items="offers"
        :ui="{
          body: 'p-0.5',
        }"
      >
        <template #default="{ item: offer }">
          <UserMedia :user="offer.party.user" />
        </template>

        <template #body="{ item: offer }">
          <MapTransferForm
            :items="offer.items"
            :max-gold="offer.gold"
            :max-troops="offer.troops"
            :transfer-offer="{
              gold: offer.gold,
              troops: offer.troops,
              items: offer.items.map(i => ({ itemId: i.item.id, count: i.count })),
            }"
            @submit="(accepted) => onAccept(offer.id, accepted)"
          >
            <template #footer="{ submit }">
              <div class="flex gap-4">
                <UButton
                  :label="$t('action.decline')"
                  block
                  color="error"
                  variant="soft"
                  @click="() => onDecline(offer.id)"
                />
                <UButton
                  :label="$t('action.accept')"
                  block
                  color="primary"
                  variant="soft"
                  @click="submit"
                />
              </div>
            </template>
          </MapTransferForm>
        </template>
      </UAccordion>
    </template>

    <template #footer>
      <UButton
        :label="$t('action.close')"
        block
        color="neutral"
        variant="soft"
        @click="onCancel"
      />
    </template>
  </UDrawer>
</template>

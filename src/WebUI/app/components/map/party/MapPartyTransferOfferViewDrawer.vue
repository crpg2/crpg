<script setup lang="ts">
import type { ItemStack, TransferOfferParty } from '~/models/strategus/party'

import { useParty } from '~/composables/strategus/use-party'
import { getSelfPartyItems } from '~/services/strategus/party-service'

const { transferOffer } = defineProps<{
  transferOffer: TransferOfferParty
}>()

const emit = defineEmits<{
  close: [value: boolean]
  cancel: []
}>()

const { partyState } = useParty()

const {
  state: items,
  executeImmediate: loadpartyItems,
  isLoading: loadingPartyItems,
} = useAsyncState<ItemStack[]>(async () => {
  return (await getSelfPartyItems())
    .filter(i => transferOffer.items.some(offerItem => offerItem.item.id === i.item.id))
}, [])

const onClose = () => {
  emit('close', false)
}

const onCancel = () => {
  emit('cancel')
}

const transferOfferUpdate = computed(() => {
  return {
    troops: transferOffer.troops,
    gold: transferOffer.gold,
    items: transferOffer.items.map(i => ({ itemId: i.item.id, count: i.count })),
  }
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
      <div class="flex flex-1 items-center justify-center gap-4">
        <UiTextView variant="h2">
          Transfer to
        </UiTextView>

        <UserMedia :user="transferOffer.targetParty.user" />
      </div>

      <div class="mr-0 ml-auto">
        <UButton color="neutral" variant="ghost" icon="i-lucide-x" @click="onCancel" />
      </div>
    </template>

    <template #body>
      <UCard
        :ui="{
          body: 'space-y-8',
        }"
      >
        <MapTransferForm
          :max-gold="partyState.party.gold"
          :max-troops="partyState.party.troops"
          :items
          :transfer-offer="transferOfferUpdate"
          readonly
        />
      </UCard>
    </template>

    <template #footer>
      <UButton
        :label="$t('action.close')"
        block
        color="neutral"
        variant="soft"
        @click="onClose"
      />

      <UButton
        :label="$t('action.cancel')"
        block
        icon="i-lucide-x"
        color="error"
        variant="soft"
        @click="onCancel"
      />
    </template>
  </UDrawer>
</template>

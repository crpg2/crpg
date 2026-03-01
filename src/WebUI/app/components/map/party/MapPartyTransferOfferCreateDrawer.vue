<script setup lang="ts">
import {
  strategusMinPartyTroops,
} from '~root/data/constants.json'

import type { PartyVisible, TransferOfferPartyUpdate } from '~/models/strategus/party'

import { useParty, usePartyItems } from '~/composables/strategus/use-party'

const { targetParty } = defineProps<{
  targetParty: PartyVisible
}>()

const emit = defineEmits<{
  close: [value: boolean, offer?: TransferOfferPartyUpdate]
}>()

const { partyState } = useParty()
const { partyItems } = usePartyItems(true)

const transferForm = useTemplateRef('transferForm')

const onCancel = () => {
  emit('close', false)
}

const onSubmit = (offer: TransferOfferPartyUpdate) => {
  emit('close', true, offer)
}

const maxTroops = computed(() => Math.max(0, partyState.value.party.troops - strategusMinPartyTroops))
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

        <UserMedia :user="targetParty.user" />
      </div>

      <div class="mr-0 ml-auto">
        <UButton color="neutral" variant="ghost" icon="i-lucide-x" @click="onCancel" />
      </div>
    </template>

    <template #body>
      <MapTransferForm
        ref="transferForm"
        :max-gold="partyState.party.gold"
        :max-troops
        :items="partyItems"
        @submit="onSubmit"
      />
    </template>

    <template #footer>
      <UButton
        :label="$t('action.cancel')"
        block
        color="neutral"
        variant="soft"
        @click="onCancel"
      />

      <UButton
        :label="$t('action.confirm')"
        block
        color="primary"
        variant="soft"
        @click="transferForm?.submit()"
      />
    </template>
  </UDrawer>
</template>

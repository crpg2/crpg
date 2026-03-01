<script setup lang="ts">
import type { ItemStack, ItemStackUpdate } from '~/models/strategus/party'
import type { SettlementPublic } from '~/models/strategus/settlement'

import { useParty, usePartyItems } from '~/composables/strategus/use-party'

const { settlementItems } = defineProps<{
  settlement: SettlementPublic
  settlementItems: ItemStack[]
}>()

const emit = defineEmits<{
  close: [value: boolean, items?: ItemStackUpdate[]]
}>()

const { partyState } = useParty()
const { partyItems, loadingPartyItems } = usePartyItems(true)

const onCancel = () => {
  emit('close', false)
}

const onSubmit = (items: ItemStackUpdate[]) => {
  emit('close', true, items)
}

const transferForm = useTemplateRef('transferForm')
</script>

<template>
  <UDrawer
    direction="top"
    :handle="false"
    handle-only
    :dismissible="false"
    :ui="{
      header: 'flex items-center justify-center gap-4',
      container: 'w-full max-w-6xl mx-auto',
      footer: 'flex flex-row justify-end sticky z-10 bottom-0',
    }"
  >
    <template #header>
      <div class="flex flex-1 items-center justify-center gap-4">
        <UiTextView variant="h2">
          Manage settlement inventory
        </UiTextView>
      </div>

      <div class="mr-0 ml-auto">
        <UButton color="neutral" variant="ghost" icon="i-lucide-x" @click="onCancel" />
      </div>
    </template>

    <template #body>
      <MapItemsSplitTransferForm
        v-if="!loadingPartyItems"
        ref="transferForm"
        :from="settlementItems"
        :to="partyItems"
        @submit="onSubmit"
      >
        <template #left-side-header>
          <SettlementMedia :settlement />
        </template>
        <template #right-side-header>
          <UserMedia :user="partyState.party.user" />
        </template>
      </MapItemsSplitTransferForm>
    </template>

    <template #footer>
      <UButton
        :label="$t('action.reset')"
        block
        color="neutral"
        variant="soft"
        @click="transferForm?.reset()"
      />

      <UButton
        :label="$t('action.submit')"
        block
        color="primary"
        variant="soft"
        @click="transferForm?.submit()"
      />
    </template>
  </UDrawer>
</template>

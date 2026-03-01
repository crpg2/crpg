<script setup lang="ts">
import { strategusMinPartyTroops } from '~root/data/constants.json'

import type { SettlementPublic } from '~/models/strategus/settlement'

import { useParty } from '~/composables/strategus/use-party'

const { settlement } = defineProps<{
  settlement: SettlementPublic
}>()

const emit = defineEmits<{
  close: [value: boolean, troops?: number]
}>()

const { partyState } = useParty()

const maxTroops = computed(() => {
  return partyState.value.party.troops + settlement.troops
})

function getEmptyTransferModel() {
  return {
    troops: settlement.troops,
  }
}

const transferModel = ref(getEmptyTransferModel())

function resetTransferModel() {
  transferModel.value = getEmptyTransferModel()
}

const troopsInSettlement = computed(() => transferModel.value.troops)
const troopsInParty = computed(() => maxTroops.value - transferModel.value.troops)

const onCancel = () => {
  emit('close', false)
}

const onReset = () => {
  resetTransferModel()
}

const onSubmit = () => {
  emit('close', true, transferModel.value.troops)
}
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
          Manage settlement troops
        </UiTextView>
      </div>

      <div class="mr-0 ml-auto">
        <UButton color="neutral" variant="ghost" icon="i-lucide-x" @click="onCancel" />
      </div>
    </template>

    <template #body>
      <UCard
        :ui="{
          footer: 'flex justify-end gap-2',
        }"
        label="Manage troops"
        icon="crpg:member"
      >
        <UFormField
          :ui="{
            description: 'flex justify-between flex-wrap text-highlighted gap-4 text-sm',
            help: 'flex justify-between flex-wrap text-highlighted gap-4 text-sm',
          }"
        >
          <template #description>
            <SettlementMedia :settlement />
            <UserMedia :user="partyState.party.user" />
          </template>

          <template #help>
            <div class="flex items-center gap-4">
              <div class="flex items-center gap-1">
                <UiDataMedia icon="crpg:member" :label="$n(settlement.troops)" />
                <template v-if="settlement.troops !== troopsInSettlement">
                  <UIcon name="i-lucide-chevron-right" />
                  <div>
                    {{ $n(troopsInSettlement) }}
                    <span
                      :class="[troopsInSettlement > settlement.troops ? 'text-success' : `
                        text-error
                      `]"
                    >
                      ({{ troopsInSettlement > settlement.troops ? '+' : '' }}{{ $n(troopsInSettlement - settlement.troops) }})
                    </span>
                  </div>
                </template>
              </div>
            </div>
            <div class="flex flex-row-reverse items-center gap-2">
              <div class="flex flex-row-reverse items-center">
                <UiDataMedia icon="crpg:member" :label="$n(partyState.party.troops)" />
                <template v-if="partyState.party.troops !== troopsInParty">
                  <UIcon name="i-lucide-chevron-left" />
                  <div>
                    {{ $n(troopsInParty) }}
                    <span
                      :class="[troopsInParty > partyState.party.troops ? 'text-success' : `
                        text-error
                      `]"
                    >
                      ({{ troopsInParty > partyState.party.troops ? '+' : '' }}{{ $n(troopsInParty - partyState.party.troops) }})
                    </span>
                  </div>
                </template>
              </div>
            </div>
          </template>

          <UInputNumber
            v-model="transferModel.troops"
            :min="strategusMinPartyTroops"
            :max="maxTroops - strategusMinPartyTroops"
            class="w-full"
            increment-icon="i-lucide-arrow-right"
            decrement-icon="i-lucide-arrow-left"
          />

          <USlider
            v-model="transferModel.troops"
            :min="strategusMinPartyTroops"
            :max="maxTroops - strategusMinPartyTroops"
            class="px-2.5"
          />
        </UFormField>
      </UCard>
    </template>

    <template #footer>
      <UButton
        :label="$t('action.reset')"
        block
        color="neutral"
        variant="soft"
        @click="onReset"
      />

      <UButton
        :label="$t('action.submit')"
        block
        color="primary"
        variant="soft"
        @click="onSubmit"
      />
    </template>
  </UDrawer>
</template>

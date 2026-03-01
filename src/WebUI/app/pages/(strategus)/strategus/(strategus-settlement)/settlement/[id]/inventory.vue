<script setup lang="ts">
import { LazySettlementItemsManageDrawer } from '#components'
import { strategusMinPartyTroops } from '~root/data/constants.json'

import type { ItemStackUpdate, PartyPublic } from '~/models/strategus/party'
import type { SettlementPublic } from '~/models/strategus/settlement'

import { useParty, usePartyItems } from '~/composables/strategus/use-party'
import { useSettlement, useSettlementItems } from '~/composables/strategus/use-settlements'
import { useUser } from '~/composables/user/use-user'
import { checkCanEditSettlementInventory } from '~/services/strategus/settlement-service'

definePageMeta({
  middleware: [
    () => {
      const { settlement } = useSettlement()
      const { user } = useUser()

      if (!checkCanEditSettlementInventory(settlement.value, user.value!)) {
        return navigateTo({ name: 'strategus-settlement-id', params: { id: settlement.value.id } })
      }
    },
  ],
})

const route = useRoute<'strategus-settlement-id-inventory'>()

const toast = useToast()

const {
  settlement,
  refreshSettlement,
  updateSettlementResources,
} = useSettlement()

const { settlementItems, pendingSettlementItems, loadSettlementItems, updateSettlementItems } = useSettlementItems()

const { user } = useUser()
const { partyState, updateParty } = useParty()
const { loadpartyItems } = usePartyItems()

const maxTroops = computed(() => {
  return partyState.value.party.troops + settlement.value.troops
})

function getEmptyTransferModel() {
  return {
    troops: settlement.value.troops,
  }
}

const transferModel = ref(getEmptyTransferModel())

function resetTransferModel() {
  transferModel.value = getEmptyTransferModel()
}

const [submitTransferModel, submittingTransferModel] = useAsyncCallback(async () => {
  await updateSettlementResources(transferModel.value.troops)

  await Promise.all([
    refreshSettlement(),
    updateParty(),
  ])
})

const troopsInSettlement = computed(() => transferModel.value.troops)
const troopsInParty = computed(() => maxTroops.value - transferModel.value.troops)

const overlay = useOverlay()

const openSettlementItemsManageDrawer = () => {
  overlay.create(LazySettlementItemsManageDrawer).open({
    settlementItems: settlementItems.value,
    settlement: settlement.value,
    async onClose(_result, items) {
      if (!_result || !items) {
        return
      }

      await updateSettlementItems(items)

      await Promise.all([
        loadSettlementItems(),
        loadpartyItems(),
      ])

      toast.add({
        title: 'Settlement items updated',
        color: 'success',
      })
    },
  })
}
</script>

<template>
  <div>
    <ItemStackGrid :items="settlementItems">
      <template #filter-trailing>
        <UButton
          variant="subtle"
          label="Manage"
          size="lg"
          @click="openSettlementItemsManageDrawer"
        />
      </template>
    </ItemStackGrid>

    <!-- <UiCard
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
                    :class="[troopsInSettlement > settlement.troops ? 'text-success' : `text-error`]"
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
                    :class="[troopsInParty > partyState.party.troops ? 'text-success' : `text-error`]"
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

      <template #footer>
        <UButton
          :label="$t('action.reset')"
          block
          variant="soft"
          color="neutral"
          @click="resetTransferModel"
        />
        <UButton
          :label="$t('action.submit')"
          block
          variant="soft"
          :loading="submittingTransferModel"
          @click="submitTransferModel"
        />
      </template>
    </UiCard> -->
  </div>
</template>

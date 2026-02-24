<script setup lang="ts">
import { ItemDetail } from '#components'
import { strategusMinPartyTroops } from '~root/data/constants.json'

import type { GroupedCompareItemsResult, Item } from '~/models/item'
import type { PartyPublic } from '~/models/strategus/party'
import type { SettlementPublic } from '~/models/strategus/settlement'
import type { SortingConfig } from '~/services/item-search-service'

import { useItemDetail } from '~/composables/item/use-item-detail'
import { useParty } from '~/composables/strategus/use-party'
import { useSettlement, useSettlementItems } from '~/composables/strategus/use-settlements'
import { useUser } from '~/composables/user/use-user'
import { getSettlementItems } from '~/services/strategus/settlement-service'

// definePageMeta({
//   middleware: [
//     // TODO: validate
//     () => {
//       const { battle } = useMapBattle()
//       const { selfBattleFighter } = useBattleFighters()

//       if (!selfBattleFighter.value) {
//         return navigateTo({ name: 'strategus-battle-id', params: { id: battle.value.id } })
//       }
//     },
//   ],
// })

const route = useRoute<'strategus-settlement-id-inventory'>()
const { settlement, refreshSettlement, updateSettlementResources } = useSettlement()

const { settlementItems } = useSettlementItems()

const { user } = useUser()
const { partyState, updateParty } = useParty()

const sortingConfig: SortingConfig = {
  rank_desc: { field: 'rank', order: 'desc' },
  type_asc: { field: 'type', order: 'asc' },
  // TODO: FIXME: by count
}

const sortingModel = ref<string>('rank_desc')

const { toggleItemDetail } = useItemDetail()

const renderItemDetail = <T extends { id: string }>(opendeItem: T, compareItemsResult: GroupedCompareItemsResult[]) => {
  const partyItem = settlementItems.value.find(i => i.item.id === opendeItem.id)

  if (!partyItem) {
    return null
  }

  // TODO: stack item detail
  return h(ItemDetail, {
    item: partyItem.item,
    compareResult: compareItemsResult.find(cr => cr.type === partyItem.item.type)?.compareResult,
  })
}

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
</script>

<template>
  <div>
    <div class="mx-auto mt-8 max-w-2xl">
      <UiCard
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

        <template #footer>
          <UButton
            label="Reset"
            variant="subtle"
            @click="resetTransferModel"
          />
          <UButton
            label="Submit"
            :loading="submittingTransferModel"
            @click="submitTransferModel"
          />
        </template>
      </UiCard>
    </div>

    <ItemGrid
      v-model:sorting="sortingModel"
      class="mx-auto max-w-2xl"
      :sorting-config="sortingConfig"
      :items="settlementItems"
    >
      <template #item="battleItem">
        <ItemCard
          class="cursor-pointer"
          :item="battleItem.item"
          @click="(e: Event) => toggleItemDetail(e.target as HTMLElement, battleItem.item.id)"
        >
          <template #badges-bottom-right>
            <UBadge :label="$n(battleItem.count)" variant="subtle" @click.stop />
          </template>
        </ItemCard>
      </template>

      <template #item-detail="{ item, compareItemsResult }">
        <component :is="renderItemDetail(item, compareItemsResult)" />
      </template>
    </ItemGrid>
  </div>
</template>

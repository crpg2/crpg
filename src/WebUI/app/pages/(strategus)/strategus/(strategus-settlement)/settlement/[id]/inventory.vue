<script setup lang="ts">
import { ItemDetail } from '#components'

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
const { settlement } = useSettlement()

const { settlementItems } = useSettlementItems()

const { user } = useUser()
const { partyState } = useParty()

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

const offerModel = ref({ troops: settlement.value.troops })

// Вычисляемые значения для отображения
const troopsInSettlement = computed(() => offerModel.value.troops)
const troopsInParty = computed(() => maxTroops.value - offerModel.value.troops)
</script>

<template>
  <div>
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

    <div class="mx-auto mt-8 max-w-2xl">
      <UFormField>
        <div class="flex justify-between text-sm">
          <div class="flex items-center">
            settlement: {{ $n(settlement.troops) }}
            <UIcon name="i-lucide-chevron-right" />
            <span
              class="font-bold"
              :class="[troopsInSettlement > settlement.troops ? 'text-success' : `text-error`]"
            >
              {{ $n(troopsInSettlement) }}
            </span>
          </div>
          <div class="flex items-center">
            <span
              class="font-bold"
              :class="[troopsInSettlement > settlement.troops ? 'text-success' : `text-error`]"
            >
              party: {{ $n(troopsInParty) }}
            </span>
          </div>
        </div>

        <UInputNumber
          v-model="offerModel.troops"
          :min="0"
          :max="maxTroops"
          class="w-full"
        />

        <USlider
          v-model="offerModel.troops"
          :min="0"
          :max="maxTroops"
          :step="1"
          class="px-2.5"
        />

        <!-- <div
            class="
              text-center text-xs text-gray-500
              dark:text-gray-400
            "
          >
            {{ $t('strategus.settlement.troopsSliderHint') }}
          </div> -->
      </UFormField>
    </div>
  </div>
</template>

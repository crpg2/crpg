<script setup lang="ts">
import { ItemDetail } from '#components'

import type { GroupedCompareItemsResult, Item } from '~/models/item'
import type { PartyPublic } from '~/models/strategus/party'
import type { SettlementPublic } from '~/models/strategus/settlement'
import type { SortingConfig } from '~/services/item-search-service'

import { useItemDetail } from '~/composables/item/use-item-detail'
import { useSettlementItems } from '~/composables/strategus/use-settlements'
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

const { settlementItems } = useSettlementItems()

const { user } = useUser()

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
</script>

<template>
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
</template>

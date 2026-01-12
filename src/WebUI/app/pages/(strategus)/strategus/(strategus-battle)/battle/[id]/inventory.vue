<script setup lang="ts">
import { ItemDetail } from '#components'

import type { GroupedCompareItemsResult, Item } from '~/models/item'
import type { BattleFighterInventory } from '~/models/strategus/battle'
import type { PartyPublic } from '~/models/strategus/party'
import type { SettlementPublic } from '~/models/strategus/settlement'
import type { SortingConfig } from '~/services/item-search-service'

import { useItemDetail } from '~/composables/item/use-item-detail'
import { useBattleFighters, useBattleItems, useMapBattle } from '~/composables/strategus/map/use-map-battle'
import { useUser } from '~/composables/user/use-user'

definePageMeta({
  middleware: [
    () => {
      const { battle } = useMapBattle()
      const { selfBattleFighter } = useBattleFighters()

      if (!selfBattleFighter.value) {
        return navigateTo({ name: 'strategus-battle-id', params: { id: battle.value.id } })
      }
    },
  ],
})

const {
  battleItems,
  loadingBattleItems, // TODO: loader
} = useBattleItems()

const { user } = useUser()

interface CountByFighter {
  [fighterId: number]: {
    party: PartyPublic | null
    settlement: SettlementPublic | null
    count: number
  }
}

interface AggregatedBattleItems {
  item: Item
  count: number
  countByFighter: CountByFighter
}

function aggregateBattleItems(inventories: BattleFighterInventory[]): AggregatedBattleItems[] {
  const map = new Map<string, AggregatedBattleItems>()

  for (const inv of inventories) {
    for (const stack of inv.items) {
      const key = stack.item.id

      if (!map.has(key)) {
        map.set(key, {
          item: stack.item,
          count: stack.count,
          countByFighter: {
            [inv.fighterId]: {
              party: inv.party ?? null,
              settlement: inv.settlement ?? null,
              count: stack.count,
            },
          },
        })
      }
      else {
        const entry = map.get(key)!
        entry.count += stack.count

        if (inv.fighterId in entry.countByFighter) {
          entry.countByFighter[inv.fighterId]!.count += stack.count
        }
        else {
          entry.countByFighter[inv.fighterId] = {
            party: inv.party ?? null,
            settlement: inv.settlement ?? null,
            count: stack.count,
          }
        }
      }
    }
  }

  return [...map.values()]
}

const items = computed(() => aggregateBattleItems(battleItems.value))

const sortingConfig: SortingConfig = {
  rank_desc: { field: 'rank', order: 'desc' },
  type_asc: { field: 'type', order: 'asc' },
  // TODO: FIXME: by count
}

const sortingModel = ref<string>('rank_desc')

const { toggleItemDetail } = useItemDetail()

const renderItemDetail = <T extends { id: string }>(opendeItem: T, compareItemsResult: GroupedCompareItemsResult[]) => {
  const partyItem = items.value.find(i => i.item.id === opendeItem.id)

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
    :items
  >
    <template #item="battleItem">
      <ItemCard
        class="cursor-pointer"
        :item="battleItem.item"
        @click="(e: Event) => toggleItemDetail(e.target as HTMLElement, battleItem.item.id)"
      >
        <!-- to cmp -->
        <template #badges-bottom-right>
          <UPopover>
            <UBadge :label="$n(battleItem.count)" variant="subtle" @click.stop />

            <template #content>
              <div class="flex flex-col gap-3.5">
                <UiDataCell v-for="(fighter, fighterId) in battleItem.countByFighter" :key="fighterId">
                  <template v-if="fighter.party" #leftContent>
                    <UserMedia :user="fighter.party.user" :is-self="fighter.party.user.id === user!.id" />
                  </template>
                  <!-- hack ;) -->
                  <div />
                  <template #rightContent>
                    <UiTextView variant="p" class="font-bold">
                      {{ $n(fighter.count) }}
                    </UiTextView>
                  </template>
                </UiDataCell>
              </div>
            </template>
          </UPopover>
        </template>
      </ItemCard>
    </template>

    <template #item-detail="{ item, compareItemsResult }">
      <component :is="renderItemDetail(item, compareItemsResult)" />
    </template>
  </ItemGrid>
</template>

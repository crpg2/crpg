<script setup lang="ts">
import { vOnLongPress } from '@vueuse/components'
import { useStorage } from '@vueuse/core'

import type { UserItem } from '~/models/user'
import type { SortingConfig } from '~/services/item-search-service'

import { useMainHeader } from '~/composables/app/use-main-header'
import { useInventoryDnD } from '~/composables/character/inventory/use-inventory-dnd'
import { useInventoryQuickEquip } from '~/composables/character/inventory/use-inventory-quick-equip'
import { useItemDetail } from '~/composables/character/inventory/use-item-detail'
import { useCharacterCharacteristic } from '~/composables/character/use-character-characteristic'
import { useCharacterItems } from '~/composables/character/use-character-items'
import { useClanMembers } from '~/composables/clan/use-clan-members'
import { usePageLoading } from '~/composables/utils/use-page-loading'
import {
  checkUpkeepIsHigh,
  validateItemNotMeetRequirement,
} from '~/services/character-service'
import {
  addItemToClanArmory,
  getClanArmoryItemLender,
  removeItemFromClanArmory,
  returnItemToClanArmory,
} from '~/services/clan-service'
import { getAggregationsConfig } from '~/services/item-search-service'
import { createItemIndex } from '~/services/item-search-service/indexator'
import { getCompareItemsResult, groupItemsByTypeAndWeaponClass } from '~/services/item-service'
import { extractItemFromUserItem } from '~/services/user-service'

const userStore = useUserStore()
const { clan, user, userItems } = toRefs(userStore)

const { mainHeaderHeight } = useMainHeader()

const { equippedItemsBySlot, itemsOverallStats, equippedItemIds, updatingCharacterItems } = useCharacterItems()

const { characterCharacteristics, healthPoints } = useCharacterCharacteristic()

const { onDragEnd, onDragStart, dragging } = useInventoryDnD(equippedItemsBySlot)

const { onQuickEquip } = useInventoryQuickEquip(equippedItemsBySlot)

const {
  closeItemDetail,
  getUniqueId,
  openedItems,
  toggleItemDetail,
} = useItemDetail()

const upkeepIsHigh = computed(() => checkUpkeepIsHigh(user.value!.gold, itemsOverallStats.value.averageRepairCostByHour))

// const refreshData = async () => {
//   await Promise.all([
//     userStore.fetchUser(),
//     userStore.fetchUserItems(),
//     loadCharacterItems(0, { id: character.value.id }),
//   ])
// }

// const { execute: onSellUserItem, loading: sellingUserItem } = useAsyncCallback(async (userItemId: number) => {
//   // unEquip linked slots
//   const characterItem = characterItems.value.find(ci => ci.userItem.id === userItemId)
//   if (characterItem !== undefined) {
//     await updateCharacterItems(character.value.id, [
//       ...getLinkedSlots(characterItem.slot, equippedItemsBySlot.value).map(ls => ({
//         slot: ls,
//         userItemId: null,
//       })),
//     ])
//   }
//   // if the item sold is the last item in the active category,
//   // you must reset the filter because that category is no longer in inventory
//   if (filteredUserItems.value.length === 1) {
//     filterByTypeModel.value = []
//   }
//   await sellUserItem(userItemId)
//   await refreshData()
//   notify(t('character.inventory.item.sell.notify.success'))
// })

// const { execute: onRepairUserItem, loading: repairingUserItem } = useAsyncCallback(async (userItemId: number) => {
//   await repairUserItem(userItemId)
//   await Promise.all([userStore.fetchUser(), userStore.fetchUserItems()])
//   notify(t('character.inventory.item.repair.notify.success'))
// })

// const { execute: onUpgradeUserItem, loading: upgradingUserItem } = useAsyncCallback(async (userItemId: number) => {
//   await upgradeUserItem(userItemId)
//   await refreshData()
//   notify(t('character.inventory.item.upgrade.notify.success'))
// })

// const { execute: onReforgeUserItem, loading: reforgingUserItem } = useAsyncCallback(async (userItemId: number) => {
//   await reforgeUserItem(userItemId)
//   await refreshData()
//   notify(t('character.inventory.item.reforge.notify.success'))
// })

// const { execute: onAddItemToClanArmory, loading: addingItemToClanArmory } = useAsyncCallback(async (userItemId: number) => {
//   if (filteredUserItems.value.length === 1) {
//     filterByTypeModel.value = []
//   }
//   await addItemToClanArmory(clan.value!.id, userItemId)
//   await refreshData()
//   notify(t('clan.armory.item.add.notify.success'))
// })

// const { execute: onReturnToClanArmory, loading: returningItemToClanArmory } = useAsyncCallback(async (userItemId: number) => {
//   await returnItemToClanArmory(clan.value!.id, userItemId)
//   if (filteredUserItems.value.length === 1) {
//     filterByTypeModel.value = []
//   }
//   await refreshData()
//   notify(t('clan.armory.item.return.notify.success'))
// })

// const { execute: onRemoveFromClanArmory, loading: removingItemToClanArmory } = useAsyncCallback(async (userItemId: number) => {
//   await removeItemFromClanArmory(clan.value!.id, userItemId)
//   await refreshData()
//   notify(t('clan.armory.item.remove.notify.success'))
// })

const onClickInventoryItem = (e: PointerEvent, userItem: UserItem) => {
  if (e.ctrlKey) {
    onQuickEquip(userItem)
    return
  }
  toggleItemDetail(e.target as HTMLElement, {
    id: userItem.item.id,
    userItemId: userItem.id,
  })
}

// const hideInArmoryItemsModel = useStorage<boolean>('character-inventory-in-armory-items', true)
// const hasArmoryItems = computed(() => userItems.value.some(ui => ui.isArmoryItem))

const flatItems = computed(() =>
  createItemIndex(
    extractItemFromUserItem(
      userItems.value,
      // .filter(item =>
      //   hideInArmoryItemsModel.value && item.isArmoryItem ? item.userId !== user.value!.id : true,
      // ),
    ),
  ),
)

// const sortingConfig: SortingConfig = {
//   price_asc: {
//     field: 'price',
//     order: 'asc',
//   },
//   price_desc: {
//     field: 'price',
//     order: 'desc',
//   },
//   rank_desc: {
//     field: 'rank',
//     order: 'desc',
//   },
//   type_asc: {
//     field: 'type',
//     order: 'asc',
//   },
// }

// const aggregationConfig = {
//   type: {
//     chosen_filters_on_top: false,
//     conjunction: false,
//     description: '',
//     size: 1000,
//     sort: 'term',
//     title: 'type',
//     view: AggregationView.Radio,
//   },
// } as AggregationConfig

// const filterByTypeModel = ref<ItemType[]>([])
const filterByNameModel = ref<string>('')

const sortingConfig: SortingConfig = {
  price_asc: {
    field: 'price',
    order: 'asc',
  },
  price_desc: {
    field: 'price',
    order: 'desc',
  },
  rank_desc: {
    field: 'rank',
    order: 'desc',
  },
  type_asc: {
    field: 'type',
    order: 'asc',
  },
}
const sortingModel = useStorage<string>('character-inventory-sorting', 'rank_desc')

// const searchResult = computed(() =>
//   getSearchResult({
//     aggregationConfig,
//     filter: {
//       type: filterByTypeModel.value,
//     },
//     items: flatItems.value,
//     page: 1,
//     perPage: 1000,
//     query: filterByNameModel.value,
//     sort: sortingModel.value,
//     sortingConfig,
//     userItemsIds: [],
//   }),
// )

// const filteredUserItems = computed(() => {
//   const foundedItemIds = searchResult.value.data.items.map(item => item.id)
//   return userItems.value
//     .filter(
//       item =>
//         foundedItemIds.includes(item.item.id)
//         && (hideInArmoryItemsModel.value && item.isArmoryItem ? item.userId !== user.value!.id : true),
//     )
//     .sort((a, b) => {
//       if (sortingModel.value === 'type_asc') {
//         const itemTypes = Object.values(ItemType)
//         return itemTypes.indexOf(a.item.type) - itemTypes.indexOf(b.item.type)
//       }
//       return foundedItemIds.indexOf(a.item.id) - foundedItemIds.indexOf(b.item.id)
//     })
// })

// const totalItemsCost = computed(() => filteredUserItems.value.reduce((out, item) => out + item.item.price, 0))

// TODO:
const compareItemsResult = computed(() => {
  // find the open items TODO: spec
  return groupItemsByTypeAndWeaponClass(
    createItemIndex(
      extractItemFromUserItem(
        userItems.value.filter(ui =>
          openedItems.value.some(oi => oi.uniqueId === getUniqueId(ui.item.id, ui.id)),
        ),
      ),
    ),
  )
    .filter(group => group.items.length >= 2) // there is no point in comparing 1 item;
    .map(group => ({
      compareResult: getCompareItemsResult(
        group.items,
        getAggregationsConfig(group.type, group.weaponClass),
      ),
      type: group.type,
      weaponClass: group.weaponClass,
    }))
})

const { clanMembers, loadClanMembers } = useClanMembers()

const fetchPageData = () => {
  const promises: Promise<any>[] = [userStore.fetchUserItems()]
  if (clan.value) {
    promises.push(loadClanMembers(0, { id: clan.value.id }))
  }
  Promise.all(promises)
}

fetchPageData()

const { togglePageLoading } = usePageLoading()

watchEffect(() => {
  // console.log('d', updatingCharacterItems.value)
  // TODO: FIXME:
  // togglePageLoading(updatingCharacterItems.value)
})

// const openedItems = ref<OpenedItem[]>([])
</script>

<template>
  <div class="relative grid grid-cols-12 gap-5">
    <div class="col-span-5">
      <ItemGrid
        v-if="Boolean(userItems.length)"
        v-model:sorting="sortingModel"
        :items="userItems"
        :sorting-config="sortingConfig"
      >
        <template #item="userItem">
          <CharacterInventoryItemCard
            v-on-long-press="[() => !dragging && onQuickEquip(userItem), { delay: 500 }]"
            class="cursor-grab"
            :user-item="userItem"
            :equipped="equippedItemIds.includes(userItem.id)"
            :not-meet-requirement="validateItemNotMeetRequirement(userItem.item, characterCharacteristics)"
            :lender="getClanArmoryItemLender(userItem, clanMembers)"
            draggable="true"
            @dragstart="onDragStart(userItem)"
            @dragend="onDragEnd"
            @click="(e: PointerEvent) => onClickInventoryItem(e, userItem)"
          />
        </template>
      </ItemGrid>
      <UCard v-else>
        <UiResultNotFound :message="$t('character.inventory.empty')" />
      </UCard>
    </div>

    <div
      class="sticky top-0 left-0 col-span-5 self-start"
      :style="{ top: `calc(${mainHeaderHeight}px + 1rem)` }"
    >
      <CharacterInventoryDoll
        :character-characteristics="characterCharacteristics"
        :equipped-items="equippedItemsBySlot"
        :items-stats-overall="itemsOverallStats"
      />

      <div
        class="mt-3 flex w-full justify-center rounded-lg bg-elevated p-4 backdrop-blur-lg"
        style="grid-area: footer"
      >
        <UiKbdCombination
          :keys="[$t('shortcuts.keys.ctrl'), $t('shortcuts.keys.lmb')]"
          :label="$t('shortcuts.hints.equip')"
        />
      </div>
    </div>

    <div
      :style="{ top: `calc(${mainHeaderHeight}px + 1rem)` }"
      class="sticky col-span-2 self-start"
    >
      <CharacterStats
        class="text-2xs"
        :characteristics="characterCharacteristics"
        :weight="itemsOverallStats.weight"
        :longest-weapon-length="itemsOverallStats.longestWeaponLength"
        :health-points="healthPoints"
      >
        <template #leading>
          <UiSimpleTableRow
            :label="$t('character.stats.price.title')"
            :tooltip="{
              title: $t('character.stats.price.title'),
              description: $t('character.stats.price.desc'),
            }"
          >
            <AppCoin :value="itemsOverallStats.price" />
          </UiSimpleTableRow>

          <UiSimpleTableRow
            :label="$t('character.stats.avgRepairCost.title')"
            :tooltip="{
              title: $t('character.stats.avgRepairCost.title'),
              description: $t('character.stats.avgRepairCost.desc'),
            }"
          >
            <AppCoin :class="{ 'text-error': upkeepIsHigh }">
              <span class="font-bold">
                {{ $n(itemsOverallStats.averageRepairCostByHour) }} / {{ $t('dateTime.hours.short') }}
              </span>
            </AppCoin>

            <!-- TODO: design -->
            <template v-if="upkeepIsHigh" #tooltip-content>
              <div class="prose prose-invert">
                <h4 class="text-status-warning">
                  {{ $t('character.highUpkeepWarning.title') }}
                </h4>
                <div v-html="$t('character.highUpkeepWarning.desc')" />
              </div>
            </template>
          </UiSimpleTableRow>
        </template>
      </CharacterStats>
    </div>

    <ItemDetailGroup>
      <template #default="di">
        <CharacterInventoryItemDetail
          :user-item="userItems.find(ui => ui.id === di.userItemId)!"
          :equipped="equippedItemIds.includes(di.userItemId)"
          :lender="getClanArmoryItemLender(userItems.find(ui => ui.id === di.userItemId)!, clanMembers)"
          :compare-result="compareItemsResult.find(cr => cr.type === flatItems.find(fi => fi.id === di.id)!.type)?.compareResult"
        />
      </template>
    </ItemDetailGroup>
  </div>
</template>

<!--
          @sell="
            () => {
              closeItemDetail(di);
              onSellUserItem(di.userItemId);
            }
          "
          @repair="
            () => {
              closeItemDetail(di);
              onRepairUserItem(di.userItemId);
            }
          "
          @upgrade="
            () => {
              closeItemDetail(di);
              onUpgradeUserItem(di.userItemId);
            }
          "
          @reforge="
            () => {
              closeItemDetail(di);
              onReforgeUserItem(di.userItemId);
            }
          "
          @return-to-clan-armory="
            () => {
              closeItemDetail(di);
              onReturnToClanArmory(di.userItemId);
            }
          "
          @remove-from-clan-armory="
            () => {
              closeItemDetail(di);
              onRemoveFromClanArmory(di.userItemId);
            }
          "
          @add-to-clan-armory="
            () => {
              closeItemDetail(di);
              onAddItemToClanArmory(di.userItemId);
            }
          " -->

<style lang="css">
.inventoryGrid {
  grid-template-areas:
    '...... sort'
    'filter items'
    'filter footer';
  grid-template-columns: auto 1fr;
  grid-template-rows: auto 1fr auto;
}
</style>

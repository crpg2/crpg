<script setup lang="ts">
import { vOnLongPress } from '@vueuse/components'
import { useStorage } from '@vueuse/core'

import type { EquippedItemId } from '~/models/character'
import type { AggregationConfig, SortingConfig } from '~/models/item-search'
import type { UserItem, UserItemsBySlot } from '~/models/user'

import { useInventoryDnD } from '~/composables/character/use-inventory-dnd'
import { useInventoryQuickEquip } from '~/composables/character/use-inventory-quick-equip'
import { useItemDetail } from '~/composables/character/use-item-detail'
import { useClanMembers } from '~/composables/clan/use-clan-members'
import { useStickySidebar } from '~/composables/use-sticky-sidebar'
import { useAsyncCallback } from '~/composables/utils/use-async-callback'
import { ItemType } from '~/models/item'
import { AggregationView } from '~/models/item-search'
import {
  checkUpkeepIsHigh,
  updateCharacterItems,
  validateItemNotMeetRequirement,
} from '~/services/characters-service'
import {
  addItemToClanArmory,
  getClanArmoryItemLender,
  removeItemFromClanArmory,
  returnItemToClanArmory,
} from '~/services/clan-service'
import { getAggregationsConfig, getSearchResult } from '~/services/item-search-service'
import { createItemIndex } from '~/services/item-search-service/indexator'
import {
  getCompareItemsResult,
  getLinkedSlots,
  groupItemsByTypeAndWeaponClass,
} from '~/services/item-service'
import { notify } from '~/services/notification-service'
import { t } from '~/services/translate-service'
import { extractItemFromUserItem, reforgeUserItem, repairUserItem, sellUserItem, upgradeUserItem } from '~/services/users-service'
import { useUserStore } from '~/stores/user'
import {
  characterCharacteristicsKey,
  characterHealthPointsKey,
  characterItemsKey,
  characterItemsStatsKey,
  characterKey,
  equippedItemsBySlotKey,
} from '~/symbols/character'
import { mainHeaderHeightKey } from '~/symbols/common'
import { scrollToTop } from '~/utils/scroll'

definePage({
  meta: {
    roles: ['User', 'Moderator', 'Admin'],
  },
  props: true,
})

const userStore = useUserStore()
const { clan, user, userItems } = toRefs(userStore)

const character = injectStrict(characterKey)
const { characterCharacteristics } = injectStrict(characterCharacteristicsKey)
const healthPoints = injectStrict(characterHealthPointsKey)
const { characterItems, loadCharacterItems } = injectStrict(characterItemsKey)
const itemsStats = injectStrict(characterItemsStatsKey)
const mainHeaderHeight = injectStrict(mainHeaderHeightKey)

const upkeepIsHigh = computed(() =>
  checkUpkeepIsHigh(user.value!.gold, itemsStats.value.averageRepairCostByHour),
)
const equippedItemsIds = computed(() => characterItems.value.map(ei => ei.userItem.id))

const { execute: onChangeEquippedItems, loading: changingEquippedItems } = useAsyncCallback(async (items: EquippedItemId[]) => {
  await updateCharacterItems(character.value.id, items)
  await loadCharacterItems(0, { id: character.value.id })
})

const refreshData = async () => {
  await Promise.all([
    userStore.fetchUser(),
    userStore.fetchUserItems(),
    loadCharacterItems(0, { id: character.value.id }),
  ])
}

const { execute: onSellUserItem, loading: sellingUserItem } = useAsyncCallback(async (userItemId: number) => {
  // unEquip linked slots
  const characterItem = characterItems.value.find(ci => ci.userItem.id === userItemId)
  if (characterItem !== undefined) {
    await updateCharacterItems(character.value.id, [
      ...getLinkedSlots(characterItem.slot, equippedItemsBySlot.value).map(ls => ({
        slot: ls,
        userItemId: null,
      })),
    ])
  }
  // if the item sold is the last item in the active category,
  // you must reset the filter because that category is no longer in inventory
  if (filteredUserItems.value.length === 1) {
    filterByTypeModel.value = []
  }
  await sellUserItem(userItemId)
  await refreshData()
  notify(t('character.inventory.item.sell.notify.success'))
})

const { execute: onRepairUserItem, loading: repairingUserItem } = useAsyncCallback(async (userItemId: number) => {
  await repairUserItem(userItemId)
  await Promise.all([userStore.fetchUser(), userStore.fetchUserItems()])
  notify(t('character.inventory.item.repair.notify.success'))
})

const { execute: onUpgradeUserItem, loading: upgradingUserItem } = useAsyncCallback(async (userItemId: number) => {
  await upgradeUserItem(userItemId)
  await refreshData()
  notify(t('character.inventory.item.upgrade.notify.success'))
})

const { execute: onReforgeUserItem, loading: reforgingUserItem } = useAsyncCallback(async (userItemId: number) => {
  await reforgeUserItem(userItemId)
  await refreshData()
  notify(t('character.inventory.item.reforge.notify.success'))
})

const { execute: onAddItemToClanArmory, loading: addingItemToClanArmory } = useAsyncCallback(async (userItemId: number) => {
  if (filteredUserItems.value.length === 1) {
    filterByTypeModel.value = []
  }
  await addItemToClanArmory(clan.value!.id, userItemId)
  await refreshData()
  notify(t('clan.armory.item.add.notify.success'))
})

const { execute: onReturnToClanArmory, loading: returningItemToClanArmory } = useAsyncCallback(async (userItemId: number) => {
  await returnItemToClanArmory(clan.value!.id, userItemId)
  if (filteredUserItems.value.length === 1) {
    filterByTypeModel.value = []
  }
  await refreshData()
  notify(t('clan.armory.item.return.notify.success'))
})

const { execute: onRemoveFromClanArmory, loading: removingItemToClanArmory } = useAsyncCallback(async (userItemId: number) => {
  await removeItemFromClanArmory(clan.value!.id, userItemId)
  await refreshData()
  notify(t('clan.armory.item.remove.notify.success'))
})

const onClickInventoryItem = (e: PointerEvent, userItem: UserItem) => {
  if (e.ctrlKey) {
    onQuickEquip(userItem)
  }
  else {
    toggleItemDetail(e.target as HTMLElement, {
      id: userItem.item.id,
      userItemId: userItem.id,
    })
  }
}

const hideInArmoryItemsModel = useStorage<boolean>('character-inventory-in-armory-items', true)
const hasArmoryItems = computed(() => userItems.value.some(ui => ui.isArmoryItem))

const flatItems = computed(() =>
  createItemIndex(
    extractItemFromUserItem(
      userItems.value.filter(item =>
        hideInArmoryItemsModel.value && item.isArmoryItem ? item.userId !== user.value!.id : true,
      ),
    ),
  ),
)

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
const aggregationConfig = {
  type: {
    chosen_filters_on_top: false,
    conjunction: false,
    description: '',
    size: 1000,
    sort: 'term',
    title: 'type',
    view: AggregationView.Radio,
  },
} as AggregationConfig

const filterByTypeModel = ref<ItemType[]>([])
const filterByNameModel = ref<string>('')
const sortingModel = useStorage<string>('character-inventory-sorting', 'rank_desc')

const searchResult = computed(() =>
  getSearchResult({
    aggregationConfig,
    filter: {
      type: filterByTypeModel.value,
    },
    items: flatItems.value,
    page: 1,
    perPage: 1000,
    query: filterByNameModel.value,
    sort: sortingModel.value,
    sortingConfig,
    userItemsIds: [],
  }),
)

const filteredUserItems = computed(() => {
  const foundedItemIds = searchResult.value.data.items.map(item => item.id)
  return userItems.value
    .filter(
      item =>
        foundedItemIds.includes(item.item.id)
        && (hideInArmoryItemsModel.value && item.isArmoryItem ? item.userId !== user.value!.id : true),
    )
    .sort((a, b) => {
      if (sortingModel.value === 'type_asc') {
        const itemTypes = Object.values(ItemType)
        return itemTypes.indexOf(a.item.type) - itemTypes.indexOf(b.item.type)
      }
      return foundedItemIds.indexOf(a.item.id) - foundedItemIds.indexOf(b.item.id)
    })
})

const totalItemsCost = computed(() =>
  filteredUserItems.value.reduce((out, item) => out + item.item.price, 0),
)

const equippedItemsBySlot = computed(() =>
  characterItems.value.reduce((out, ei) => {
    out[ei.slot] = ei.userItem as UserItem
    return out
  }, {} as UserItemsBySlot),
)
provide(equippedItemsBySlotKey, equippedItemsBySlot)

const { onDragEnd, onDragStart, dragging } = useInventoryDnD(equippedItemsBySlot)
const { onQuickEquip } = useInventoryQuickEquip(equippedItemsBySlot)

const { closeItemDetail, getUniqueId, openedItems, toggleItemDetail } = useItemDetail()

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

const aside = ref<HTMLDivElement | null>(null)
const { top: stickySidebarTop } = useStickySidebar(aside, mainHeaderHeight.value + 16, 16)

const promises: Promise<any>[] = [userStore.fetchUserItems()]

const { clanMembers, loadClanMembers } = useClanMembers()

if (clan.value) {
  promises.push(loadClanMembers(0, { id: clan.value.id }))
}

await Promise.all(promises)
</script>

<template>
  <div class="relative grid grid-cols-12 gap-5">
    <OLoading
      full-page
      :active="
        changingEquippedItems
          || sellingUserItem
          || repairingUserItem
          || upgradingUserItem
          || reforgingUserItem
          || addingItemToClanArmory
          || returningItemToClanArmory
          || removingItemToClanArmory"
      icon-size="xl"
    />
    <div class="col-span-5">
      <template v-if="userItems.length !== 0">
        <div class="inventoryGrid relative grid h-full gap-x-3 gap-y-4">
          <div
            ref="aside"
            style="grid-area: filter"
            class="sticky space-y-2"
            :style="{ top: `${stickySidebarTop}px` }"
          >
            <OButton
              v-if="hasArmoryItems"
              v-tooltip.bottom="
                hideInArmoryItemsModel
                  ? $t('character.inventory.filter.showInArmory')
                  : $t('character.inventory.filter.hideInArmory')
              "
              :variant="hideInArmoryItemsModel ? 'secondary' : 'primary'"
              outlined
              size="xl"
              rounded
              icon-left="armory"
              @click="
                () => {
                  hideInArmoryItemsModel = !hideInArmoryItemsModel;
                  filterByTypeModel = [];
                }
              "
            />

            <ItemGridFilter
              v-if="'type' in searchResult.data.aggregations"
              v-model="filterByTypeModel"
              :buckets="searchResult.data.aggregations.type.buckets"
              @click="scrollToTop"
            />
          </div>

          <div
            class="grid grid-cols-3 gap-4 2xl:grid-cols-4"
            style="grid-area: sort"
          >
            <div class="col-span-2 2xl:col-span-3">
              <OInput
                v-model="filterByNameModel"
                type="text"
                expanded
                clearable
                :placeholder="$t('action.search')"
                icon="search"
                rounded
                size="sm"
              />
            </div>

            <ItemGridSearch
              v-model="sortingModel"
              class="col-span-1"
              :config="sortingConfig"
            />
          </div>

          <div
            class="relative h-full"
            style="grid-area: items"
          >
            <div class="grid grid-cols-3 gap-2 2xl:grid-cols-4">
              <CharacterInventoryItemCard
                v-for="userItem in filteredUserItems"
                :key="userItem.id"
                v-on-long-press="[
                  () => {
                    !dragging && onQuickEquip(userItem)
                  },
                  { delay: 500 },
                ]"
                class="cursor-grab"
                :user-item="userItem"
                :equipped="equippedItemsIds.includes(userItem.id)"
                :not-meet-requirement="
                  validateItemNotMeetRequirement(userItem.item, characterCharacteristics)
                "
                :lender="getClanArmoryItemLender(userItem, clanMembers)"
                draggable="true"
                @dragstart="onDragStart(userItem)"
                @dragend="onDragEnd"
                @click="e => onClickInventoryItem(e, userItem)"
              />
            </div>
          </div>

          <div
            class="sticky bottom-4 left-0 z-10 flex w-full justify-center rounded-lg bg-base-200 bg-opacity-60 p-4 backdrop-blur-lg"
            style="grid-area: footer"
          >
            <i18n-t
              scope="global"
              keypath="character.inventory.total.tpl"
              tag="div"
              :plural="filteredUserItems.length"
            >
              <template #count>
                <i18n-t
                  scope="global"
                  keypath="character.inventory.total.count"
                  tag="span"
                  :plural="filteredUserItems.length"
                >
                  <template #count>
                    <span class="font-bold text-content-100">
                      {{ filteredUserItems.length }}
                    </span>
                  </template>
                </i18n-t>
              </template>

              <template #sum>
                <i18n-t
                  scope="global"
                  tag="span"
                  keypath="character.inventory.total.sum"
                >
                  <template #sum>
                    <Coin :value="totalItemsCost" />
                  </template>
                </i18n-t>
              </template>
            </i18n-t>
          </div>
        </div>
      </template>

      <ResultNotFound
        v-else
        class="rounded-xl border border-dashed border-border-300"
        :message="$t('character.inventory.empty')"
      />
    </div>

    <div
      class="sticky left-0 col-span-5 self-start"
      :style="{ top: `${mainHeaderHeight + 16}px` }"
    >
      <CharacterInventoryDoll @change="onChangeEquippedItems" />
      <div
        class="mt-3 flex w-full justify-center rounded-lg bg-base-200 p-4 backdrop-blur-lg"
        style="grid-area: footer"
      >
        <KbdCombination
          :keys="[$t('shortcuts.keys.ctrl'), $t('shortcuts.keys.lmb')]"
          :label="$t('shortcuts.hints.equip')"
        />
      </div>
    </div>

    <div
      class="sticky col-span-2 grid grid-cols-1 items-start gap-2 self-start rounded-lg border border-border-200 py-2 text-2xs"
      :style="{ top: `${mainHeaderHeight + 16}px` }"
    >
      <SimpleTableRow
        :label="$t('character.stats.price.title')"
        :tooltip="{
          title: $t('character.stats.price.title'),
          description: $t('character.stats.price.desc'),
        }"
      >
        <div class="inline-flex gap-1.5 align-middle">
          <SvgSpriteImg
            name="coin"
            viewBox="0 0 18 18"
            class="w-4"
          />
          <span class="text-xs font-bold text-primary">{{ $n(itemsStats.price) }}</span>
        </div>
      </SimpleTableRow>

      <SimpleTableRow
        :label="$t('character.stats.avgRepairCost.title')"
        :tooltip="{
          title: $t('character.stats.avgRepairCost.title'),
          description: $t('character.stats.avgRepairCost.desc'),
        }"
      >
        <div class="inline-flex gap-1.5 align-middle">
          <SvgSpriteImg
            name="coin"
            viewBox="0 0 18 18"
            class="w-4"
          />
          <ClosableTooltip
            :disabled="!upkeepIsHigh"
            shown
            placement="top"
          >
            <span
              class="text-xs font-bold"
              :class="[upkeepIsHigh ? 'text-status-danger' : 'text-primary']"
            >
              {{ $n(itemsStats.averageRepairCostByHour) }} / {{ $t('dateTime.hours.short') }}
            </span>
            <template #popper>
              <div class="prose prose-invert">
                <h4 class="text-status-warning">
                  {{ $t('character.highUpkeepWarning.title') }}
                </h4>
                <div v-html="$t('character.highUpkeepWarning.desc')" />
              </div>
            </template>
          </ClosableTooltip>
        </div>
      </SimpleTableRow>

      <CharacterStats
        :characteristics="characterCharacteristics"
        :weight="itemsStats.weight"
        :longest-weapon-length="itemsStats.longestWeaponLength"
        :health-points="healthPoints"
        :mount-speed-base="itemsStats.mountSpeedBase"
        :mount-harness-weight="itemsStats.mountHarnessWeight"
      />
    </div>

    <ItemDetailGroup>
      <template #default="di">
        <CharacterInventoryItemDetail
          :compare-result="
            compareItemsResult.find(cr => cr.type === flatItems.find(fi => fi.id === di.id)!.type)
              ?.compareResult
          "
          :user-item="userItems.find(ui => ui.id === di.userItemId)!"
          :equipped="equippedItemsIds.includes(di.userItemId)"
          :lender="
            getClanArmoryItemLender(userItems.find(ui => ui.id === di.userItemId)!, clanMembers)
          "
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
          "
        />
      </template>
    </ItemDetailGroup>
  </div>
</template>

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

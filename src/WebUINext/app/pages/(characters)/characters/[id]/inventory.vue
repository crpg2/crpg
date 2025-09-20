<script setup lang="ts">
import type { DropdownMenuItem } from '@nuxt/ui'

import { vOnLongPress } from '@vueuse/components'
import { useStorage } from '@vueuse/core'

import type { UserItem } from '~/models/user'
import type { SortingConfig } from '~/services/item-search-service'

import { useMainHeader } from '~/composables/app/use-main-header'
import { useCharacterInventory } from '~/composables/character/inventory/use-character-inventory'
import { useInventoryDnD } from '~/composables/character/inventory/use-inventory-dnd'
import { useInventoryQuickEquip } from '~/composables/character/inventory/use-inventory-quick-equip'
import { useItemDetail } from '~/composables/character/inventory/use-item-detail'
import { useCharacterCharacteristic } from '~/composables/character/use-character-characteristic'
import { useCharacterItems, useCharacterItemsProvider } from '~/composables/character/use-character-items'
import { useUserItemsProvider } from '~/composables/user/use-user-items'
import { validateItemNotMeetRequirement } from '~/services/character-service'
import { getClanArmoryItemLender, getClanMembers } from '~/services/clan-service'
import { getAggregationsConfig } from '~/services/item-search-service'
import { createItemIndex } from '~/services/item-search-service/indexator'
import { extractItem, getCompareItemsResult, groupItemsByTypeAndWeaponClass } from '~/services/item-service'

const userStore = useUserStore()
const { clan, user } = toRefs(userStore)

const { data: userItems, pending: loadingUserItems } = useUserItemsProvider()
useCharacterItemsProvider()

const { t } = useI18n()
const { mainHeaderHeight } = useMainHeader()

const {
  equippedItemsBySlot,
  itemsOverallStats,
  equippedItemIds,
  upkeepIsHigh,
} = useCharacterItems()

const { onDragEnd, onDragStart, dragging } = useInventoryDnD()
const { onQuickEquip } = useInventoryQuickEquip()

const {
  onSellUserItem,
  onRepairUserItem,
  onUpgradeUserItem,
  onReforgeUserItem,
  onAddItemToClanArmory,
  onRemoveFromClanArmory,
  onReturnToClanArmory,
} = useCharacterInventory()

const { characterCharacteristics, healthPoints } = useCharacterCharacteristic()

const hasArmoryItems = computed(() => userItems.value.some(ui => ui.isArmoryItem))

const {
  closeItemDetail,
  getUniqueId,
  openedItems,
  toggleItemDetail,
} = useItemDetail()

const onClickInventoryItem = (e: PointerEvent, userItem: UserItem) => {
  if (e.ctrlKey) {
    return onQuickEquip(userItem)
  }
  toggleItemDetail(e.target as HTMLElement, {
    id: userItem.item.id,
    userItemId: userItem.id,
  })
}

const sortingConfig: SortingConfig = {
  price_asc: { field: 'price', order: 'asc' },
  price_desc: { field: 'price', order: 'desc' },
  rank_desc: { field: 'rank', order: 'desc' },
  type_asc: { field: 'type', order: 'asc' },
}

const sortingModel = useStorage<string>('character-inventory-sorting', 'rank_desc')

// TODO: FIXME: to composable
const compareItemsResult = computed(() => {
  // find the open items TODO: spec
  return groupItemsByTypeAndWeaponClass(
    // TODO: ....
    createItemIndex(
      userItems.value
        .filter(ui => openedItems.value.some(oi => oi.uniqueId === getUniqueId(ui.item.id, ui.id)),
        ).map(extractItem),
    ),
  )
    .filter(group => group.items.length >= 2) // there is no point in comparing 1 item
    .map(group => ({
      compareResult: getCompareItemsResult(
        group.items,
        getAggregationsConfig(group.type, group.weaponClass),
      ),
      type: group.type,
      weaponClass: group.weaponClass,
    }))
})

const {
  state: clanMembers,
} = useAsyncState(
  async () => clan.value ? getClanMembers(clan.value.id) : [],
  [],
)

const hideInArmoryItemsModel = useStorage<boolean>('character-inventory-in-armory-items', true)
const additionalFilteritems = computed<DropdownMenuItem[]>(() => [
  {
    label: t('character.inventory.filter.hideInArmory'),
    type: 'checkbox' as const,
    icon: 'crpg:armory',
    checked: hideInArmoryItemsModel.value,
    onUpdateChecked(checked: boolean) {
      hideInArmoryItemsModel.value = checked
    },
    onSelect(e: Event) {
      e.preventDefault()
    },
  },
])

const items = computed(() => {
  if (!hideInArmoryItemsModel.value) {
    return userItems.value
  }
  return userItems.value.filter(ui => ui.isArmoryItem ? ui.userId !== user.value!.id : true)
})
</script>

<template>
  <div class="relative grid grid-cols-12 gap-5">
    <div class="col-span-5">
      <ItemGrid
        v-if="Boolean(userItems.length)"
        v-model:sorting="sortingModel"
        :items
        :sorting-config="sortingConfig"
        :with-pagination="false"
      >
        <template
          v-if="hasArmoryItems"
          #filter-leading
        >
          <UDropdownMenu
            :items="additionalFilteritems"
            :modal="false"
          >
            <UChip
              inset
              size="2xl"
              :show="hideInArmoryItemsModel"
              :ui="{ base: 'bg-[var(--color-notification)]' }"
            >
              <UButton
                variant="outline"
                color="neutral"
                size="xl"
                icon="crpg:dots"
              />
            </UChip>
          </UDropdownMenu>
        </template>

        <template #item="userItem">
          <CharacterInventoryItemCard
            v-on-long-press="[() => !dragging && onQuickEquip(userItem), { delay: 500 }]"
            class="cursor-grab"
            :user-item="userItem"
            :equipped="equippedItemIds.includes(userItem.id)"
            :not-meet-requirement="validateItemNotMeetRequirement(userItem.item, characterCharacteristics)"
            :lender="userItem.isArmoryItem ? getClanArmoryItemLender(userItem.userId, clanMembers) : null"
            draggable="true"
            @dragstart="onDragStart(userItem)"
            @dragend="onDragEnd"
            @click="(e: PointerEvent) => onClickInventoryItem(e, userItem)"
          />
        </template>

        <template #footer="{ filteredItemsCost, filteredItemsCount }">
          <UCard
            variant="soft"
            :ui="{ body: 'justify-center flex', root: 'backdrop-blur-lg' }"
          >
            <i18n-t
              scope="global"
              keypath="character.inventory.total.tpl"
              tag="div"
              :plural="filteredItemsCount"
            >
              <template #count>
                <i18n-t
                  scope="global"
                  keypath="character.inventory.total.count"
                  :plural="filteredItemsCount"
                >
                  <template #count>
                    <span class="font-bold text-highlighted">
                      {{ filteredItemsCount }}
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
                    <AppCoin :value="filteredItemsCost" />
                  </template>
                </i18n-t>
              </template>
            </i18n-t>
          </UCard>
        </template>
      </ItemGrid>

      <UCard v-else-if="!loadingUserItems">
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

      <UCard
        style="grid-area: footer"
        variant="soft"
        class="mt-3"
        :ui="{ body: 'justify-center flex', root: 'backdrop-blur-lg' }"
      >
        <UiKbdCombination
          :keys="[$t('shortcuts.keys.ctrl'), $t('shortcuts.keys.lmb')]"
          :label="$t('shortcuts.hints.equip')"
        />
      </UCard>
    </div>

    <div
      :style="{ top: `calc(${mainHeaderHeight}px + 1rem)` }"
      class="sticky col-span-2 self-start"
    >
      <CharacterStats
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
                <h4 class="text-warning">
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
          :lender="getClanArmoryItemLender(userItems.find(ui => ui.id === di.userItemId)!.userId, clanMembers)"
          :compare-result="compareItemsResult.find(cr => cr.type === userItems.find(fi => fi.item.id === di.id)!.item.type)?.compareResult"
          @sell="() => {
            closeItemDetail(di); // TODO: copy pasta
            onSellUserItem(di.userItemId);
          }"
          @repair="() => {
            closeItemDetail(di);
            onRepairUserItem(di.userItemId);
          }"
          @upgrade="() => {
            closeItemDetail(di);
            onUpgradeUserItem(di.userItemId);
          }"
          @reforge="() => {
            closeItemDetail(di);
            onReforgeUserItem(di.userItemId);
          }"
          @add-to-clan-armory="() => {
            closeItemDetail(di);
            onAddItemToClanArmory(di.userItemId);
          }"
          @remove-from-clan-armory="() => {
            closeItemDetail(di);
            onRemoveFromClanArmory(di.userItemId);
          }"
          @return-to-clan-armory=" () => {
            closeItemDetail(di);
            onReturnToClanArmory(di.userItemId);
          }"
        />
      </template>
    </ItemDetailGroup>
  </div>
</template>

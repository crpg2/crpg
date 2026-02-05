<script setup lang="ts">
import { vOnLongPress } from '@vueuse/components'
import { useStorage } from '@vueuse/core'
import { CharacterInventoryItemDetail } from '#components'

import type { GroupedCompareItemsResult, ItemSlot } from '~/models/item'
import type { UserItem } from '~/models/user'
import type { SortingConfig } from '~/services/item-search-service'

import { useMainHeader } from '~/composables/app/use-main-header'
import { useCharacterInventory } from '~/composables/character/inventory/use-character-inventory'
import { useInventoryDnD } from '~/composables/character/inventory/use-inventory-dnd'
import { useInventoryQuickEquip } from '~/composables/character/inventory/use-inventory-quick-equip'
import { useCharacterCharacteristic } from '~/composables/character/use-character-characteristic'
import { useCharacterItems, useCharacterItemsProvider } from '~/composables/character/use-character-items'
import { useItemDetail } from '~/composables/item/use-item-detail'
import { useUser } from '~/composables/user/use-user'
import { useUserItemsProvider } from '~/composables/user/use-user-items'
import { validateItemNotMeetRequirement } from '~/services/character-service'
import { getClanArmoryItemLender, getClanMembers } from '~/services/clan-service'

const { clan, user } = useUser()

const { data: userItems, pending: loadingUserItems } = useUserItemsProvider()
useCharacterItemsProvider()

const { mainHeaderHeight } = useMainHeader()

const {
  equippedItemsBySlot,
  itemsOverallStats,
  equippedItemIds,
  upkeepIsHigh,
} = useCharacterItems()

const { onDragEnd, onDragStart, dragging } = useInventoryDnD()
const { onQuickEquip, onQuickUnEquip } = useInventoryQuickEquip()

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

const { closeItemDetail, toggleItemDetail } = useItemDetail()

const onClickInventoryItem = (e: PointerEvent, userItem: UserItem, slot?: ItemSlot) => {
  if (e.ctrlKey) {
    slot ? onQuickUnEquip(slot) : onQuickEquip(userItem)
    return
  }
  toggleItemDetail(e.target as HTMLElement, userItem.item.id)
}

const sortingConfig: SortingConfig = {
  price_asc: { field: 'price', order: 'asc' },
  price_desc: { field: 'price', order: 'desc' },
  rank_desc: { field: 'rank', order: 'desc' },
  type_asc: { field: 'type', order: 'asc' },
}
const sortingModel = useStorage<string>('character-inventory-sorting', 'rank_desc')

const { state: clanMembers } = useAsyncState(
  async () => clan.value ? getClanMembers(clan.value.id) : [],
  [],
)

const hideInArmoryItemsModel = useStorage<boolean>('character-inventory-in-armory-items', true)

const items = computed(() => {
  if (!hideInArmoryItemsModel.value) {
    return userItems.value
  }
  // filter by isArmoryItem
  return userItems.value.filter(ui => ui.isArmoryItem ? ui.userId !== user.value!.id : true)
})

const renderCharacterInventoryItemDetail = <T extends { id: string }>(opendeItem: T, compareItemsResult: GroupedCompareItemsResult[]) => {
  const userItem = userItems.value.find(i => i.item.id === opendeItem.id)

  if (!userItem) {
    return null
  }

  return h(CharacterInventoryItemDetail, {
    userItem,
    equipped: equippedItemIds.value.includes(userItem.id),
    lender: getClanArmoryItemLender(userItem.userId, clanMembers.value),
    compareResult: compareItemsResult.find(cr => cr.type === userItem.item.type)?.compareResult,
    onSell: () => {
      onSellUserItem(userItem.id)
      closeItemDetail(opendeItem.id)
    },
    onRepair: () => onRepairUserItem(userItem.id),
    onUpgrade: () => onUpgradeUserItem(userItem.id),
    onReforge: () => onReforgeUserItem(userItem.id),
    onAddToClanArmory: () => {
      onAddItemToClanArmory(userItem.id)
      closeItemDetail(opendeItem.id)
    },
    onRemoveFromClanArmory: () => onRemoveFromClanArmory(userItem.id),
    onReturnToClanArmory: () => {
      onReturnToClanArmory(userItem.id)
      closeItemDetail(opendeItem.id)
    },
  })
}
</script>

<template>
  <div class="relative grid grid-cols-12 gap-5">
    <div class="col-span-5">
      <ItemGrid
        v-model:sorting="sortingModel"
        :items
        :loading="loadingUserItems"
        :sorting-config="sortingConfig"
        :with-pagination="false"
      >
        <template
          v-if="hasArmoryItems"
          #filter-trailing
        >
          <UDropdownMenu
            :items="[
              {
                label: $t('character.inventory.filter.hideInArmory'),
                type: 'checkbox',
                icon: 'crpg:armory',
                checked: hideInArmoryItemsModel,
                onUpdateChecked(checked: boolean) {
                  hideInArmoryItemsModel = checked
                },
              },
            ]"
            :modal="false"
            size="xl"
          >
            <UChip
              inset
              size="2xl"
              :show="hideInArmoryItemsModel"
              :ui="{ base: 'bg-notification' }"
            >
              <UButton
                variant="outline"
                color="neutral"
                size="xl"
                icon="i-lucide-ellipsis-vertical"
              />
            </UChip>
          </UDropdownMenu>
        </template>

        <template #item="userItem">
          <CharacterInventoryItemCard
            v-on-long-press="[() => !dragging && onQuickEquip(userItem), { delay: 500 }]"
            class="cursor-grab"
            :user-item
            :user-id="user!.id"
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

        <template #item-detail="{ item, compareItemsResult }">
          <component :is="renderCharacterInventoryItemDetail(item, compareItemsResult)" />
        </template>
      </ItemGrid>
    </div>

    <div
      :style="{ top: `calc(${mainHeaderHeight}px + 1rem)` }"
      class="sticky left-0 col-span-5 space-y-3 self-start"
    >
      <CharacterInventoryDoll
        :character-characteristics="characterCharacteristics"
        :equipped-items="equippedItemsBySlot"
        :items-stats-overall="itemsOverallStats"
        @item-click="(e, itemId, slot) => onClickInventoryItem(e, userItems.find(ui => ui.item.id === itemId)!, slot)"
        @un-equip="onQuickUnEquip"
      />
      <UCard
        style="grid-area: footer"
        variant="soft"
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
        :items-overall-stats="itemsOverallStats"
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
  </div>
</template>

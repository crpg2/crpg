<script setup lang="ts">
import type { SelectItem } from '@nuxt/ui'

import type { UserItem } from '~/models/user'
import type { SortingConfig } from '~/services/item-search-service'

import { useMainHeader } from '~/composables/app/use-main-header'
import { useStickySidebar } from '~/composables/character/use-sticky-sidebar'
import { ItemType } from '~/models/item'
import { filterItemsByType, getFacetsByItemType } from '~/services/item-search-service'
import { createItemIndex } from '~/services/item-search-service/indexator'
import { extractItemFromUserItem } from '~/services/user-service'

const { sortingConfig, items } = defineProps<{
  sortingConfig: SortingConfig
  items: UserItem[]
}>()

const { t } = useI18n()

const { mainHeaderHeight } = useMainHeader()
const aside = useTemplateRef('aside')
const { top: stickySidebarTop } = useStickySidebar(aside, mainHeaderHeight.value + 16, 16)

const filterByNameModel = ref<string>('')
const sortModel = defineModel<string>('sorting', { default: '' })

const sortingItems = computed(() => Object.entries(sortingConfig).map<SelectItem>(([key, value]) => ({
  label: t(`item.sort.${key}`),
  value: key,
})))

const flatItems = computed(() =>
  createItemIndex(extractItemFromUserItem(items)),
)

const itemType = ref<ItemType>(ItemType.Undefined)
const itemTypes = computed(() => {
  return getFacetsByItemType(flatItems.value)
})

const filteredItems = computed(() => {
  const d = filterItemsByType(flatItems.value, itemType.value)

  console.log(d.map(dd => ({ d: dd.requirement, ddd: dd.id })))

  return d
  // return userItems.value
  //   .filter(
  //     item =>
  //       foundedItemIds.includes(item.item.id)
  //       && (hideInArmoryItemsModel.value && item.isArmoryItem ? item.userId !== user.value!.id : true),
  //   )
  //   .sort((a, b) => {
  //     if (sortingModel.value === 'type_asc') {
  //       const itemTypes = Object.values(ItemType)
  //       return itemTypes.indexOf(a.item.type) - itemTypes.indexOf(b.item.type)
  //     }
  //     return foundedItemIds.indexOf(a.item.id) - foundedItemIds.indexOf(b.item.id)
  // })
})
</script>

<template>
  <div class="itemGrid grid h-full items-start gap-x-3 gap-y-4">
    <div
      ref="aside"
      style="grid-area: filter"
      class="sticky top-0 left-0 space-y-2"
      :style="{ top: `${stickySidebarTop}px` }"
    >
      <AppFilterItemsByType
        v-model:item-type="itemType"
        :item-types="itemTypes"
        orientation="vertical"
        with-all-categories
      />

      <!-- <OButton
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
          /> -->
    </div>

    <div
      class="
        grid grid-cols-3 gap-4
        2xl:grid-cols-4
      "
      style="grid-area: sort"
    >
      <div
        class="
          col-span-2
          2xl:col-span-3
        "
      >
        <UInput
          v-model="filterByNameModel"
          :placeholder="$t('action.search')"
          icon="crpg:search"
          variant="subtle"
          class="w-full"
        >
          <template v-if="filterByNameModel?.length" #trailing>
            <UButton
              color="neutral"
              variant="link"
              icon="crpg:close"
              @click="filterByNameModel = ''"
            />
          </template>
        </UInput>
      </div>

      <USelect
        v-model="sortModel"
        :items="sortingItems"
        trailing-icon="crpg:arrow-up-down"
      />
    </div>

    <div
      class="
        grid grid-cols-3 gap-2
        2xl:grid-cols-4
      " style="grid-area: items"
    >
      <template v-for="item in items" :key="item.id">
        <slot name="item" v-bind="item" />
      </template>
      <!-- <CharacterInventoryItemCard
        v-for="userItem in userItems"
        :key="userItem.id"
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
      /> -->
    </div>

    <div
      class="
        sticky bottom-4 left-0 z-10 flex w-full justify-center rounded-lg bg-elevated p-4
        backdrop-blur-lg
      "
      style="grid-area: footer"
    >
      <!-- <i18n-t
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
                  <AppCoin :value="totalItemsCost" />
                </template>
              </i18n-t>
            </template>
          </i18n-t> -->
    </div>
  </div>
</template>

<style lang="css">
.itemGrid {
  grid-template-areas:
    '...... sort'
    'filter items'
    'filter footer';
  grid-template-columns: auto 1fr;
  grid-template-rows: auto 1fr auto;
}
</style>

<script setup lang="ts">
import type { TableColumn, TabsItem } from '@nuxt/ui'
import type { ColumnFiltersState, PaginationState, VisibilityState } from '@tanstack/vue-table'

import { getFacetedRowModel, getFacetedUniqueValues, getPaginationRowModel } from '@tanstack/vue-table'
import { UContainer } from '#components'
import { pick } from 'es-toolkit'

import type { WeaponClass } from '~/models/item'

import { type Item, type ItemFlat, ItemType } from '~/models/item'
import { SomeRole } from '~/models/role'
import { aggregationsConfig, aggregationsKeysByItemType, aggregationsKeysByWeaponClass } from '~/services/item-search-service/aggregations'
import { createItemIndex } from '~/services/item-search-service/indexator'
import { getItems, getWeaponClassesByItemType, hasWeaponClassesByItemType, itemTypeToIcon, weaponClassToIcon } from '~/services/item-service'

// import type { ItemFlat } from '~/models/item'

// import { useItemsCompare } from '~/composables/shop/use-compare'
// import { useItemsFilter } from '~/composables/shop/use-filters'
// import { useItemsSort } from '~/composables/shop/use-sort'
// import { usePagination } from '~/composables/use-pagination'
// import { useSearchDebounced } from '~/composables/use-search-debounce'
// import { useAsyncCallback } from '~/composables/utils/use-async-callback'
// import { WeaponUsage } from '~/models/item'
// import { getSearchResult } from '~/services/item-search-service'
// import {
//   canUpgrade,
//   getCompareItemsResult,
//   getItems,
//   itemIsNewDays,
// } from '~/services/item-service'
// import { notify } from '~/services/notification-service'
// import { t } from '~/services/translate-service'
// import { useUserStore } from '~/stores/user'

definePageMeta({
  roles: SomeRole,
  // layoutOptions:{
  //   noStickyHeader: true,
  // }
})

const userStore = useUserStore()
const router = useRouter()
const route = useRoute()

const {
  execute: loadItems,
  state: items,
} = useAsyncState(() => getItems(), [], {
  immediate: false,
  onSuccess: (data) => {
    // insertMultiple(db, data)
  },
})

const flatItems = computed((): ItemFlat[] => createItemIndex(items.value, true))

// TODO: FIXME: no await
Promise.all([loadItems(), userStore.fetchUserItems()])

// const userItemIds = computed(() => userStore.userItems.map(ui => ui.item.id))

// const getInInventoryItems = (baseId: string) => {
//   return userStore.userItems.filter(ui => ui.item.baseId === baseId)
// }

const table = useTemplateRef('table')

const { t } = useI18n()

// FIXME: ед. точка ответственности
const columnFilters = ref<ColumnFiltersState>([
  {
    id: 'type',
    value: ItemType.OneHandedWeapon,
  },
])

// const itemType = useRouteQuery<ItemType>('itemType', ItemType.OneHandedWeapon)
// TODO: use useRouteQuery
const itemType = computed({
  get() {
    return (route.query?.type as ItemType) || ItemType.OneHandedWeapon
  },
  set(type: ItemType) {
    const weaponClasses = getWeaponClassesByItemType(type)
    router.replace({
      query: {
        type,
        ...(weaponClasses.length !== 0 && { weaponClass: weaponClasses[0] }),
        // TODO:
        // ...pick(route.query, ['hideOwnedItems']),
      },
    })
    table.value?.tableApi?.getColumn('type')?.setFilterValue(type)
  },
})

// const weaponClass = useRouteQuery<WeaponClass | null>(
//   'weaponClass',
//   () => getWeaponClassesByItemType(itemType.value)?.[0] ?? null,
// )
const weaponClass = computed({
  get() {
    if (route.query?.weaponClass) {
      return route.query.weaponClass as WeaponClass
    }
    const weaponClasses = getWeaponClassesByItemType(itemType.value)
    return weaponClasses.length !== 0 ? weaponClasses[0] : null
  },
  set(weaponClass: WeaponClass | null) {
    router.replace({
      query: {
        type: itemType.value,
        weaponClass: weaponClass ?? undefined,
        // ...pick(route.query, ['hideOwnedItems']),
      },
    })
    table.value?.tableApi?.getColumn('weaponClass')?.setFilterValue(weaponClass)
  },
})

function getInitialPaginationState(): PaginationState {
  return {
    pageIndex: 0,
    pageSize: 10, // TODO: FIXME:
  }
}

const pagination = ref<PaginationState>(getInitialPaginationState())

const globalFilter = ref('')

const columns = computed<TableColumn<ItemFlat>[]>(() => {
  return [
    // {
    //   accessorKey: 'type',
    // },
    // {
    //   accessorKey: 'weaponClass',
    // },
    // {
    //   accessorKey: 'name',
    // },
    // {
    //   accessorKey: 'culture',
    // },
    ...Object.keys(aggregationsConfig).map(key => ({
      accessorKey: key,
    })),
  ]
})

// const c = computed()
const columnVisibility = computed<VisibilityState>(() => {
  const visibleKeys = [
    ...aggregationsKeysByItemType[itemType.value] ?? [],
    ...(weaponClass.value ? (aggregationsKeysByWeaponClass[weaponClass.value] ?? []) : []),
  ]
  return {
    ...Object.keys(aggregationsConfig).reduce((out, key) => {
      out[key] = visibleKeys.includes(key as keyof ItemFlat)
      return out
    }, {} as VisibilityState),
    // ...Object.keys(aggregationsKeysByItemType[itemType.value])?.reduce((out, key) => {
    //   out[key] = ture
    //   return out
    // }, {}),
    // ...Object.keys(aggregationsConfig)
    // ...aggregationsKeysByItemType
    // // culture: true,
    // ...(itemType.value === ItemType.Shield && {
    //   culture: false,
    // }),
    // ...hiddenRestrictedUser && { restrictedUser_name: false }
  }
})

// preFacet
const allTypes = computed<ItemType[]>(() => {
  // const rows = table.value?.tableApi.getPreFilteredRowModel().rows

  // if (!rows) {
  //   return []
  // }

  const set = new Set<ItemType>()

  for (const { type } of flatItems.value) {
    // const value = row.getValue('type')
    // if (value) {
    set.add(type)
    // }
  }

  return Array.from(set)
})
</script>

<template>
  <UContainer class="max-w-full py-6">
    <!-- {{ allTypes }} -->
    {{ { itemType } }}
    {{ { weaponClass } }}
    <UTabs
      v-model="itemType"
      :items="(allTypes)
        .map<TabsItem>((type) => ({
          icon: `crpg:${itemTypeToIcon[type]}`,
          value: type,
        }))"
      :content="false"
    >
      <template #default="{ item }">
        <UTabs
          v-if="hasWeaponClassesByItemType(itemType) && item.value === itemType && weaponClass"
          v-model="weaponClass"
          :items="([...table?.tableApi.getColumn('weaponClass')?.getFacetedUniqueValues().keys() || []] as WeaponClass[])
            // .filter(weaponClass => getWeaponClassesByItemType(itemType).includes(weaponClass))
            .map<TabsItem>((weaponClass) => ({
              icon: `crpg:${weaponClassToIcon[weaponClass]}`,
              value: weaponClass,
            }))"
          :content="false"
        />
      </template>
    </UTabs>

    <!-- TODO: FIXME: dynamic columns, or visible columns -->
    <!-- :key="`${itemType}_${weaponClass}`" -->
    <UTable
      ref="table"
      v-model:pagination="pagination"
      v-model:column-filters="columnFilters"
      v-model:column-visibility="columnVisibility"
      class="relative rounded-md border border-muted"
      :data="flatItems"
      :columns
      :initial-state="{
        pagination: getInitialPaginationState(),
      }"
      :faceted-options="{
        getFacetedRowModel: getFacetedRowModel(),
        getFacetedUniqueValues: getFacetedUniqueValues(),
      }"
      :pagination-options="{
        getPaginationRowModel: getPaginationRowModel(),
      }"
    >
      <template #empty>
        <UiResultNotFound />
      </template>
    </UTable>

    <UPagination
      v-if="table?.tableApi.getCanNextPage() || table?.tableApi.getCanPreviousPage()"
      class="flex justify-center"
      variant="soft"
      color="secondary"
      active-variant="solid"
      active-color="primary"
      :page="pagination.pageIndex + 1"
      :show-controls="false"
      :default-page="(table?.tableApi?.initialState.pagination.pageIndex || 0) + 1"
      :items-per-page="table?.tableApi?.initialState.pagination.pageSize"
      :total="table?.tableApi?.getFilteredRowModel().rows.length"
      @update:page="(p) => table?.tableApi?.setPageIndex(p - 1)"
    />
    <!-- <div class="mb-2 flex items-center gap-6 overflow-x-auto pb-2">
      <VDropdown
        :triggers="['click']"
        placement="bottom-end"
      >
        <MoreOptionsDropdownButton
          :active="
            hideOwnedItemsModel
              || Boolean('weaponUsage' in filterModel && filterModel.weaponUsage!.length > 1)
              || Boolean('new' in filterModel && filterModel.new!.length)
          "
        />

        <template #popper="{ hide }">
          <DropdownItem>
            <Tooltip
              :title="$t('item.aggregations.new.title')"
              :description="$t('item.aggregations.new.description', { days: itemIsNewDays })"
            >
              <OCheckbox
                :native-value="1"
                :model-value="filterModel.new"
                :disabled="newItemCount === 0"
                @update:model-value="(val) => updateFilter('new', val)"
                @change="hide"
              >
                {{ $t('item.aggregations.new.title') }}
                ({{ newItemCount }})
              </OCheckbox>
            </Tooltip>
          </DropdownItem>

          <DropdownItem>
            <OCheckbox
              v-model="hideOwnedItemsModel"
              @change="hide"
            >
              {{ $t('shop.hideOwnedItems.title') }}
            </OCheckbox>
          </DropdownItem>

          <DropdownItem v-if="'weaponUsage' in filterModel">
            <Tooltip
              :title="$t('shop.nonPrimaryWeaponMode.tooltip.title')"
              :description="$t('shop.nonPrimaryWeaponMode.tooltip.desc')"
            >
              <OCheckbox
                :native-value="WeaponUsage.Secondary"
                :model-value="filterModel.weaponUsage"
                @update:model-value="(val: string) => updateFilter('weaponUsage', val)"
                @change="hide"
              >
                {{ $t('shop.nonPrimaryWeaponMode.title') }}
              </OCheckbox>
            </Tooltip>
          </DropdownItem>
        </template>
      </VDropdown>

      <div class="h-8 w-px select-none bg-border-200" />

      <ShopItemTypeSelect
        v-model:item-type="itemTypeModel"
        v-model:weapon-class="weaponClassModel"
        :item-type-buckets="aggregationByType.data.buckets"
        :weapon-class-buckets="aggregationByClass.data.buckets"
      />
    </div> -->

    <!-- <OTable
      v-model:current-page="pageModel"
      v-model:checked-rows="compareList"
      :data="searchResult.data.items"
      bordered
      narrowed
      hoverable
      sort-icon="chevron-up"
      sort-icon-size="xs"
      sticky-header
      :detailed="isUpgradableCategory"
      detail-key="id"
      custom-row-key="id"
      :loading="userStore.buyingItem"
    >
      <OTableColumn
        field="compare"
        :width="36"
      >
        <template #header>
          <span class="inline-flex items-center">
            <OCheckbox
              v-tooltip="
                compareList.length ? $t('shop.compare.removeAll') : $t('shop.compare.addAll')
              "
              :model-value="compareList.length >= 1"
              :native-value="true"
              @update:model-value="
                () =>
                  compareList.length
                    ? removeAllFromCompareList()
                    : addAllToCompareList(searchResult.data.items.map(item => item.modId))
              "
            />
          </span>
        </template>
        <template #default="{ row: item }: { row: ItemFlat }">
          <span class="inline-flex items-center">
            <OCheckbox
              v-tooltip="
                compareList.includes(item.modId)
                  ? $t('shop.compare.remove')
                  : $t('shop.compare.add')
              "
              :model-value="compareList.includes(item.modId)"
              :native-value="true"
              @update:model-value="() => toggleToCompareList(item.modId)"
            />
          </span>
        </template>
      </OTableColumn>

      <OTableColumn field="name">
        <template #header>
          <div class="max-w-[220px]">
            <OInput
              v-model="searchModel"
              type="text"
              :placeholder="$t('action.search')"
              icon="search"
              rounded
              expanded
              clearable
              size="sm"
              icon-right-clickable
              data-aq-search-shop-input
            />
          </div>
        </template>

        <template #default="{ row: item }: { row: ItemFlat }">
          <ShopGridItemName
            :item="item"
            show-tier
          />
        </template>
      </OTableColumn>

      <OTableColumn
        v-for="(field, idx) in (Object.keys(aggregationsConfigVisible) as Array<keyof ItemFlat>)"
        :key="idx"
        :field="field"
        :width="aggregationsConfigVisible[field]?.width ?? 140"
      >
        <template #header>
          <ShopGridFilter
            v-if="field in searchResult.data.aggregations"
            v-model:sorting="sortingModel"
            :scope-aggregation="scopeAggregations[field]"
            :aggregation="searchResult.data.aggregations[field]"
            :aggregation-config="aggregationsConfig[field]!"
            :filter="filterModel[field]!"
            :sorting-config="getSortingConfigByField(field)"
            @update:filter="val => updateFilter(field, val)"
          />
        </template>

        <template #default="{ row: item }: { row: ItemFlat }">
          <ItemParam
            :item="item"
            :field="field"
            :best-value="compareItemsResult !== null ? compareItemsResult[field] : undefined"
            :is-compare="isCompare"
          >
            <template
              v-if="field === 'upkeep'"
              #default="{ rawBuckets }"
            >
              <Coin>
                {{ $t('item.format.upkeep', { upkeep: $n(rawBuckets as number) }) }}
              </Coin>
            </template>

            <template
              v-else-if="field === 'price'"
              #default="{ rawBuckets }"
            >
              <ShopGridItemBuyBtn
                :price="(rawBuckets as number)"
                :upkeep="item.upkeep"
                :in-inventory-items="getInInventoryItems(item.baseId)"
                :not-enough-gold="user!.gold < item.price"
                @buy="buyItem(item)"
              />
            </template>
          </ItemParam>
        </template>
      </OTableColumn>

      <template #detail="{ row: item }: { row: ItemFlat }">
        <ShopGridUpgradesTable
          :item="item"
          :cols="aggregationsConfigVisible"
        />
      </template>

      <template #empty>
        <ResultNotFound />
      </template>

      <template #footer>
        <div class="space-y-4 bg-base-100 py-4 pr-2 backdrop-blur-sm">
          <div class="grid h-14 grid-cols-3 items-center gap-6">
            <Pagination
              v-model="pageModel"
              :total="searchResult.pagination.total"
              :per-page="searchResult.pagination.per_page"
              order="left"
              with-input
            />

            <div class="flex justify-center">
              <OButton
                v-if="compareList.length >= 2"
                variant="primary"
                size="lg"
                outlined
                :icon-right="isCompare ? 'close' : ''"
                data-aq-shop-handler="toggle-compare"
                :label="$t('shop.compare.title')"
                @click="toggleCompare"
              />
            </div>

            <div class="flex items-center justify-end gap-4">
              <div class="text-content-400">
                {{ $t('shop.pagination.perPage') }}
              </div>
              <OTabs
                v-model="perPageModel"
                size="xl"
                type="bordered-rounded"
                content-class="hidden"
              >
                <OTabItem
                  v-for="pp in perPageConfig"
                  :key="pp"
                  :label="String(pp)"
                  :value="pp"
                />
              </OTabs>
            </div>
          </div>
        </div>
      </template>
    </OTable> -->
  </UContainer>
</template>

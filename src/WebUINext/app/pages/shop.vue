<script setup lang="ts">
import type { TableColumn, TabsItem } from '@nuxt/ui'
import type {
  ColumnFiltersState,
  PaginationState,
  RowSelectionState,
  SortingState,
  VisibilityState,
} from '@tanstack/vue-table'

import {
  getFacetedRowModel,
  getFacetedUniqueValues,
  getPaginationRowModel,
} from '@tanstack/vue-table'
import {
  AppCoin,
  ItemParam,
  ShopGridItemBuyBtn,
  ShopGridItemMedia,
  UButton,
  UCheckbox,
  UContainer,
  // UInput,
  UiTableColumnHeader,
} from '#components'
import { h } from '#imports'
import { uniq } from 'es-toolkit'

import type { ItemFlat } from '~/models/item'
import type { AggregationConfig, AggregationOptions } from '~/services/item-search-service/aggregations'

// import { useItemsCompare } from '~/composables/shop/use-compare'
// import { useItemsFilter } from '~/composables/shop/use-filters'
// import { useItemsSort } from '~/composables/shop/use-sort'
// import { usePagination } from '~/composables/use-pagination'
// import { useSearchDebounced } from '~/composables/use-search-debounce'
import { useAsyncCallback } from '~/composables/utils/use-async-callback'
import { usePageLoading } from '~/composables/utils/use-page-loading'
import { ItemType, WeaponClass } from '~/models/item'
import { SomeRole } from '~/models/role'
import {
  aggregationsConfig,
  aggregationsKeysByItemType,
  aggregationsKeysByWeaponClass,
  AggregationView,
} from '~/services/item-search-service/aggregations'
import { createItemIndex } from '~/services/item-search-service/indexator'
// import type { ItemFlat } from '~/models/item'
import {
  canUpgrade,
  getItems,
  getWeaponClassesByItemType,
  hasWeaponClassesByItemType,
  itemTypeToIcon,
  weaponClassToIcon,
} from '~/services/item-service'
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
  layoutOptions: {
    noStickyHeader: true,
  },
})
const UInput = resolveComponent('UInput')
const { t, n } = useI18n()
const toast = useToast()

const userStore = useUserStore()
const router = useRouter()
const route = useRoute()

const {
  state: items,
  execute: loadItems,
  isLoading: loadingItems,
} = useAsyncState(() => getItems(), [], {
  immediate: false,
})

const {
  execute: buyItem,
} = useAsyncCallback(async (item: ItemFlat) => {
  await userStore.buyItem(item.id)
  toast.add({
    title: t('shop.item.buy.notify.success'),
    close: false,
    color: 'success',
  })
})

Promise.all([loadItems(), userStore.fetchUserItems()])

const flatItems = computed((): ItemFlat[] => createItemIndex(items.value, true))

const getInInventoryItems = (baseId: string) => {
  return userStore.userItems.filter(ui => ui.item.baseId === baseId)
}

function getInitialPaginationState(): PaginationState {
  return {
    pageIndex: 0,
    pageSize: 10, // TODO: FIXME:
  }
}

const pagination = ref<PaginationState>(getInitialPaginationState())

function getInitialSortingState(): SortingState {
  return [
    {
      id: 'price',
      desc: true,
    },
  ]
}
const sorting = ref<SortingState>(getInitialSortingState())

const columnVisibility = computed<VisibilityState>(() => {
  return {
    // ...Object.keys(aggregationsConfig).reduce((out, key) => {
    //   out[key] = visibleAggregationKeys.value.includes(key as keyof ItemFlat)
    //   return out
    // }, {} as VisibilityState),
    type: false,
    weaponClass: false,
    modId: false,
    weaponUsage: false,
  }
})

const table = useTemplateRef('table')

// const itemType = useRouteQuery<ItemType>('itemType', ItemType.OneHandedWeapon)
const itemType = computed({
  get() {
    return (route.query?.type as ItemType) || ItemType.OneHandedWeapon
  },
  set(type: ItemType) {
    const [weaponClass] = getWeaponClassesByItemType(type)
    table.value?.tableApi.setColumnFilters([
      { id: 'type', value: type },
      { id: 'weaponClass', value: weaponClass },
    ])
    router.replace({
      query: {
        type,
        weaponClass,
      },
    })
    table.value?.tableApi.setPageIndex(0)
  },
})

// const weaponClass = useRouteQuery<WeaponClass | null>('weaponClass', () => getWeaponClassesByItemType(itemType.value)?.[0] ?? null)
const weaponClass = computed({
  get() {
    if (route.query?.weaponClass) {
      return route.query.weaponClass as WeaponClass
    }
    const weaponClasses = getWeaponClassesByItemType(itemType.value)
    return weaponClasses.length !== 0 ? weaponClasses[0] : null
  },
  set(weaponClass: WeaponClass | null) {
    table.value?.tableApi?.getColumn('weaponClass')?.setFilterValue(weaponClass ?? undefined)
    router.replace({
      query: {
        type: itemType.value,
        weaponClass: weaponClass ?? undefined,
      },
    })
    table.value?.tableApi.setPageIndex(0)
  },
})

function getInitialColumnFiltersState(): ColumnFiltersState {
  return [
    { id: 'type', value: itemType.value },
    ...(weaponClass.value ? [{ id: 'weaponClass', value: weaponClass.value }] : []),
  ]
}

const columnFilters = ref<ColumnFiltersState>(getInitialColumnFiltersState())

// TODO: FIXME:
const isUpgradableItemType = computed(() => canUpgrade(itemType.value))

const visibleAggregationKeys = computed(() => {
  if (weaponClass.value && weaponClass.value in aggregationsKeysByWeaponClass) {
    return aggregationsKeysByWeaponClass[weaponClass.value]!
  }
  return aggregationsKeysByItemType[itemType.value] || []
})

const currentAggregations = computed<AggregationConfig>(
  () => Object.fromEntries(
    Object.entries(aggregationsConfig)
      .sort(([a], [b]) => {
        const indexA = visibleAggregationKeys.value.indexOf(a as keyof ItemFlat)
        const indexB = visibleAggregationKeys.value.indexOf(b as keyof ItemFlat)
        return indexA === -1 ? (indexB === -1 ? 0 : 1) : (indexB === -1 ? -1 : indexA - indexB)
      }),
  ),
)

const rowSelection = ref<RowSelectionState>({})
const [isCompareMode, toggleCompareMode] = useToggle()
watch(isCompareMode, () => {
  table.value?.tableApi.getColumn('modId')?.setFilterValue(
    isCompareMode.value
      ? table.value?.tableApi.getSelectedRowModel().rows.map(row => row.original.modId)
      : undefined,
  )
})

function createTableColumn(key: keyof ItemFlat, options: AggregationOptions): TableColumn<ItemFlat> {
  return {
    accessorKey: key,
    cell: ({ row }) => h(ItemParam, {
      field: key,
      item: row.original,
      // bestValue: compareItemsResult !== null ? compareItemsResult[field] : undefined,
      // isCompare: isCompareMode.value
    }, {
      ...(key === 'upkeep' && {
        default: ({ rawBuckets }: { rawBuckets: number }) => h(AppCoin, null, {
          default: () => t('item.format.upkeep', { upkeep: n(rawBuckets) }),
        }),
      }),
      ...(key === 'price' && {
        default: ({ rawBuckets }: { rawBuckets: number }) => h(ShopGridItemBuyBtn, {
          price: rawBuckets,
          upkeep: row.original.upkeep,
          inInventoryItems: getInInventoryItems(row.original.baseId),
          notEnoughGold: userStore.user!.gold < row.original.price,
          onBuy: () => buyItem(row.original),
        }),
      }),
    }),
    meta: {
      class: {
        td: 'min-w-[140px]',
        th: 'min-w-[140px]',
      },
    },
    header: ({ header, column }) => {
      const description = t(`item.aggregations.${header.id}.description`)
      return h(UiTableColumnHeader, {
        label: t(`item.aggregations.${header.id}.title`),
        withSort: options.view === AggregationView.Range,
        sorted: column.getIsSorted(),
        onSort: () => column.toggleSorting(column.getIsSorted() === 'asc'),
      })
      // return h(UTooltip, {
      //   delayDuration: 600,
      //   disabled: !description,
      //   ui: {
      //     content: 'max-w-sm',
      //   },
      // }, {
      //   default: () => h('div', null, t(`item.aggregations.${header.id}.title`)),
      //   content: () => h('div', { class: 'prose prose-invert' }, [
      //     h('h4', null, t(`item.aggregations.${header.id}.title`)),
      //     h('div', { innerHTML: description }),
      //   ]),
      // })
    },
  }
}

const columns = computed<TableColumn<ItemFlat>[]>(() => {
  return [
    {
      id: 'expand',
      meta: {
        class: {
          th: 'px-1 w-[50px]',
          td: 'px-1 w-[50px]',
        },
      },
      cell: ({ row }) =>
        h(UButton, {
          color: 'neutral',
          variant: 'ghost',
          icon: 'crpg:chevron-down',
          square: true,
          // 'aria-label': 'Expand',
          ui: {
            leadingIcon: [
              'transition-transform',
              row.getIsExpanded() ? 'duration-200 rotate-180' : '',
            ],
          },
          onClick: () => row.toggleExpanded(),
        }),
    },
    {
      id: 'select',
      header: ({ table }) => h(UCheckbox, {
        'modelValue': table.getIsSomePageRowsSelected()
          ? 'indeterminate'
          : table.getIsAllPageRowsSelected(),
        'onUpdate:modelValue': (value: boolean | 'indeterminate') =>
          table.toggleAllPageRowsSelected(!!value),
        // 'aria-label': 'Select all',
        'size': 'xl',
      }),
      cell: ({ row }) => h(UCheckbox, {
        'modelValue': row.getIsSelected(),
        'onUpdate:modelValue': (value: boolean | 'indeterminate') => row.toggleSelected(!!value),
        // 'aria-label': 'Select row',
        'size': 'xl',
      }),
      meta: {
        class: {
          th: 'px-1 w-[40px]',
          td: 'px-1 w-[40px]',
        },
      },
    },
    {
      accessorKey: 'name',
      header: ({ column }) => h(UInput, {
        'icon': 'crpg:search',
        'variant': 'soft',
        'size': 'xs',
        'placeholder': t('action.search'),
        'modelValue': column.getFilterValue(),
        'onUpdate:modelValue': (value: string) => {
          table.value?.tableApi.setPageIndex(0)
          column.setFilterValue(value)
        },
      }),
      cell: ({ row }) => h(ShopGridItemMedia, {
        item: row.original,
        showTier: true,
      }),
      meta: {
        class: {
          td: 'max-w-[320px]',
          th: 'max-w-[320px]',
        },
      },
    },
    ...Object.entries(currentAggregations.value)
      .filter(([key]) => ['type', 'weaponClass'].includes(key) || visibleAggregationKeys.value.includes(key as keyof ItemFlat))
      .map(([key, config]) => createTableColumn(key as keyof ItemFlat, config)),
  ]
})

const { togglePageLoading } = usePageLoading()

watchEffect(() => {
  togglePageLoading(loadingItems.value)
})

const itemTypeOptions = computed(() => {
  const orders = Object.values(ItemType)
  return uniq(flatItems.value.map(({ type }) => type))
    .sort((a, b) => orders.indexOf(a) - orders.indexOf(b))
    .map<TabsItem>(type => ({
      icon: `crpg:${itemTypeToIcon[type]}`,
      value: type,
    }))
})

const weaponClassOptions = computed(() => {
  const orders = Object.values(WeaponClass)
  return uniq(flatItems.value.map<WeaponClass>(({ weaponClass }) => weaponClass as WeaponClass))
    .filter(weaponClass => getWeaponClassesByItemType(itemType.value).includes(weaponClass))
    .sort((a, b) => orders.indexOf(a) - orders.indexOf(b))
    .map<TabsItem>(weaponClass => ({
      icon: `crpg:${weaponClassToIcon[weaponClass]}`,
      value: weaponClass,
    }))
})
</script>

<template>
  <UContainer class="max-w-full space-y-6 py-3">
    <!-- <pre>visibleAggregationKeys: {{ visibleAggregationKeys }}</pre> -->
    <!-- {{ allTypes }} -->
    <!-- <pre>{{ { itemType, weaponClass } }}</pre> -->
    <!-- <pre>{{ columnFilters }}</pre> -->
    <!-- <pre>{{ table?.tableApi.getState().columnFilters }}</pre> -->
    <!-- <pre>
      {{ table?.tableApi.getState() }}
    </pre> -->

    <UTabs
      v-model="itemType"
      :items="itemTypeOptions"
      :content="false"
      size="xl"
      :ui="{
        list: 'w-auto',
        root: 'flex-row',
        trigger: 'min-w-16 h-16',
        leadingIcon: 'size-7',
      }"
    >
      <template v-if="hasWeaponClassesByItemType(itemType)" #default="{ item }">
        <UTabs
          v-if="item.value === itemType && weaponClass"
          v-model="weaponClass"
          :items="weaponClassOptions"
          :content="false"
          size="xl"
          :ui="{
            list: 'w-auto',
            root: 'flex-row',
            leadingIcon: 'size-7',
          }"
        />
      </template>
    </UTabs>

    <!-- TODO: FIXME: dynamic columns, or visible columns -->
    <!-- :key="`${itemType}_${weaponClass}`" -->
    <!-- {{ columns.map((i) => i.accessorKey || i.id) }} -->
    <!--  -->

    <UTable
      ref="table"
      v-model:pagination="pagination"
      v-model:sorting="sorting"
      v-model:column-filters="columnFilters"
      v-model:column-visibility="columnVisibility"
      v-model:row-selection="rowSelection"
      class="relative overflow-clip rounded-md border border-muted"
      :data="flatItems"
      :columns
      sticky
      :loading="loadingItems"
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

      <template #expanded="{ row }">
        <ShopGridUpgradesTable
          :aggregation-config="Object.fromEntries(Object.entries(currentAggregations).filter(([key]) => visibleAggregationKeys.includes(key as keyof ItemFlat)))"
          :item="row.original"
        />
      </template>
    </UTable>

    <div
      class="
        sticky bottom-0 left-0 z-[1] grid grid-cols-3 items-center gap-6
        bg-default/75 py-4 backdrop-blur
      "
    >
      <UPagination
        v-if="table?.tableApi.getCanNextPage() || table?.tableApi.getCanPreviousPage()"
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

      <div class="flex justify-center">
        <UButton
          v-if="Object.keys(rowSelection).length >= 2"
          size="lg"
          variant="subtle"
          :icon="isCompareMode ? 'crpg:close' : undefined"
          data-aq-shop-handler="toggle-compare"
          :label="$t('shop.compare.title')"
          @click="() => { toggleCompareMode() }"
        />
      </div>
    </div>
    <!--
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
         -->
  </UContainer>
</template>

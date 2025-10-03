<script setup lang="ts">
import type { SelectItem, TableColumn } from '@nuxt/ui'
import type { ColumnFiltersState, RowSelectionState, SortingState } from '@tanstack/vue-table'

import {
  filterFns,
  getFacetedMinMaxValues,
  getFacetedRowModel,
  getFacetedUniqueValues,
  getPaginationRowModel,
} from '@tanstack/vue-table'
import {
  AppCoin,
  ItemParam,
  ItemTableMedia,
  ShopGridItemBuyBtn,
  UButton,
  UCheckbox,
  UContainer,
  UiGridColumnHeader,
  UiGridColumnHeaderLabel,
  UiInputRange,
  UInput,
  USelect,
  UTooltip,
} from '#components'
import { h } from '#imports'

import type { ItemFlat, ItemType, WeaponClass } from '~/models/item'
import type { AggregationOptions } from '~/services/item-search-service/aggregations'

import { usePageLoading } from '~/composables/app/use-page-loading'
import { useUserItemsProvider } from '~/composables/user/use-user-items'
import { useAsyncCallback } from '~/composables/utils/use-async-callback'
import { ITEM_COMPARE_MODE, ITEM_TYPE } from '~/models/item'
import { SomeRole } from '~/models/role'
import { getAggregationsConfig, getBuckets, getColumnVisibility, getFacetsByItemType, getFacetsByWeaponClass, getFilterFn } from '~/services/item-search-service'
import { AGGREGATION_VIEW } from '~/services/item-search-service/aggregations'
import { getStepRange } from '~/services/item-search-service/helpers'
import { createItemIndex } from '~/services/item-search-service/indexator'
import {
  canUpgradeItem,
  getCompareItemsResult,
  getItems,
  getItemUpgrades,
  getRelativeEntries,
  getWeaponClassesByItemType,
  humanizeBucket,
} from '~/services/item-service'
import { buyUserItem } from '~/services/user-service'

definePageMeta({
  roles: SomeRole,
  layoutOptions: {
    noStickyHeader: true,
  },
})

const { t, n } = useI18n()

const router = useRouter()
const route = useRoute()
const userStore = useUserStore()

const { data: userItems, refresh: refreshUserItems } = useUserItemsProvider()

const {
  state: items,
  isLoading: loadingItems,
} = useAsyncState(() => getItems(), [])
usePageLoading(loadingItems)

const flatItems = ref<ItemFlat[]>([])
watch(items, () => {
  flatItems.value = createItemIndex(items.value, false)
})

const [buyItem] = useAsyncCallback(async (item: ItemFlat) => {
  await buyUserItem(item.id)
  await refreshUserItems()
  await userStore.fetchUser() // update gold
}, {
  successMessage: t('shop.item.buy.notify.success'),
  pageLoading: true,
})

const getInInventoryItems = (baseId: string) => {
  return userItems.value.filter(ui => ui.item.baseId === baseId)
}

const table = useTemplateRef('table')

const { pagination } = usePagination()

function resetPagination() {
  table.value?.tableApi.setPageIndex(0)
}

function getInitialSortingState(): SortingState {
  return [
    {
      id: 'price',
      desc: true,
    },
  ]
}
const sorting = ref<SortingState>(getInitialSortingState())
function resetSorting() {
  table.value?.tableApi.setSorting(getInitialSortingState())
}

const itemType = computed({
  get() {
    return (route.query?.type as ItemType) || ITEM_TYPE.OneHandedWeapon
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
    resetPagination()
    resetSorting()
    resetSelection()
    resetCompareMode()
  },
})

const itemTypes = computed(() => getFacetsByItemType(flatItems.value.map(item => item.type)))

const isUpgradableItemType = computed(() => canUpgradeItem(itemType.value))

const weaponClass = computed({
  get() {
    if (route.query?.weaponClass) {
      return route.query.weaponClass as WeaponClass
    }
    return getWeaponClassesByItemType(itemType.value).at(0) ?? null
  },
  set(weaponClass: WeaponClass | null) {
    table.value?.tableApi.setColumnFilters([
      { id: 'weaponClass', value: weaponClass },
    ])
    router.replace({
      query: {
        type: itemType.value,
        weaponClass: weaponClass ?? undefined,
      },
    })
    resetPagination()
    resetSorting()
    resetSelection()
    resetCompareMode()
  },
})

const weaponClasses = computed(() => getFacetsByWeaponClass(flatItems.value
  .map(item => item.weaponClass)
  .filter(item => item !== null), itemType.value))

function getInitialColumnFiltersState(): ColumnFiltersState {
  return [
    { id: 'type', value: itemType.value },
    // { id: 'weaponUsage', value: [WEAPON_USAGE.Primary] }, // TODO: FIXME:
    ...(weaponClass.value ? [{ id: 'weaponClass', value: weaponClass.value }] : []),
  ]
}
const columnFilters = ref<ColumnFiltersState>(getInitialColumnFiltersState())

const currentAggregations = computed(() => getAggregationsConfig(itemType.value, weaponClass.value))

const columnVisibility = computed(() => getColumnVisibility(currentAggregations.value))

const rowSelection = ref<RowSelectionState>({})
function resetSelection() {
  table.value?.tableApi.resetRowSelection()
}
const [isCompareMode, toggleCompareMode] = useToggle()
function resetCompareMode() {
  toggleCompareMode(false)
}
watch(isCompareMode, () => {
  table.value?.tableApi.getColumn('modId')?.setFilterValue(
    isCompareMode.value
      ? table.value?.tableApi.getSelectedRowModel().rows.map(row => row.original.modId)
      : undefined,
  )
})

const compareItems = computed(() => isCompareMode.value && table.value?.tableApi
  ? getCompareItemsResult(table.value.tableApi.getFilteredRowModel().rows.map(row => row.original), currentAggregations.value)
  : null)

const compareUpgrades = computed(() => {
  const expandedRows = table.value?.tableApi.getState().expanded
  if (!expandedRows) {
    return {}
  }
  return Object.fromEntries(
    Object.entries(expandedRows)
      .map(([rowId]) => {
        const row = table.value!.tableApi.getRow(rowId).original
        return [
          row.baseId,
          getRelativeEntries(row, currentAggregations.value),
        ]
      }),
  )
})

function createTableColumn(key: keyof ItemFlat, options: AggregationOptions): TableColumn<ItemFlat> {
  return {
    accessorKey: key,
    cell: ({ row }) => {
      return h(ItemParam, {
        field: key,
        item: row.original,
        ...(row.depth === 1
          ? {
              isCompare: true,
              compareMode: ITEM_COMPARE_MODE.Relative,
              relativeValue: compareUpgrades.value[row.getParentRow()!.original.baseId]?.[key],
            }
          : {
              isCompare: isCompareMode.value,
              bestValue: compareItems.value !== null ? compareItems.value[key] : undefined,
            }),
      }, {
        ...(key === 'upkeep' && { default: ({ rawBuckets }: { rawBuckets: number }) => h(AppCoin, { value: t('item.format.upkeep', { upkeep: n(rawBuckets) }) }) }),
        ...(key === 'price' && {
          default: ({ rawBuckets }: { rawBuckets: number }) => h(ShopGridItemBuyBtn, {
            price: rawBuckets,
            upkeep: row.original.upkeep,
            inInventoryItems: getInInventoryItems(row.original.baseId),
            notEnoughGold: userStore.user!.gold < row.original.price,
            onBuy: () => buyItem(row.original),
          }),
        }),
      })
    },
    ...(['upkeep', 'price'].includes(key) && {
      meta: {
        class: {
          th: 'w-[180px]',
          td: 'w-[180px]',
        },
      },
    }),
    filterFn: getFilterFn(options),
    sortingFn: (rowA, rowB, columnId) => {
      // disable sort for upgrades items
      if (rowA.depth > 0 || rowB.depth > 0) {
        return 0
      }
      return rowA.getValue<number>(columnId) - rowB.getValue<number>(columnId)
    },
    header: ({ header, column }) => {
      return h(UiGridColumnHeader, {
        label: t(`item.aggregations.${header.id}.title`),
        description: t(`item.aggregations.${header.id}.description`),
        withSort: options.view === AGGREGATION_VIEW.Range,
        sorted: column.getIsSorted(),
        withFilter: true,
        filtered: column.getIsFiltered(),
        onSort: () => column.toggleSorting(column.getIsSorted() === 'asc'),
        onResetFilter: () => column.setFilterValue(undefined),
      }, {
        ...(options.view === AGGREGATION_VIEW.Range && {
          // TODO: FIXME: пересмотреть компонент
          'filter-content': () => {
            const [min, max] = column.getFacetedMinMaxValues() ?? [0, 1]
            const buckets = [...new Set([...column.getFacetedUniqueValues().keys()].flat())] as number[]
            return h(UiInputRange, {
              'min': min,
              'max': max,
              'step': getStepRange(buckets),
              'modelValue': column.getFilterValue() as [number, number],
              'onUpdate:modelValue': column.setFilterValue,
            })
          },
        }),
        filter: () => {
          if (options.view === AGGREGATION_VIEW.Checkbox) {
            const _buckets = Object.entries(getBuckets(column.getFacetedUniqueValues()))
            // @ts-expect-error TODO: https://github.com/nuxt/ui/issues/2968
            return h(USelect, {
              'class': 'w-full',
              'multiple': true,
              'variant': 'none',
              'size': 'xl',
              'trailing-icon': '', // TODO:
              'ui': {
                content: 'min-w-fit',
                base: 'px-0 py-0',
              },
              'items': _buckets.length
                ? _buckets.map<SelectItem>(([bucket, count]) => {
                    const humanBucket = humanizeBucket(column.id as keyof ItemFlat, bucket)
                    return {
                      value: bucket,
                      label: `${humanBucket.label}${count ? ` (${count})` : ''}`,
                      ...(humanBucket.icon && { icon: `crpg:${humanBucket.icon}` }),
                    }
                  })
                : [
                    {
                      label: t('not-found'),
                      icon: 'crpg:error',
                      disabled: true,
                    } satisfies SelectItem,
                  ],
              'modelValue': column.getFilterValue(),
              'onUpdate:modelValue': column.setFilterValue,
            }, {
              default: () => h(UiGridColumnHeaderLabel, {
                label: t(`item.aggregations.${header.id}.title`),
                withFilter: true,
              }),
            })
          }
          return undefined
        },
      })
    },
  }
}

const columns = computed<TableColumn<ItemFlat>[]>(() => {
  return [
    {
      id: 'expand',
      meta: {
        class: {
          th: 'px-0 w-[36px]',
          td: 'px-0 w-[36px]',
        },
      },
      cell: ({ row }) => {
        if (!row.getCanExpand()) {
          return null
        }
        const _isExpanded = row.getIsExpanded()
        return h('div', { class: 'w-[36px] flex justify-center' }, [
          h(UTooltip, { text: _isExpanded ? t('shop.upgrades.collapse') : t('shop.upgrades.expand') }, {
            default: () => h(UButton, {
              color: 'neutral',
              variant: 'ghost',
              icon: 'crpg:chevron-down',
              ui: {
                leadingIcon: ['transition-transform', _isExpanded ? 'duration-200 -rotate-90' : ''],
              },
              onClick: async () => {
                if (!row.original.children.length) {
                  row.original.children.push(...(await getItemUpgrades(row.original.baseId)).toSpliced(0, 1))
                }
                row.toggleExpanded()
              },
            }),
          }),
        ])
      },
    },
    {
      id: 'select',
      meta: {
        class: {
          th: 'px-0 w-[36px]',
          td: 'px-0 w-[36px]',
        },
      },
      header: ({ table }) => {
        const _isSomeSelected = table.getIsSomePageRowsSelected()
        const _isAllSelected = table.getIsAllPageRowsSelected()
        return h('div', { class: 'w-[36px] flex justify-center' }, [
          h(UTooltip, { text: _isAllSelected ? t('shop.compare.removeAll') : t('shop.compare.addAll') }, {
            default: () => h(UCheckbox, {
              'modelValue': _isSomeSelected ? 'indeterminate' : _isAllSelected,
              'onUpdate:modelValue': value => table.toggleAllPageRowsSelected(!!value),
              'size': 'xl',
            }),
          }),
        ])
      },
      cell: ({ row }) => {
        if (!row.getCanSelect()) {
          return null
        }
        const _isSelected = row.getIsSelected()
        return h('div', { class: 'w-[36px] flex justify-center' }, [
          h(UTooltip, { text: _isSelected ? t('shop.compare.remove') : t('shop.compare.add') }, {
            default: () => h(UCheckbox, {
              'modelValue': _isSelected,
              'onUpdate:modelValue': value => row.toggleSelected(!!value),
              'size': 'xl',
            }),
          }),
        ])
      },
    },
    {
      accessorKey: 'name',
      meta: {
        class: {
          th: 'w-[360px]',
          td: 'w-[360px]',
        },
      },
      header: ({ column }) => {
        const _column = table.value?.tableApi.getColumn('isNew')
        const _count = _column?.getFacetedUniqueValues().get(true)
        const _value = (_column?.getFilterValue() as boolean) || false

        return h('div', {
          class: tw`flex items-center gap-2`,
        }, [
          // @ts-expect-error TODO: https://github.com/nuxt/ui/issues/2968
          h(UInput, {
            'icon': 'crpg:search',
            'variant': 'soft',
            'size': 'xl',
            'placeholder': t('action.search'),
            'modelValue': column.getFilterValue(),
            'onUpdate:modelValue': column.setFilterValue,
          }),
          ...(_column && _count
            ? [
                h(UButton, {
                  label: `New (${_count})`,
                  color: 'success',
                  variant: 'soft',
                  activeVariant: 'solid',
                  active: _value,
                  onClick: () => {
                    _column.setFilterValue(!_value || undefined)
                  },
                }),
              ]
            : []),
        ])
      },
      cell: ({ row }) => h(ItemTableMedia, { item: row.original, showTier: true }),
    },
    ...objectEntries(currentAggregations.value).map(([key, options]) => createTableColumn(key, options!)),
  ]
})
</script>

<template>
  <UContainer class="max-w-full space-y-3 !p-0">
    <!-- TODO: skeleton -->
    <UCard
      v-if="!loadingItems"
      :ui="{
        root: 'overflow-visible rounded-none',
        footer: 'sticky bottom-0 left-0 z-[1] bg-default/75 py-4 backdrop-blur',
      }"
    >
      <template #header>
        <ItemSearchFilterByType
          v-if="itemTypes.length"
          v-model:item-type="itemType"
          v-model:weapon-class="weaponClass"
          :item-types="itemTypes"
          :weapon-classes="weaponClasses"
        />
      </template>

      <UTable
        ref="table"
        v-model:pagination="pagination"
        v-model:sorting="sorting"
        v-model:column-filters="columnFilters"
        v-model:column-visibility="columnVisibility"
        v-model:row-selection="rowSelection"
        :data="flatItems"
        :get-sub-rows="(row) => row.children"
        :ui="{
          root: 'overflow-visible',
          base: 'border-separate border-spacing-0',
          tbody: '[&>tr]:last:[&>td]:border-b-0',
          tr: 'group data-[expanded=true]:bg-elevated',
          td: 'empty:p-0 group-has-[td:not(:empty)]:border-b border-default ',
        }"
        sticky
        :columns
        :faceted-options="{
          getFacetedRowModel: getFacetedRowModel(),
          getFacetedUniqueValues: getFacetedUniqueValues(),
          getFacetedMinMaxValues: getFacetedMinMaxValues(),
        }"
        :expanded-options="{
          getRowCanExpand(row) {
            return row.depth === 0 && isUpgradableItemType
          },
        }"
        :row-selection-options="{
          enableRowSelection(row) {
            return row.depth === 0
          },
        }"
        :column-filters-options="{
          filterFns: { includesSome },
          maxLeafRowFilterDepth: 0,
        }"
        :pagination-options="{
          getPaginationRowModel: getPaginationRowModel(),
        }"
        @update:column-filters="() => { resetPagination() }"
      >
        <template #empty>
          <UiResultNotFound class="min-h-[480px]" />
        </template>
      </UTable>

      <template #footer>
        <UiGridPagination
          v-if="table?.tableApi"
          :table-api="toRef(() => table!.tableApi)"
        >
          <UButton
            v-if="Object.keys(rowSelection).length >= 2"
            size="xl"
            variant="subtle"
            :icon="isCompareMode ? 'crpg:close' : undefined"
            data-aq-shop-handler="toggle-compare"
            :label="$t('shop.compare.title')"
            @click="() => { toggleCompareMode() }"
          />
        </UiGridPagination>
      </template>
    </UCard>
  </UContainer>
</template>

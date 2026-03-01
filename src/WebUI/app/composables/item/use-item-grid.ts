import type { SelectItem } from '@nuxt/ui'
import type { ColumnFiltersState, TableOptions } from '@tanstack/vue-table'
import type { MaybeRefOrGetter, ModelRef } from 'vue'

import {
  functionalUpdate,
  getCoreRowModel,
  getFilteredRowModel,
  getPaginationRowModel,
  getSortedRowModel,
  useVueTable,
} from '@tanstack/vue-table'

import type { GroupedCompareItemsResult, Item, ItemType } from '~/models/item'
import type { SortingConfig } from '~/services/item-search-service'

import { ITEM_TYPE } from '~/models/item'
import { getAggregationsConfig, getFacetsByItemType, getFilterFn } from '~/services/item-search-service'
import { aggregationsConfig } from '~/services/item-search-service/aggregations'
import { createItemIndex } from '~/services/item-search-service/indexator'
import { extractItem, getCompareItemsResult, groupItemsByTypeAndWeaponClass } from '~/services/item-service'

import { useItemDetail } from './use-item-detail'

interface UseItemGridOptions<T extends { item: Item }> {
  items: MaybeRefOrGetter<T[]>
  sortingModel?: ModelRef<string>
  sortingConfig?: SortingConfig<string>
  withPagination?: boolean
  additionalColumns?: TableOptions<T>['columns']
  additionalColumnFilters?: MaybeRefOrGetter<ColumnFiltersState>
}

export const useItemGrid = <T extends { item: Item }>(options: UseItemGridOptions<T>) => {
  const defaultSortingConfig: SortingConfig = {
    rank_desc: { field: 'rank', order: 'desc' },
    type_asc: { field: 'type', order: 'asc' },
  }

  const {
    items: _items,
    sortingConfig: _sortingConfig,
    withPagination = false,
    additionalColumns = [],
    additionalColumnFilters,
    sortingModel,
  } = options

  const { t } = useI18n()
  const { isOpen } = useItemDetail()

  const items = toRef(_items)

  const sortingConfig = {
    ...defaultSortingConfig,
    ..._sortingConfig,
  }

  const sortingItems = computed(() => Object.keys(sortingConfig).map<SelectItem>(key => ({
    label: t(`item.sort.${key}`),
    value: key,
  })))
  const sorting = computed(() => {
    const key = sortingModel?.value ?? Object.keys(defaultSortingConfig).at(0)
    const cfg = key ? sortingConfig[key] : undefined
    return cfg
      ? [{ id: cfg.field, desc: cfg.order === 'desc' }]
      : []
  })

  const filterByNameModel = ref<string | undefined>(undefined)

  const itemType = ref<ItemType>(ITEM_TYPE.Undefined)
  const itemTypes = computed(() => getFacetsByItemType(items.value.map(wrapper => wrapper.item.type)))

  const columnFilters = computed<ColumnFiltersState>(() => [
    ...(itemType.value !== ITEM_TYPE.Undefined ? [{ id: 'type', value: itemType.value }] : []),
    ...(additionalColumnFilters ? toValue(additionalColumnFilters) : []),
  ])

  const baseColumns: TableOptions<T>['columns'] = [
    { accessorFn: row => row.item.id, id: 'id' },
    {
      accessorFn: row => row.item.type,
      id: 'type',
      filterFn: getFilterFn(aggregationsConfig.type!),
    },
    { accessorFn: row => row.item.price, id: 'price' },
    { accessorFn: row => row.item.rank, id: 'rank' },
    { accessorFn: row => row.item.name, id: 'name' },
  ]

  const columns = [...baseColumns, ...additionalColumns]

  const { pagination, setPagination } = usePagination({ pageSize: 20 })

  const grid = useVueTable({
    get data() { return items as MaybeRef },
    columns,
    getCoreRowModel: getCoreRowModel(),
    getFilteredRowModel: getFilteredRowModel(),
    getSortedRowModel: getSortedRowModel(),
    filterFns: {
      includesSome,
    },
    state: {
      get sorting() {
        return sorting.value
      },
      get globalFilter() {
        return filterByNameModel.value
      },
      get columnFilters() {
        return columnFilters.value
      },
      get pagination() {
        return pagination.value
      },
    },
    ...(withPagination && {
      getPaginationRowModel: getPaginationRowModel(),
      onPaginationChange: (updater) => {
        setPagination(functionalUpdate(updater, pagination.value))
      },
    }),
  })

  watch(() => items.value, () => {
    if (!grid.getRowModel().rows.length) {
      itemType.value = ITEM_TYPE.Undefined
    }
  })

  watch(itemType, () => {
    window.scrollTo({ behavior: 'smooth', top: 0 })
  })

  const filteredItemsCost = computed(() =>
    grid.getRowModel().rows.reduce((out, row) => out + row.original.item.price, 0),
  )

  const showPagination = computed(() => grid.getRowCount() > pagination.value.pageSize)

  const compareItemsResult = computed<GroupedCompareItemsResult[]>(() => {
    return groupItemsByTypeAndWeaponClass(
      createItemIndex(items.value.filter(wrapper => isOpen(wrapper.item.id)).map(extractItem)),
    )
      .filter(group => group.items.length >= 2)
      .map(group => ({
        compareResult: getCompareItemsResult(group.items, getAggregationsConfig(group.type, group.weaponClass)),
        type: group.type,
        weaponClass: group.weaponClass,
      }))
  })

  return {
    itemType,
    itemTypes,
    sortingItems,
    filterByNameModel,

    grid,

    pagination,
    showPagination,

    filteredItemsCost,
    compareItemsResult,
  }
}

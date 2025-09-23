<script setup lang="ts">
import type { DropdownMenuItem, SelectItem, TableColumn } from '@nuxt/ui'
import type {
  ColumnFiltersState,
  FilterFnOption,
  RowSelectionState,
  SortingState,
  VisibilityState,
} from '@tanstack/vue-table'

import {
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
  UiInputRange,
  UInput,
  UiTableColumnHeader,
  UiTableColumnHeaderLabel,
  USelect,
  UTooltip,
} from '#components'
import { h } from '#imports'

import type { ItemFlat, ItemType, WeaponClass } from '~/models/item'
import type { AggregationOptions } from '~/services/item-search-service/aggregations'

import { usePageLoading } from '~/composables/app/use-page-loading'
import { useUserItemsProvider } from '~/composables/user/use-user-items'
import { useAsyncCallback } from '~/composables/utils/use-async-callback'
import { ITEM_FIELD_FORMAT, ITEM_TYPE } from '~/models/item'
import { SomeRole } from '~/models/role'
import { getAggregationsConfig, getFacetsByItemType, getFacetsByWeaponClass } from '~/services/item-search-service'
import { AggregationView } from '~/services/item-search-service/aggregations'
import { getStepRange } from '~/services/item-search-service/helpers'
import { createItemIndex } from '~/services/item-search-service/indexator'
import {
  canUpgradeItem,
  getCompareItemsResult,
  getItems,
  getWeaponClassesByItemType,
  humanizeBucket,
} from '~/services/item-service'
import { buyUserItem } from '~/services/user-service'
import { includesSome } from '~/utils/grid'

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

const [buyItem] = useAsyncCallback(async (item: ItemFlat) => {
  await buyUserItem(item.id)
  await refreshUserItems()
  await userStore.fetchUser() // update gold
}, {
  successMessage: t('shop.item.buy.notify.success'),
  pageLoading: true,
})

const flatItems = computed(() => createItemIndex(items.value, true))

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
  },
})
const itemTypes = computed(() => getFacetsByItemType(flatItems.value.map(item => item.type)))

const isUpgradableItemType = computed(() => canUpgradeItem(itemType.value))

const weaponClass = computed({
  get() {
    if (route.query?.weaponClass) {
      return route.query.weaponClass as WeaponClass
    }
    const weaponClasses = getWeaponClassesByItemType(itemType.value)
    return weaponClasses.length !== 0 ? weaponClasses[0]! : null
  },
  set(weaponClass: WeaponClass | null) {
    table.value?.tableApi?.getColumn('weaponClass')?.setFilterValue(weaponClass ?? undefined)
    router.replace({
      query: {
        type: itemType.value,
        weaponClass: weaponClass ?? undefined,
      },
    })
    resetPagination()
    resetSorting()
  },
})
const weaponClasses = computed(() => {
  return getFacetsByWeaponClass(flatItems.value.map(item => item.weaponClass).filter(wc => wc !== null), itemType.value)
})

// TODO: нужен свой типизированный объект
function getInitialColumnFiltersState(): ColumnFiltersState {
  return [
    { id: 'type', value: itemType.value },
    ...(weaponClass.value ? [{ id: 'weaponClass', value: weaponClass.value }] : []),
  ]
}

const columnFilters = ref<ColumnFiltersState>(getInitialColumnFiltersState())
const currentAggregations = computed(() => getAggregationsConfig(itemType.value, weaponClass.value))

const columnVisibility = computed<VisibilityState>(() => {
  return {
    ...Object.entries(currentAggregations.value)
      .filter(([, value]) => value.hidden)
      .reduce((out, [key]) => {
        out[key] = false
        return out
      }, {} as VisibilityState),
    expand: isUpgradableItemType.value,
  }
})

const rowSelection = ref<RowSelectionState>({})
const [isCompareMode, toggleCompareMode] = useToggle()
watch(isCompareMode, () => {
  table.value?.tableApi.getColumn('modId')?.setFilterValue(
    isCompareMode.value
      ? table.value?.tableApi.getSelectedRowModel().rows.map(row => row.original.modId)
      : undefined,
  )
})
const compareItemsResult = computed(() => isCompareMode.value
  ? getCompareItemsResult(table.value?.tableApi.getFilteredRowModel().rows.map(row => row.original) ?? [], currentAggregations.value)
  : null)

// TODO: to cfgs
function getFilterFn(key: keyof ItemFlat, options: AggregationOptions): FilterFnOption<any> {
  if (options.view === AggregationView.Range) {
    return 'inNumberRange'
  }

  if (options.view === AggregationView.Checkbox) {
    if (options.format === ITEM_FIELD_FORMAT.List) {
      return 'arrIncludesSome'
    }

    return includesSome
  }

  return 'auto'
}

// TODO: to utils, cpec
function getFacets(rawFacets: Map<any, number>): Record<string, number> {
  return [...rawFacets]
    .reduce((out, [bucket, count]: [number | undefined | null | string | string[], number]) => {
      if (bucket === undefined || bucket === null) {
        return out
      }

      if (Array.isArray(bucket)) {
        for (const item of bucket) {
          out[item] = (out[item] ?? 0) + count
        }
      }
      else {
        out[bucket] = (out[bucket] ?? 0) + count
      }

      return out
    }, {} as Record<string, number>)
}

function createTableColumn(key: keyof ItemFlat, options: AggregationOptions): TableColumn<ItemFlat> {
  const widthPx = options.width || 160
  return {
    accessorKey: key,
    meta: {
      style: {
        th: {
          width: `${widthPx}px`,
        },
        td: {
          width: `${widthPx}px`,
        },
      },
    },
    cell: ({ row }) => h(ItemParam, {
      field: key,
      item: row.original,
      bestValue: compareItemsResult.value !== null ? compareItemsResult.value[key] : undefined,
      isCompare: isCompareMode.value,
    }, {
      ...(key === 'upkeep' && {
        default: ({ rawBuckets }: { rawBuckets: number }) => h(AppCoin, { value: t('item.format.upkeep', { upkeep: n(rawBuckets) }) }),
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
    filterFn: getFilterFn(key, options),
    header: ({ header, column }) => {
      return h(UiTableColumnHeader, {
        label: t(`item.aggregations.${header.id}.title`),
        description: t(`item.aggregations.${header.id}.description`),
        withSort: options.view === AggregationView.Range,
        sorted: column.getIsSorted(),
        withFilter: true,
        filtered: column.getIsFiltered(),
        onSort: () => column.toggleSorting(column.getIsSorted() === 'asc'),
        onResetFilter: () => column.setFilterValue(undefined),
      }, {
        ...(options.view === AggregationView.Range && {
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
          if (options.view === AggregationView.Checkbox) {
            const _facets = Object.entries(getFacets(column.getFacetedUniqueValues()))
            // @ts-expect-error TODO: https://github.com/nuxt/ui/issues/2968
            return h(USelect, {
              'class': 'w-full',
              'multiple': true,
              'variant': 'none',
              'disabled': !_facets.length,
              'size': 'xl',
              'trailing-icon': '', // TODO:
              'ui': {
                content: 'min-w-fit',
                base: 'px-0 py-0',
              },
              'items': _facets.map<SelectItem>(([bucket, count]) => {
                const humanBucket = humanizeBucket(column.id as keyof ItemFlat, bucket)
                return {
                  value: bucket,
                  label: `${humanBucket.label}${count ? ` (${count})` : ''}`,
                  ...(humanBucket.icon && { icon: `crpg:${humanBucket.icon}` }),
                }
              }),
              'modelValue': column.getFilterValue(),
              'onUpdate:modelValue': column.setFilterValue,
            }, {
              default: () => h(UiTableColumnHeaderLabel, {
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
              onClick: () => row.toggleExpanded(),
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
      // @ts-expect-error TODO: https://github.com/nuxt/ui/issues/2968
      header: ({ column }) => h(UInput, {
        'icon': 'crpg:search',
        'variant': 'soft',
        'size': 'lg',
        'class': 'w-[280px]',
        'placeholder': t('action.search'),
        'modelValue': column.getFilterValue(),
        'onUpdate:modelValue': column.setFilterValue,
      }),
      cell: ({ row }) => h(ItemTableMedia, { item: row.original, showTier: true }),
    },
    ...Object.entries(currentAggregations.value).map(([key, config]) => createTableColumn(key as keyof ItemFlat, config)),
  ]
})

const onlyNewItemsFilter = (): DropdownMenuItem => {
  const _column = table.value?.tableApi.getColumn('isNew')
  const newItemsCount = _column?.getFacetedUniqueValues().get(true)
  return {
    label: `${t(`item.aggregations.isNew.title`)}${newItemsCount ? ` (${newItemsCount})` : ''}`,
    type: 'checkbox' as const,
    ...(_column && {
      checked: (_column.getFilterValue() as boolean) || false,
      onUpdateChecked(value) {
        _column.setFilterValue(value || undefined)
      },
    }),
  }
}
</script>

<template>
  <UContainer class="max-w-full space-y-3 !p-0">
    <!-- <pre>{{ allTypes }}</pre> -->
    <!-- <pre>{{ { itemType, weaponClass } }}</pre> -->
    <!-- <pre>{{ columnFilters }}</pre> -->
    <!-- <pre>{{ columnVisibility }}</pre> -->
    <!-- <pre>{{ table?.tableApi.getState().columnFilters }}</pre> -->
    <!-- <pre>{{ table?.tableApi.getState() }}</pre> -->
    <!-- <pre>{{ columnFilters }}</pre> -->

    <!-- TODO: skeleton -->
    <UCard
      v-if="!loadingItems"
      :ui="{
        root: 'overflow-visible rounded-none',
        footer: 'sticky bottom-0 left-0 z-[1] bg-default/75 py-4 backdrop-blur',
      }"
    >
      <template #header>
        <div class="flex items-center gap-4">
          <UDropdownMenu
            :items="[onlyNewItemsFilter()]"
            :modal="false"
            size="xl"
          >
            <UChip
              inset
              size="2xl"
              :show="Boolean(table?.tableApi.getColumn('isNew')?.getIsFiltered())"
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

          <ItemSearchFilterByType
            v-if="itemTypes.length"
            v-model:item-type="itemType"
            v-model:weapon-class="weaponClass"
            :item-types="itemTypes"
            :weapon-classes="weaponClasses"
          />
        </div>
      </template>

      <UTable
        ref="table"
        v-model:pagination="pagination"
        v-model:sorting="sorting"
        v-model:column-filters="columnFilters"
        v-model:column-visibility="columnVisibility"
        v-model:row-selection="rowSelection"
        :data="flatItems"
        :ui="{
          root: 'overflow-visible',
        }"
        sticky
        :columns
        :faceted-options="{
          getFacetedRowModel: getFacetedRowModel(),
          getFacetedUniqueValues: getFacetedUniqueValues(),
          getFacetedMinMaxValues: getFacetedMinMaxValues(),
        }"
        :pagination-options="{
          getPaginationRowModel: getPaginationRowModel(),
        }"
        @update:column-filters="() => { resetPagination() }"
      >
        <template #empty>
          <UiResultNotFound class="min-h-[480px]" />
        </template>

        <template #expanded="{ row }">
          <ShopGridUpgradesTable
            class="-m-4 bg-elevated"
            :aggregation-config="currentAggregations"
            :item="row.original"
          />
        </template>
      </UTable>

      <template #footer>
        <UiGridPagination
          v-if="table?.tableApi"
          :table-api="table!.tableApi"
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

    <!-- TODO: FIXME:
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
    </DropdownItem> -->
  </UContainer>
</template>

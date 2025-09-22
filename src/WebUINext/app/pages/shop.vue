<script setup lang="ts">
import type { SelectItem, TableColumn } from '@nuxt/ui'
import type {
  ColumnFiltersState,
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
} from '#components'
import { h } from '#imports'

import type { ItemFlat, ItemType, WeaponClass } from '~/models/item'
import type { AggregationOptions } from '~/services/item-search-service/aggregations'

import { usePageLoading } from '~/composables/app/use-page-loading'
import { useUserItemsProvider } from '~/composables/user/use-user-items'
import { useAsyncCallback } from '~/composables/utils/use-async-callback'
import { ITEM_TYPE } from '~/models/item'
import { SomeRole } from '~/models/role'
import { getAggregationsConfig, getFacetsByItemType, getFacetsByWeaponClass } from '~/services/item-search-service'
import {
  AggregationView,
} from '~/services/item-search-service/aggregations'
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
const toast = useToast()

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

function createTableColumn(key: keyof ItemFlat, options: AggregationOptions): TableColumn<ItemFlat> {
  return {
    accessorKey: key,
    cell: ({ row }) => h(ItemParam, {
      field: key,
      item: row.original,
      bestValue: compareItemsResult.value !== null ? compareItemsResult.value[key] : undefined,
      isCompare: isCompareMode.value,
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
    ...(options.view === AggregationView.Range) && {
      filterFn: 'inNumberRange',
    },
    ...(options.view === AggregationView.Checkbox) && {
      filterFn: includesSome,
    },
    ...(key === 'type' || key === 'weaponClass') && {
      filterFn: 'equals',
    },
    header: ({ header, column }) => {
      // TODO: FIXME:
      const description = t(`item.aggregations.${header.id}.description`)
      return h(UiTableColumnHeader, {
        label: t(`item.aggregations.${header.id}.title`),
        withSort: options.view === AggregationView.Range,
        sorted: column.getIsSorted(),
        withFilter: true,
        filtered: column.getIsFiltered(),
        onSort: () => column.toggleSorting(column.getIsSorted() === 'asc'),
        onResetFilter: () => column.setFilterValue(undefined),
      }, {
        ...(options.view === AggregationView.Range && {
          'filter-content': () => {
            const [min, max] = column.getFacetedMinMaxValues() ?? [0, 1]
            const buckets: number[] = [...new Set(Array.from(column.getFacetedUniqueValues().keys()).flat())]
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
            const buckets: string[] = [...new Set(Array.from(column.getFacetedUniqueValues().keys()).flat())].filter(Boolean)
            // @ts-expect-error TODO:
            return h(USelect, {
              'class': 'w-full',
              'multiple': true,
              'variant': 'none',
              'size': 'xl',
              'trailing-icon': '',
              'ui': {
                content: 'min-w-fit',
                base: 'px-0 py-0',
              },
              'items': buckets.map<SelectItem>((bucket) => {
                const humanBucket = humanizeBucket(column.id as keyof ItemFlat, bucket)
                return {
                  value: bucket,
                  label: humanBucket.label,
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
      accessorKey: 'modId',
      filterFn: 'arrIncludesSome',
    },
    {
      id: 'expand',
      meta: {
        class: {
          th: '!px-0',
          td: '!px-0',
        },
      },
      cell: ({ row }) => h(UButton, {
        color: 'neutral',
        variant: 'link',
        icon: 'crpg:chevron-down',
        size: 'lg',
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
        'size': 'xl',
      }),
      cell: ({ row }) => h(UCheckbox, {
        'modelValue': row.getIsSelected(),
        'onUpdate:modelValue': (value: boolean | 'indeterminate') => row.toggleSelected(!!value),
        'size': 'xl',
      }),
      meta: {
        class: {
          th: '!px-2',
          td: '!px-2',
        },
      },
    },
    {
      accessorKey: 'name',
      // @ts-expect-error TODO:
      header: ({ column }) => h(UInput, {
        'icon': 'crpg:search',
        'variant': 'soft',
        'placeholder': t('action.search'),
        'modelValue': column.getFilterValue(),
        'onUpdate:modelValue': column.setFilterValue,
      }),
      cell: ({ row }) => h(ItemTableMedia, {
        item: row.original,
        showTier: true,
      }),
      meta: {
        class: {
          td: 'w-[360px]',
          th: 'w-[360px]',
        },
      },
    },
    ...Object.entries(currentAggregations.value).map(([key, config]) => createTableColumn(key as keyof ItemFlat, config)),
  ]
})
</script>

<template>
  <UContainer class="max-w-full space-y-3 py-3">
    <!-- {{ allTypes }} -->
    <!-- <pre>{{ { itemType, weaponClass } }}</pre> -->
    <!-- <pre>{{ columnFilters }}</pre> -->
    <!-- <pre>{{ columnVisibility }}</pre> -->
    <!-- <pre>{{ table?.tableApi.getState().columnFilters }}</pre> -->
    <!-- <pre> {{ table?.tableApi.getState() }}</pre> -->
    <!-- {{ columnFilters }} -->

    <ItemSearchFilterByType
      v-if="itemTypes.length"
      v-model:item-type="itemType"
      v-model:weapon-class="weaponClass"
      :item-types="itemTypes"
      :weapon-classes="weaponClasses"
    />

    <UTable
      ref="table"
      v-model:pagination="pagination"
      v-model:sorting="sorting"
      v-model:column-filters="columnFilters"
      v-model:column-visibility="columnVisibility"
      v-model:row-selection="rowSelection"
      class="relative rounded-md border border-muted"
      :data="flatItems"
      :columns
      :loading="loadingItems"
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
      <template v-if="!loadingItems" #empty>
        <UiResultNotFound />
      </template>

      <template #expanded="{ row }">
        <ShopGridUpgradesTable
          :aggregation-config="currentAggregations"
          :item="row.original"
        />
      </template>
    </UTable>

    <div
      class="
        sticky bottom-0 left-0 z-[1] grid grid-cols-3 items-center gap-6 bg-default/75 py-4
        backdrop-blur
      "
    >
      <div>
        <UPagination
          v-if="table?.tableApi.getCanNextPage() || table?.tableApi.getCanPreviousPage()"
          variant="soft"
          active-variant="solid"
          active-color="primary"
          :page="pagination.pageIndex + 1"
          :show-controls="false"
          show-edges
          size="xl"
          :default-page="(table?.tableApi?.initialState.pagination.pageIndex || 0) + 1"
          :items-per-page="table?.tableApi?.initialState.pagination.pageSize"
          :total="table?.tableApi?.getFilteredRowModel().rows.length"
          @update:page="(p) => table?.tableApi?.setPageIndex(p - 1)"
        />
      </div>

      <div class="flex justify-center">
        <UButton
          v-if="Object.keys(rowSelection).length >= 2"
          size="xl"
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

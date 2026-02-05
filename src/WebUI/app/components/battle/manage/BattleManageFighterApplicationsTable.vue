<script setup lang="ts">
import type { SelectItem, TableColumn } from '@nuxt/ui'
import type { ColumnFiltersState, GroupingState, SortingState, VisibilityState } from '@tanstack/vue-table'

import { getFacetedRowModel, getFacetedUniqueValues, getGroupedRowModel, getPaginationRowModel } from '@tanstack/vue-table'
import { AppApplicationStatusBadge, UButton, UiDataCell, UiDataMedia, UiGridColumnHeader, UiGridColumnHeaderLabel, UserMedia } from '#components'

import type { BattleFighterApplication } from '~/models/strategus/battle'

import { BATTLE_FIGHTER_APPLICATION_STATUS, BATTLE_MERCENARY_APPLICATION_STATUS } from '~/models/strategus/battle'

const { applications } = defineProps<{
  applications: BattleFighterApplication[]
  loading: boolean
}>()

const emit = defineEmits<{
  respond: [number, boolean]
  showItems: [number]
}>()

const { t, n } = useI18n()

const table = useTemplateRef('table')
const { getInitialPaginationState, pagination } = usePagination()

const getDefaultColumnFiltersState = (): ColumnFiltersState => {
  return [{ id: 'status', value: [BATTLE_FIGHTER_APPLICATION_STATUS.Pending] }]
}

const columnFilters = ref<ColumnFiltersState>(getDefaultColumnFiltersState())

const grouping = ref<GroupingState>(
  [
    'party_id',
  ],
)

const columns = computed<TableColumn<BattleFighterApplication>[]>(() => [
  {
    accessorKey: 'id',
    header: '',
    id: 'id',
    filterFn: 'equals',
  },
  {
    accessorFn: row => row.party.id,
    id: 'party_id',
    header: t('strategus.battle.manage.mercenaryApplications.table.columns.user.label'),
    meta: {
      class: {
        th: tw`w-56`,
      },
    },
  },
  {
    accessorFn: row => row.party.troops,
    id: 'party_troops',
    header: ({ column }) => h(UiGridColumnHeader, {
      label: 'Troops',
      withSort: true,
      sorted: column.getIsSorted(),
      onSort: () => column.toggleSorting(column.getIsSorted() === 'asc'),
    }),
    cell: ({ row }) => h(UiDataMedia, { label: n(row.original.party!.troops), icon: 'crpg:member' }),
  },
  {
    accessorFn: row => row.party.user.region,
    id: 'region',
    header: ({ column }) => {
      return h(UiGridColumnHeader, {
        label: t('strategus.battle.manage.mercenaryApplications.table.columns.region.label'),
        withFilter: true,
        filtered: column.getIsFiltered(),
        onResetFilter: () => column.setFilterValue(undefined),
      }, {
        filter: () =>
          // @ts-expect-error TODO: https://github.com/nuxt/ui/issues/2968
          h(USelect, {
            'variant': 'none',
            'multiple': true,
            'trailing-icon': '',
            'size': 'xl',
            'ui': {
              content: 'min-w-fit',
              base: 'px-0 py-0',
            },
            'items': [...column.getFacetedUniqueValues()].map<SelectItem>(([region, count]) => ({
              value: region,
              label: `${t(`region.${region}`, 0)} (${count})`,
            })),
            'modelValue': column.getFilterValue(),
            'onUpdate:modelValue': column.setFilterValue,
          }, {
            default: () => h(UiGridColumnHeaderLabel, {
              label: t('strategus.battle.manage.mercenaryApplications.table.columns.region.label'),
              withFilter: true,
            }),
          }),
      })
    },
    cell: ({ row }) => row.original.party.user.region,

    filterFn: 'arrIncludesSome',
  },
  {
    accessorKey: 'status',
    header: ({ column }) => {
      return h(UiGridColumnHeader, {
        label: t('strategus.battle.manage.mercenaryApplications.table.columns.status.label'),
        withFilter: true,
        filtered: column.getIsFiltered(),
        onResetFilter: () => column.setFilterValue(undefined),
      }, {
        filter: () =>
          // @ts-expect-error TODO: https://github.com/nuxt/ui/issues/2968
          h(USelect, {
            'variant': 'none',
            'multiple': true,
            'trailing-icon': '',
            'size': 'xl',
            'ui': {
              content: 'min-w-fit',
              base: 'px-0 py-0',
            },
            'items': [...column.getFacetedUniqueValues()].map<SelectItem>(([status, count]) => ({
              value: status,
              label: `${t(`application.status.${status}`)} (${count})`,
            })),
            'modelValue': column.getFilterValue(),
            'onUpdate:modelValue': column.setFilterValue,
          }, {
            default: () => h(UiGridColumnHeaderLabel, {
              label: t('strategus.battle.manage.mercenaryApplications.table.columns.status.label'),
              withFilter: true,
            }),
          }),
      })
    },
    filterFn: 'arrIncludesSome',
    cell: ({ row }) => h(AppApplicationStatusBadge, { status: row.original.status }),
  },
  {
    id: 'actions',
    header: '',
    cell: ({ row }) => {
      if (row.original.status !== BATTLE_MERCENARY_APPLICATION_STATUS.Pending) {
        return null
      }

      return h('div', { class: 'flex items-center justify-end gap-2' }, [
        h(UButton, {
          variant: 'subtle',
          color: 'error',
          icon: 'crpg:close',
          onClick: () => emit('respond', row.original.id, false),
        }),
        h(UButton, {
          icon: 'crpg:check',
          variant: 'subtle',
          color: 'success',
          onClick: () => emit('respond', row.original.id, true),
        }),
      ])
    },
  },
])

const columnVisibility = ref<VisibilityState>({
  id: false,
})

const sorting = ref<SortingState>([])
</script>

<template>
  <div>
    <UTable
      ref="table"
      v-model:pagination="pagination"
      v-model:column-filters="columnFilters"
      v-model:grouping="grouping"
      v-model:column-visibility="columnVisibility"
      v-model:sorting="sorting"
      :loading
      :grouping-options="{
        getGroupedRowModel: getGroupedRowModel(),
      }"
      class="relative rounded-md border border-muted"
      :data="applications"
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
      :ui="{
        td: 'empty:p-0', // helps with the colspaned row added for expand slot
      }"
    >
      <template #party_id-cell="{ row }">
        <div class="flex items-center gap-2">
          <UButton
            v-if="row.getIsGrouped() && row.getLeafRows().length > 1"
            variant="ghost"
            color="neutral"
            icon="crpg:chevron-down"
            :ui="{
              leadingIcon: ['transition-transform', row.getIsExpanded() ? 'duration-200 -rotate-90' : ''],
            }"
            @click="row.toggleExpanded()"
          />
          <UiDataCell v-if="row.groupingColumnId === 'party_id'">
            <template #leftContent>
              <UserMedia :user="row.original.party.user" />
            </template>
            <UButton variant="ghost" icon="crpg:chest" @click="$emit('showItems', row.original.party.id)" />
          </UiDataCell>
        </div>
      </template>
      <template #empty>
        <UiResultNotFound />
      </template>
    </UTable>

    <UiGridPagination
      v-if="table?.tableApi && table.tableApi.getRowCount() as number > pagination.pageSize"
      :table-api="toRef(() => table!.tableApi)"
    />
  </div>
</template>

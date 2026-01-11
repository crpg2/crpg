<script setup lang="ts">
import type { SelectItem, TableColumn } from '@nuxt/ui'
import type { ColumnFiltersState, GroupingState, SortingState, VisibilityState } from '@tanstack/vue-table'

import { getFacetedRowModel, getFacetedUniqueValues, getGroupedRowModel, getPaginationRowModel } from '@tanstack/vue-table'
import { AppCoin, BattleMercenaryApplicationStatusBadge, UButton, UiCollapsibleText, UiGridColumnHeader, UiGridColumnHeaderLabel, UiTooltipContent, UserMedia, UTooltip } from '#components'

import type { BattleMercenaryApplication } from '~/models/strategus/battle'

import { BATTLE_MERCENARY_APPLICATION_STATUS } from '~/models/strategus/battle'

const { mercenaryApplications, totalSlots, usedSlots, mercenaryApplicationId } = defineProps<{
  mercenaryApplications: BattleMercenaryApplication[]
  mercenaryApplicationId?: number
  totalSlots: number
  usedSlots: number
  loading: boolean
}>()

const emit = defineEmits<{
  respond: [number, boolean]
}>()

const { t, d } = useI18n()

const hasFreeSlots = computed(() => usedSlots < totalSlots)
const table = useTemplateRef('table')
const { getInitialPaginationState, pagination } = usePagination()

const getDefaultColumnFiltersState = (): ColumnFiltersState => {
  if (mercenaryApplicationId) {
    return [{ id: 'id', value: mercenaryApplicationId }]
  }

  return [{ id: 'status', value: [BATTLE_MERCENARY_APPLICATION_STATUS.Pending] }]
}

const columnFilters = ref<ColumnFiltersState>(getDefaultColumnFiltersState())

const grouping = ref<GroupingState>(['user_id'])

const columns = computed<TableColumn<BattleMercenaryApplication>[]>(() => [
  {
    accessorKey: 'id',
    header: '',
    id: 'id',
    filterFn: 'equals',
  },
  {
    accessorFn: row => row.user.id,
    id: 'user_id',
    header: t('strategus.battle.manage.mercenaryApplications.table.columns.user.label'),
    meta: {
      class: {
        th: tw`w-56`,
      },
    },
  },
  {
    accessorKey: 'createdAt',
    header: ({ column }) => h(UiGridColumnHeader, {
      label: t('strategus.battle.manage.mercenaryApplications.table.columns.createdAt.label'),
      withSort: true,
      sorted: column.getIsSorted(),
      onSort: () => column.toggleSorting(column.getIsSorted() === 'asc'),
    }),
    cell: ({ row }) => d(row.original.createdAt, 'short'),
  },
  {
    accessorFn: row => row.character.level,
    id: 'character_level',
    header: ({ column }) => h(UiGridColumnHeader, {
      label: t('strategus.battle.manage.mercenaryApplications.table.columns.level.label'),
      withSort: true,
      sorted: column.getIsSorted(),
      onSort: () => column.toggleSorting(column.getIsSorted() === 'asc'),
    }),
  },
  {
    accessorFn: row => row.user.region,
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
    cell: ({ row }) => row.original.user.region,
    filterFn: 'arrIncludesSome',
  },

  {
    accessorKey: 'wage',
    header: t('strategus.battle.manage.mercenaryApplications.table.columns.wage.label'),
    cell: ({ row }) => h(AppCoin, { value: row.original.wage }),
    meta: {
      class: {
        th: tw`w-32`,
      },
    },
  },
  {
    accessorKey: 'note',
    header: t('strategus.battle.manage.mercenaryApplications.table.columns.note.label'),
    cell: ({ row }) => h(UiCollapsibleText, { text: row.original.note }),
    meta: {
      class: {
        td: tw`w-64 whitespace-normal`,
      },
    },
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
              label: `${t(`strategus.battle.mercenaryApplication.status.${status}`)} (${count})`,
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
    cell: ({ row }) => h(BattleMercenaryApplicationStatusBadge, { applicationStatus: row.original.status }),
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
          label: t('action.decline'),
          variant: 'subtle',
          color: 'error',
          icon: 'crpg:close',
          onClick: () => emit('respond', row.original.id, false),
        }),
        h(UTooltip, { disabled: hasFreeSlots.value }, {
          default: () => h(UButton, {
            label: t('action.accept'),
            icon: 'crpg:check',
            variant: 'subtle',
            color: 'success',
            disabled: !hasFreeSlots.value,
            onClick: () => emit('respond', row.original.id, true),
          }),
          content: () => h(UiTooltipContent, {
            title: t('strategus.battle.manage.mercenaryApplications.respond.accept.tooltip.noFreeSlots.title'),
            validation: t('strategus.battle.manage.mercenaryApplications.respond.accept.tooltip.noFreeSlots.description', { totalSlots }),
          }),
        }),
      ])
    },
    meta: {
      class: {
        th: tw`text-right w-96`,
      },
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
      :data="mercenaryApplications"
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
      <template #user_id-cell="{ row }">
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
          <UserMedia v-if="row.groupingColumnId === 'user_id'" :user="row.original.user" />
        </div>
      </template>
      <template #empty>
        <UiResultNotFound />
      </template>
    </UTable>

    <UiGridPagination
      v-if="table?.tableApi"
      :table-api="toRef(() => table!.tableApi)"
    />
  </div>
</template>

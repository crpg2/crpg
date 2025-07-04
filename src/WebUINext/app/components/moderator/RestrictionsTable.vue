<script setup lang="ts">
import type { DropdownMenuItem, SelectItem, TableColumn } from '@nuxt/ui'
import type { PaginationState, SortingState, VisibilityState } from '@tanstack/vue-table'

import { getPaginationRowModel } from '@tanstack/vue-table'
import { navigateTo } from '#app'
import { NuxtLink, UBadge, UiCollapsibleText, UInput, UiTableColumnHeader, UiTableColumnHeaderLabel, USelect, UserMedia, UTooltip } from '#components'

import type { UserRestriction } from '~/models/user'

import { UserRestrictionStatus, UserRestrictionType } from '~/models/user'
import { computeLeftMs, parseTimestamp } from '~/utils/date'

const { hiddenRestrictedUser = false } = defineProps<{
  restrictions: UserRestriction[]
  hiddenRestrictedUser?: boolean
  loading?: boolean
}>()

const { t, d } = useI18n()

function getInitialPaginationState(): PaginationState {
  return {
    pageIndex: 0,
    pageSize: 10, // TODO: FIXME:
  }
}

const pagination = ref<PaginationState>(getInitialPaginationState())

const table = useTemplateRef('table')

const globalFilter = ref<string | undefined>(undefined)

const columns: TableColumn<UserRestriction>[] = [
  {
    accessorKey: 'id',
    meta: {
      class: {
        td: tw`min-w-16`,
      },
    },
  },
  {
    accessorKey: 'status',
    header: ({ column }) => {
      return h(UiTableColumnHeader, {
        label: t('restriction.table.column.status'),
        withFilter: true,
        filtered: column.getIsFiltered(),
        onResetFilter: () => column.setFilterValue(undefined),
      }, {
        filter: () =>
          // @ts-expect-error TODO:
          h(USelect, {
            'variant': 'none',
            'multiple': true,
            'trailing-icon': '',
            'size': 'xl',
            'ui': {
              content: 'min-w-fit',
              base: 'px-0 py-0',
            },
            'items': Object.values(UserRestrictionStatus).map<SelectItem>(status => ({
              value: status,
              label: status,
            })),
            'modelValue': column.getFilterValue(),
            'onUpdate:modelValue': column.setFilterValue,
          }, {
            default: () => h(UiTableColumnHeaderLabel, {
              label: t('restriction.table.column.status'),
              withFilter: true,
            }),
          }),
      })
    },
    filterFn: 'arrIncludesSome',
    cell: ({ row }) => row.original.status === UserRestrictionStatus.Active
      ? h(UTooltip, {
          text: t('dateTimeFormat.dd:hh:mm', { ...parseTimestamp(computeLeftMs(row.original.createdAt, Number(row.original.duration))) }),
        }, () => h(UBadge, { label: t('restriction.status.active'), size: 'sm', color: 'success', variant: 'subtle' }))
      : h(UBadge, { label: t('restriction.status.inactive'), size: 'sm', color: 'neutral', variant: 'subtle' }),
  },
  {
    accessorKey: 'type',
    header: ({ column }) => {
      return h(UiTableColumnHeader, {
        label: t('restriction.table.column.type'),
        withFilter: true,
        filtered: column.getIsFiltered(),
        onResetFilter: () => column.setFilterValue(undefined),
      }, {
        filter: () =>
          // @ts-expect-error TODO:
          h(USelect, {
            'variant': 'none',
            'multiple': true,
            'trailing-icon': '',
            'size': 'xl',
            'ui': {
              content: 'min-w-fit',
              base: 'px-0 py-0',
            },
            'items': Object.values(UserRestrictionType).map<SelectItem>(rt => ({
              value: rt,
              label: t(`restriction.type.${rt}`),
            })),
            'modelValue': column.getFilterValue(),
            'onUpdate:modelValue': column.setFilterValue,
          }, {
            default: () => h(UiTableColumnHeaderLabel, {
              label: t('restriction.table.column.type'),
              withFilter: true,
            }),
          }),
      })
    },
    filterFn: 'arrIncludesSome',
    cell: ({ row }) => t(`restriction.type.${row.original.type}`),
  },
  {
    accessorKey: 'restrictedUser.name',
    id: 'restrictedUser_name',
    // @ts-expect-error TODO: FIXME:
    header: () => h(UInput, {
      'icon': 'crpg:search',
      'variant': 'ghost',
      'size': 'xs',
      'placeholder': t('restriction.table.column.user'),
      'modelValue': globalFilter.value,
      'onUpdate:modelValue': (val: string) => globalFilter.value = val,
    }),
    cell: ({ row }) => h(NuxtLink, {
      to: { name: 'moderator-user-id-restrictions', params: { id: row.original.restrictedUser.id } },
    }, () => h(UserMedia, {
      user: row.original.restrictedUser,
      hiddenClan: true,
    })),
  },
  {
    accessorKey: 'createdAt',
    header: () => t('restriction.table.column.createdAt'),
    cell: ({ row }) => d(row.original.createdAt, 'short'),
  },
  {
    accessorKey: 'duration',
    header: () => t('restriction.table.column.duration'),
    cell: ({ row }) => t('dateTimeFormat.dd:hh:mm', { ...parseTimestamp(Number(row.original.duration)) }),
  },
  {
    accessorKey: 'reason',
    header: () => t('restriction.table.column.reason'),
    cell: ({ row }) => h(UiCollapsibleText, { text: row.original.reason }),
    meta: {
      class: {
        td: tw`max-w-96 whitespace-normal`,
      },
    },
  },
  {
    accessorKey: 'publicReason',
    header: () => t('restriction.table.column.publicReason'),
    cell: ({ row }) => h(UiCollapsibleText, { text: row.original.publicReason }),
    meta: {
      class: {
        td: tw`max-w-96 whitespace-normal`,
      },
    },
  },
  {
    accessorKey: 'restrictedByUser',
    header: () => t('restriction.table.column.restrictedBy'),
    cell: ({ row }) => h(UserMedia, {
      user: row.original.restrictedByUser,
      hiddenClan: true,
    }),
  },
]

const columnVisibility = ref<VisibilityState>(
  {
    ...(hiddenRestrictedUser && { restrictedUser_name: false }),
  },
)

const sorting = ref<SortingState>([
  { id: 'id', desc: true },
])
</script>

<template>
  <div class="space-y-4">
    <UTable
      ref="table"
      v-model:global-filter="globalFilter"
      v-model:pagination="pagination"
      v-model:column-visibility="columnVisibility"
      v-model:sorting="sorting"
      class="rounded-md border border-muted"
      :data="restrictions"
      :columns
      :loading
      :initial-state="{
        pagination: getInitialPaginationState(),
      }"
      :pagination-options="{
        getPaginationRowModel: getPaginationRowModel(),
      }"
      @select="(row) => navigateTo({ name: 'moderator-user-id-restrictions', params: { id: row.original.restrictedUser.id } })"
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
  </div>
</template>

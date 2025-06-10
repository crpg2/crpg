<script setup lang="ts">
import type { TableColumn } from '@nuxt/ui'
import type { ColumnFiltersState, PaginationState } from '@tanstack/vue-table'

import { getFacetedRowModel, getFacetedUniqueValues, getPaginationRowModel } from '@tanstack/vue-table'
import { navigateTo } from '#app'
import { NuxtLink, UBadge, UiCollapsibleText, UInput, UserMedia, UTooltip } from '#components'

import type { UserRestrictionWithActive } from '~/models/user'

import { computeLeftMs, parseTimestamp } from '~/utils/date'

defineProps<{
  restrictions: UserRestrictionWithActive[]
  hiddenCols?: string[]
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

const globalFilter = ref('')

const columns: TableColumn<UserRestrictionWithActive>[] = [
  {
    accessorKey: 'id',
  },
  {
    accessorKey: 'active',
    header: () => t('restriction.table.column.status'),
    cell: ({ row }) => row.original.active
      ? h(UTooltip, {
          text: t('dateTimeFormat.dd:hh:mm', { ...parseTimestamp(computeLeftMs(row.original.createdAt, Number(row.original.duration))) }),
        }, () => h(UBadge, { label: t('restriction.status.active'), size: 'sm', color: 'success', variant: 'subtle' }))
      : h(UBadge, { label: t('restriction.status.inactive'), size: 'sm', color: 'neutral', variant: 'subtle' }),
  },
  {
    accessorKey: 'type',
    header: () => t('restriction.table.column.type'),
    cell: ({ row }) => t(`restriction.type.${row.original.type}`),
  },
  {
    accessorKey: 'restrictedUser.name',
    header: () => h(UInput, {
      'icon': 'crpg:search',
      'variant': 'ghost',
      'size': 'xs',
      'placeholder': t('restriction.table.column.user'),
      'modelValue': globalFilter.value,
      'onUpdate:modelValue': val => globalFilter.value = val,
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
</script>

<template>
  <div class="space-y-4">
    <UTable
      ref="table"
      v-model:global-filter="globalFilter"
      v-model:pagination="pagination"
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

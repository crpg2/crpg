<script setup lang="ts">
import type { TableColumn } from '@nuxt/ui'

import { getPaginationRowModel } from '@tanstack/vue-table'
import { CharacterMedia, UButton, UiCollapsibleText, UserMedia } from '#components'

import type { BattleMercenaryApplication } from '~/models/strategus/battle'

const { mercenaryApplications } = defineProps<{
  mercenaryApplications: BattleMercenaryApplication[]
}>()

const emit = defineEmits<{
  respond: [number, boolean]
}>()

const { t } = useI18n()

const table = useTemplateRef('table')
const { getInitialPaginationState, pagination } = usePagination()

const columns: TableColumn<BattleMercenaryApplication>[] = [
  {
    accessorKey: 'user',
    // header: t('clan.application.table.column.name'),
    cell: ({ row }) => h(UserMedia, { user: row.original.user }),
  },
  {
    accessorKey: 'character',
    // header: t('clan.application.table.column.name'),
    cell: ({ row }) => h(CharacterMedia, {
      character: row.original.character,
    }),
  },
  {
    accessorKey: 'status',
    // header: t('clan.application.table.column.name'),
    // cell: ({ row }) => h(UserMedia, { user: row.original.user }),
  },
  {
    accessorKey: 'wage',

    // header: t('clan.application.table.column.name'),
    // cell: ({ row }) => h(UserMedia, { user: row.original.user }),
  },
  {
    accessorKey: 'note',
    // header: () => t('restriction.table.column.reason'),
    cell: ({ row }) => h(UiCollapsibleText, { text: row.original.note }),
    meta: {
      class: {
        td: tw`max-w-96 whitespace-normal`,
      },
    },
    // header: t('clan.application.table.column.name'),
    // cell: ({ row }) => h(UserMedia, { user: row.original.user }),
  },
  {
    header: t('clan.application.table.column.actions'), // TODO:
    cell: ({ row }) => h('div', { class: 'flex items-center justify-end gap-2' }, [
      h(UButton, {
        'label': t('action.decline'),
        'variant': 'subtle',
        'color': 'error',
        'icon': 'crpg:close',
        'data-aq-clan-application-action': 'decline',
        'onClick': () => emit('respond', row.original.id, false),
      }),
      h(UButton, {
        'label': t('action.accept'),
        'icon': 'crpg:check',
        'variant': 'subtle',
        'color': 'success',
        'data-aq-clan-application-action': 'accept',
        'onClick': () => emit('respond', row.original.id, true),
      }),
    ]),
    meta: {
      class: {
        th: tw`text-right`,
      },
    },
  },
]
</script>

<template>
  <div>
    <!-- :loading="responding" -->
    <UTable
      ref="table"
      v-model:pagination="pagination"
      class="relative rounded-md border border-muted"
      :data="mercenaryApplications"
      :columns
      :initial-state="{
        pagination: getInitialPaginationState(),
      }"
      :pagination-options="{
        getPaginationRowModel: getPaginationRowModel(),
      }"
    >
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

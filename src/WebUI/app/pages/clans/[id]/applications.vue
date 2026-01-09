<script setup lang="ts">
import type { TableColumn } from '@nuxt/ui'

import { getPaginationRowModel } from '@tanstack/vue-table'
import { UButton, UserMedia } from '#components'

import type { ClanInvitation } from '~/models/clan'

import { useClan } from '~/composables/clan/use-clan'
import { useClanApplications } from '~/composables/clan/use-clan-applications'
import { useUser } from '~/composables/user/use-user'
import { useAsyncCallback } from '~/composables/utils/use-async-callback'
import { SomeRole } from '~/models/role'
import { canManageApplicationsValidate } from '~/services/clan-service'

definePageMeta({
  props: true,
  roles: SomeRole,
  middleware: [
    'clan-foreign-validate',
    /**
     * @description clan role check
     */
    () => {
      const { clanMemberRole } = useUser()
      if (clanMemberRole.value && !canManageApplicationsValidate(clanMemberRole.value)) {
        return navigateTo({ name: 'clans' })
      }
    },
  ],
})

const toast = useToast()
const { t } = useI18n()
const { clan } = useClan()

const {
  applications,
  loadClanApplications,
  loadingClanApplications,
  respondToClanInvitation,
} = useClanApplications()

const [onRespond, responding] = useAsyncCallback(
  async (application: ClanInvitation, status: boolean) => {
    await respondToClanInvitation(application.id, status)
    await loadClanApplications()
    toast.add({
      title: status
        ? t('clan.application.respond.accept.notify.success')
        : t('clan.application.respond.decline.notify.success'),
      close: false,
      color: 'success',
    })
  },
  {
    pageLoading: true,
  },
)

const table = useTemplateRef('table')

const { getInitialPaginationState, pagination } = usePagination()

const columns: TableColumn<ClanInvitation>[] = [
  {
    accessorKey: 'invitee',
    header: t('clan.application.table.column.name'),
    cell: ({ row }) => h(UserMedia, {
      user: row.original.invitee,
      hiddenClan: true,
    }),
  },
  {
    header: t('clan.application.table.column.actions'),
    cell: ({ row }) => h('div', { class: 'flex items-center justify-end gap-2' }, [
      h(UButton, {
        'label': t('action.decline'),
        'variant': 'subtle',
        'color': 'error',
        'icon': 'crpg:close',
        'data-aq-clan-application-action': 'decline',
        'onClick': () => onRespond(row.original, false),
      }),
      h(UButton, {
        'label': t('action.accept'),
        'icon': 'crpg:check',
        'variant': 'subtle',
        'color': 'success',
        'data-aq-clan-application-action': 'accept',
        'onClick': () => onRespond(row.original, true),
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
  <UContainer class="space-y-12 py-6">
    <AppPageHeaderGroup
      :title="$t('clan.application.page.title')"
      :back-to="{ name: 'clans-id', params: { id: clan.id } }"
    />

    <div class="mx-auto max-w-2xl space-y-10">
      <UTable
        ref="table"
        v-model:pagination="pagination"
        class="relative rounded-md border border-muted"
        :loading="loadingClanApplications || responding"
        :data="applications"
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
  </UContainer>
</template>

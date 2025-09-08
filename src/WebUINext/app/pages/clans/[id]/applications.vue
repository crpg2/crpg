<script setup lang="ts">
import type { TableColumn } from '@nuxt/ui'
import type { PaginationState } from '@tanstack/vue-table'

import { getPaginationRowModel } from '@tanstack/vue-table'
import { UButton, UserMedia } from '#components'

import type { ClanInvitation } from '~/models/clan'

import { usePageLoading } from '~/composables/app/use-page-loading'
import { useClanApplications } from '~/composables/clan/use-clan-applications'
import { useAsyncCallback } from '~/composables/utils/use-async-callback'
import { SomeRole } from '~/models/role'
import { canManageApplicationsValidate } from '~/services/clan-service'

const props = defineProps<{
  id: string
}>()

definePageMeta({
  props: true,
  roles: SomeRole,
  middleware: [
    'clan-id-param-validate',
    'clan-foreign-validate',
    /**
     * @description clan role check
     */
    () => {
      const userStore = useUserStore()
      if (userStore.clanMemberRole && !canManageApplicationsValidate(userStore.clanMemberRole)) {
        return navigateTo({ name: 'clans' })
      }
    },
  ],
})

const clanId = computed(() => Number(props.id))

const toast = useToast()
const { t } = useI18n()

const {
  applications,
  loadClanApplications,
  loadingClanApplications,
  respondToClanInvitation,
} = useClanApplications(clanId)

loadClanApplications()

const {
  execute: respond,
  isLoading: responding,
} = useAsyncCallback(
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
)

const table = useTemplateRef('table')

const pagination = ref<PaginationState>(getInitialPaginationState())

function getInitialPaginationState(): PaginationState {
  return {
    pageIndex: 0,
    pageSize: 10, // TODO: FIXME:
  }
}

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
        'size': 'sm',
        'variant': 'subtle',
        'color': 'error',
        'icon': 'crpg:close',
        'data-aq-clan-application-action': 'decline',
        'onClick': () => respond(row.original, false),
      }),
      h(UButton, {
        'label': t('action.accept'),
        'icon': 'crpg:check',
        'size': 'sm',
        'variant': 'subtle',
        'color': 'success',
        'data-aq-clan-application-action': 'accept',
        'onClick': () => respond(row.original, true),
      }),
    ]),
    meta: {
      class: {
        th: tw`text-right`,
        td: tw``,
      },
    },
  },
]
</script>

<template>
  <UContainer class="space-y-12 py-6">
    <AppPageHeaderGroup
      :title="$t('clan.application.page.title')"
      :back-to="{ name: 'clans-id', params: { id: clanId } }"
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

      <UPagination
        v-if="table?.tableApi.getCanNextPage() || table?.tableApi.getCanPreviousPage()"
        class="flex justify-center"
        variant="soft"
        color="secondary"
        active-variant="solid"
        active-color="primary"
        :page="pagination.pageIndex + 1"
        :show-controls="false"
        :default-page="(table?.tableApi.initialState.pagination.pageIndex || 0) + 1"
        :items-per-page="table?.tableApi.initialState.pagination.pageSize"
        :total="table?.tableApi.getFilteredRowModel().rows.length"
        @update:page="(p) => table?.tableApi.setPageIndex(p - 1)"
      />
    </div>
  </UContainer>
</template>

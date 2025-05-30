<script setup lang="ts">
import type { ClanInvitation } from '~/models/clan'

import { useClanApplications } from '~/composables/clan/use-clan-applications'
import { useAsyncCallback } from '~/composables/utils/use-async-callback'
import { usePagination } from '~/composables/utils/use-pagination'
import { SomeRole } from '~/models/role'
import { canManageApplicationsValidate, respondToClanInvitation } from '~/services/clan-service'

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

const { $notify } = useNuxtApp()
const { t } = useI18n()
const clanId = computed(() => Number(props.id))

const { applications, loadClanApplications } = useClanApplications()
const { pageModel, perPage } = usePagination()

const {
  execute: respond,
  loading: responding,
} = useAsyncCallback(
  async (application: ClanInvitation, status: boolean) => {
    await respondToClanInvitation(clanId.value, application.id, status)
    await loadClanApplications(0, { id: clanId.value })
    status
      ? $notify(t('clan.application.respond.accept.notify.success'))
      : $notify(t('clan.application.respond.decline.notify.success'))
  },
)

loadClanApplications(0, { id: clanId.value })
</script>

<template>
  <div class="p-6">
    <OLoading
      full-page
      :active="responding"
      icon-size="xl"
    />
    <NuxtLink
      :to="{ name: 'clans-id', params: { id: clanId } }"
    >
      <OButton
        v-tooltip.bottom="$t('nav.back')"
        variant="secondary"
        size="xl"
        outlined
        rounded
        icon-left="arrow-left"
        data-aq-link="back-to-clan"
      />
    </NuxtLink>

    <div class="mx-auto max-w-2xl py-6">
      <h1 class="mb-14 text-center text-xl text-content-100">
        {{ $t('clan.application.page.title') }}
      </h1>

      <div class="container">
        <div class="mx-auto max-w-3xl">
          <OTable
            v-model:current-page="pageModel"
            :data="applications"
            :per-page="perPage"
            bordered
            :paginated="applications.length > perPage"
          >
            <OTableColumn
              v-slot="{ row: application }: { row: ClanInvitation }"
              field="name"
              :label="$t('clan.application.table.column.name')"
            >
              <UserMedia
                :user="application.invitee"
                hidden-clan
              />
            </OTableColumn>

            <OTableColumn
              v-slot="{ row: application }: { row: ClanInvitation }"
              field="action"
              position="right"
              :label="$t('clan.application.table.column.actions')"
              width="160"
            >
              <div class="flex items-center justify-center gap-1">
                <OButton
                  variant="primary"
                  inverted
                  :label="$t('action.accept')"
                  size="xs"
                  data-aq-clan-application-action="accept"
                  @click="respond(application, true)"
                />
                <OButton
                  variant="primary"
                  inverted
                  :label="$t('action.decline')"
                  size="xs"
                  data-aq-clan-application-action="decline"
                  @click="respond(application, false)"
                />
              </div>
            </OTableColumn>

            <template #empty>
              <AppResultNotFound />
            </template>
          </OTable>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { Clan } from '~/models/clan'

import { useClan } from '~/composables/clan/use-clan'
import { useClanMembers } from '~/composables/clan/use-clan-members'
import { useAsyncCallback } from '~/composables/utils/use-async-callback'
import { kickClanMember, updateClan } from '~/services/clan-service'
import { notify } from '~/services/notification-service'
import { t } from '~/services/translate-service'
import { useUserStore } from '~/stores/user'

const props = defineProps<{
  id: string
}>()

definePage({
  meta: {
    layout: 'default',
    middleware: 'canUpdateClan', // TODO: ['clanIdParamValidate', 'canManageApplications']
    roles: ['User', 'Moderator', 'Admin'],
  },
  props: true,
})

const userStore = useUserStore()
const router = useRouter()

const { clan, clanId, loadClan } = useClan(props.id)
const { isLastMember, loadClanMembers } = useClanMembers()

const { execute: onUpdateClan, loading: updatingClan } = useAsyncCallback(async (form: Omit<Clan, 'id'>) => {
  const clan = await updateClan(clanId.value, { ...form, id: clanId.value })
  await userStore.fetchUser() // update clan info
  notify(t('clan.update.notify.success'))
  router.replace({ name: 'ClansId', params: { id: clan.id } })
})

const { execute: onDeleteClan, loading: deletingClan } = useAsyncCallback(async () => {
  await kickClanMember(clanId.value, userStore.user!.id) // delete yourself from the clan as the only member === delete the clan
  userStore.fetchUser() // update clan info
  notify(t('clan.delete.notify.success'))
  return router.replace({ name: 'Clans' })
})

await Promise.all([loadClan(0, { id: clanId.value }), loadClanMembers(0, { id: clanId.value })])
</script>

<template>
  <div class="p-6">
    <OLoading
      full-page
      :active="updatingClan || deletingClan"
      icon-size="xl"
    />
    <RouterLink :to="{ name: 'ClansId', params: { id: clanId } }">
      <OButton
        v-tooltip.bottom="$t('nav.back')"
        variant="secondary"
        size="xl"
        outlined
        rounded
        icon-left="arrow-left"
      />
    </RouterLink>

    <div class="mx-auto max-w-2xl space-y-10 py-6">
      <div class="space-y-14">
        <h1 class="text-center text-xl text-content-100">
          {{ $t('clan.update.page.title') }}
        </h1>

        <div class="container">
          <div class="mx-auto max-w-3xl">
            <ClanForm
              :clan-id="clanId"
              :clan="clan!"
              @submit="onUpdateClan"
            />
          </div>
        </div>
      </div>

      <i18n-t
        scope="global"
        keypath="clan.delete.title"
        tag="div"
        class="text-center"
      >
        <template #link>
          <Modal closable>
            <span class="cursor-pointer text-status-danger hover:text-opacity-80">
              {{ $t('clan.delete.link') }}
            </span>
            <template #popper="{ hide }">
              <ConfirmActionForm
                v-if="isLastMember"
                :title="$t('clan.delete.dialog.title')"
                :description="$t('clan.delete.dialog.desc')"
                :name="clan!.name"
                :confirm-label="$t('action.delete')"
                data-aq-clan-delete-confirm-action-form
                @cancel="hide"
                @confirm="
                  () => {
                    onDeleteClan();
                    hide();
                  }
                "
              />
              <div
                v-else
                class="px-12 pb-11 pt-20 text-center"
                data-aq-clan-delete-required-message
              >
                {{ $t('clan.delete.required') }}
              </div>
            </template>
          </Modal>
        </template>
      </i18n-t>
    </div>
  </div>
</template>

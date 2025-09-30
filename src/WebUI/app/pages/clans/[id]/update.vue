<script setup lang="ts">
import { LazyAppConfirmActionDialog } from '#components'

import type { ClanUpdate } from '~/models/clan'

import { useClan } from '~/composables/clan/use-clan'
import { useClanMembers } from '~/composables/clan/use-clan-members'
import { useAsyncCallback } from '~/composables/utils/use-async-callback'
import { SomeRole } from '~/models/role'
import { canUpdateClanValidate } from '~/services/clan-service'
import { useUserStore } from '~/stores/user'

definePageMeta({
  props: true,
  roles: SomeRole,
  middleware: [
    'clan-foreign-validate',
    /**
     * @description clan role check
     */
    () => {
      const userStore = useUserStore()
      if (userStore.clanMemberRole && !canUpdateClanValidate(userStore.clanMemberRole)) {
        return navigateTo({ name: 'clans' })
      }
    },
  ],
})

const toast = useToast()
const { t } = useI18n()

const userStore = useUserStore()
const { clan, updateClan } = useClan()
const { isLastMember, kickClanMember } = useClanMembers()

function backToClanPage() {
  return navigateTo({ name: 'clans-id', params: { id: clan.value.id } })
}

const [onUpdateClan] = useAsyncCallback(
  async (data: ClanUpdate) => {
    await updateClan(data)
    await userStore.fetchUser() // update clan info
    toast.add({
      title: t('clan.update.notify.success'),
      close: false,
      color: 'success',
    })
    backToClanPage()
  },
  {
    throwError: true,
    successMessage: t('clan.update.notify.success'),
    onSuccess: backToClanPage,
  },
)

const [onDeleteClan] = useAsyncCallback(
  async () => {
    await kickClanMember(userStore.user!.id) // delete yourself from the clan as the only member === delete the clan
    await userStore.fetchUser() // update clan info
  },
  {
    onSuccess: () => navigateTo({ name: 'clans' }),
    successMessage: t('clan.delete.notify.success'),
  },
)

const overlay = useOverlay()

const confirmDeleteDialog = overlay.create(LazyAppConfirmActionDialog, {
  props: {
    title: t('clan.delete.dialog.title'),
    description: t('clan.delete.dialog.desc'),
    confirm: clan.value.name,
    confirmLabel: t('action.delete'),
  },
})

async function deleteClan() {
  if (!(await confirmDeleteDialog.open())) {
    return
  }

  onDeleteClan()
}
</script>

<template>
  <UContainer class="space-y-12 py-6">
    <AppPageHeaderGroup
      :title="$t('clan.update.page.title')"
      :back-to="{ name: 'clans-id', params: { id: clan.id } }"
    />

    <div class="mx-auto max-w-2xl space-y-4">
      <ClanForm
        :clan-id="clan.id"
        :clan="clan"
        @submit="onUpdateClan"
      />

      <div class="space-y-2.5 text-center">
        <div
          v-if="!isLastMember"
          class="text-warning"
          data-aq-clan-delete-required-message
        >
          {{ t('clan.delete.required') }}
        </div>

        <i18n-t
          scope="global"
          keypath="clan.delete.title"
          :class="{ 'pointer-events-none opacity-30': !isLastMember }"
          tag="div"
        >
          <template #link>
            <ULink
              class="
                cursor-pointer text-error
                hover:text-error/80
              "
              @click="deleteClan"
            >
              {{ $t('clan.delete.link') }}
            </ULink>
          </template>
        </i18n-t>
      </div>
    </div>
  </UContainer>
</template>

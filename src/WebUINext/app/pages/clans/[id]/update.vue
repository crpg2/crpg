<script setup lang="ts">
import { LazyAppConfirmActionDialog } from '#components'

import type { ClanUpdate } from '~/models/clan'

import { usePageLoading } from '~/composables/app/use-page-loading'
import { useClan } from '~/composables/clan/use-clan'
import { useClanMembers } from '~/composables/clan/use-clan-members'
import { useAsyncCallback } from '~/composables/utils/use-async-callback'
import { SomeRole } from '~/models/role'
import { canUpdateClanValidate } from '~/services/clan-service'
import { useUserStore } from '~/stores/user'

const props = defineProps<{ id: string }>()

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
      if (userStore.clanMemberRole && !canUpdateClanValidate(userStore.clanMemberRole)) {
        return navigateTo({ name: 'clans' })
      }
    },
  ],
})

const toast = useToast()
const { t } = useI18n()

const clanId = computed(() => Number(props.id))

const userStore = useUserStore()

const { clan, loadClan, loadingClan, updateClan } = useClan(clanId)
const { isLastMember, loadClanMembers, kickClanMember } = useClanMembers(clanId)

function backToClanPage() {
  navigateTo({ name: 'clans-id', params: { id: clanId.value } })
}

const {
  execute: onUpdateClan,
  isLoading: updatingClan,
} = useAsyncCallback(
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
  { throwError: true },
)

const {
  execute: onDeleteClan,
  isLoading: deletingClan,
} = useAsyncCallback(
  async () => {
    await kickClanMember(userStore.user!.id) // delete yourself from the clan as the only member === delete the clan
    await userStore.fetchUser() // update clan info
    toast.add({
      title: t('clan.delete.notify.success'),
      close: false,
      color: 'success',
    })
    navigateTo({ name: 'clans' })
  },
)

usePageLoading({
  watch: [loadingClan, updatingClan, deletingClan],
})

// TODO:
await loadClan() // provider
loadClanMembers()

const overlay = useOverlay()

const confirmDeleteDialog = overlay.create(LazyAppConfirmActionDialog, {
  props: {
    title: t('clan.delete.dialog.title'),
    description: t('clan.delete.dialog.desc'),
    confirm: clan.value?.name || 'TODO: FIXME:',
    confirmLabel: t('action.delete'),
    onClose: () => confirmDeleteDialog.close(),
    onConfirm: onDeleteClan,
  },
})
</script>

<template>
  <UContainer
    class="space-y-12 py-6"
  >
    <AppPageHeaderGroup
      :title="$t('clan.update.page.title')"
      :back-to="{ name: 'clans-id', params: { id: clanId } }"
    />

    <div class="mx-auto max-w-2xl space-y-4">
      <ClanForm
        v-if="clan"
        :clan-id="clanId"
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
          tag="div"
          :class="{ 'pointer-events-none opacity-30': isLastMember }"
        >
          <template #link>
            <span
              class="cursor-pointer text-error"
              @click="() => confirmDeleteDialog.open()"
            >
              {{ $t('clan.delete.link') }}
            </span>
          </template>
        </i18n-t>
      </div>
    </div>
  </UContainer>
</template>

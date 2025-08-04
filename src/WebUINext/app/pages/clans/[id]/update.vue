<script setup lang="ts">
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
const { togglePageLoading } = usePageLoading()

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

Promise.all([
  loadClan(),
  loadClanMembers(),
])

watchEffect(() => {
  togglePageLoading(loadingClan.value || updatingClan.value || deletingClan.value)
})
</script>

<template>
  <UContainer v-if="clan" class="space-y-6 py-6">
    <AppBackButton @click="backToClanPage" />

    <div class="mx-auto max-w-2xl space-y-10">
      <h1 class="text-content-100 text-center text-xl">
        {{ $t('clan.update.page.title') }}
      </h1>

      <ClanForm
        :clan-id="clanId"
        :clan="clan"
        @submit="onUpdateClan"
      />

      <i18n-t
        scope="global"
        keypath="clan.delete.title"
        tag="div"
        class="text-center"
      >
        <template #link>
          <UModal :title="$t('clan.delete.dialog.title')">
            <span class="cursor-pointer text-error">
              {{ $t('clan.delete.link') }}
            </span>

            <template #body="{ close }">
              <AppConfirmActionForm
                v-if="isLastMember"
                :description="$t('clan.delete.dialog.desc')"
                :name="clan!.name"
                :confirm-label="$t('action.delete')"
                data-aq-clan-delete-confirm-action-form
                @cancel="close"
                @confirm=" () => {
                  onDeleteClan();
                  close();
                }"
              />

              <div
                v-else
                class="text-center"
                data-aq-clan-delete-required-message
              >
                {{ $t('clan.delete.required') }}
              </div>
            </template>
          </UModal>
        </template>
      </i18n-t>
    </div>
  </UContainer>
</template>

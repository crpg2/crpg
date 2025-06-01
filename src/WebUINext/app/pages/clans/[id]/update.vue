<script setup lang="ts">
import type { Clan } from '~/models/clan'

import { useClan } from '~/composables/clan/use-clan'
import { useClanMembers } from '~/composables/clan/use-clan-members'
import { useAsyncCallback } from '~/composables/utils/use-async-callback'
import { SomeRole } from '~/models/role'
import { canUpdateClanValidate, kickClanMember, updateClan } from '~/services/clan-service'
import { useUserStore } from '~/stores/user'

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

      if (userStore.clanMemberRole && !canUpdateClanValidate(userStore.clanMemberRole)) {
        return navigateTo({ name: 'clans' })
      }
    },
  ],
})

const { $notify } = useNuxtApp()
const { t } = useI18n()

const clanId = computed(() => Number(props.id))

const userStore = useUserStore()

const { clan, loadClan, loadingClan } = useClan()
const { isLastMember, loadClanMembers } = useClanMembers()

function backToClanPage() {
  navigateTo({ name: 'clans-id', params: { id: clanId.value } })
}

const {
  execute: onUpdateClan,
  loading: updatingClan,
} = useAsyncCallback(
  async (form: Omit<Clan, 'id'>) => {
    await updateClan(clanId.value, { ...form, id: clanId.value })
    await userStore.fetchUser() // update clan info
    $notify(t('clan.update.notify.success'))
    backToClanPage()
  },
)

const {
  execute: onDeleteClan,
  loading: deletingClan,
} = useAsyncCallback(
  async () => {
    await kickClanMember(clanId.value, userStore.user!.id) // delete yourself from the clan as the only member === delete the clan
    await userStore.fetchUser() // update clan info
    $notify(t('clan.delete.notify.success'))
    navigateTo({ name: 'clans' })
  },
)

Promise.all([
  loadClan(0, { id: clanId.value }),
  loadClanMembers(0, { id: clanId.value }),
])
</script>

<template>
  <div class="relative p-6">
    <!-- TODO: new cmp -->
    <OLoading
      full-page
      :active="loadingClan || updatingClan || deletingClan"
      icon-size="xl"
    />

    <!-- TODO: to cmp -->
    <UButton
      color="secondary"
      variant="outline"
      size="xl"
      icon="crpg:arrow-left"
      @click="backToClanPage"
    />

    <UContainer v-if="clan" class="py-6">
      <div class="mx-auto max-w-2xl space-y-10">
        <div class="space-y-14">
          <h1 class="text-center text-xl text-content-100">
            {{ $t('clan.update.page.title') }}
          </h1>

          <div class="container">
            <div class="mx-auto max-w-3xl">
              <ClanForm
                :clan-id="clanId"
                :clan="clan"
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
            <UModal
              :title="$t('clan.delete.dialog.title')"
              :close="{
                size: 'sm',
                color: 'secondary',
                variant: 'solid',
              }"
            >
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
  </div>
</template>

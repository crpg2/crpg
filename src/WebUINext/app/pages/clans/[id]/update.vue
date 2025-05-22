<script setup lang="ts">
import type { RouteLocationNormalizedLoaded } from 'vue-router'

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

const { clan, loadClan } = useClan()
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
  <div class="p-6">
    <OLoading
      full-page
      :active="updatingClan || deletingClan"
      icon-size="xl"
    />
    <OButton
      v-tooltip.bottom="$t('nav.back')"
      variant="secondary"
      size="xl"
      outlined
      rounded
      icon-left="arrow-left"
      @click="backToClanPage"
    />

    <div v-if="clan" class="mx-auto max-w-2xl space-y-10 py-6">
      <div class="space-y-14">
        <h1 class="text-center text-xl text-content-100">
          {{ $t('clan.update.page.title') }}
        </h1>

        <div class="container">
          <div class="mx-auto max-w-3xl">
            <ClanForm
              :clan-id="clanId"
              :clan
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
          <UiModal closable>
            <span class="cursor-pointer text-status-danger hover:text-opacity-80">
              {{ $t('clan.delete.link') }}
            </span>
            <template #popper="{ hide }">
              <AppConfirmActionForm
                v-if="isLastMember"
                :title="$t('clan.delete.dialog.title')"
                :description="$t('clan.delete.dialog.desc')"
                :name="clan.name"
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
          </UiModal>
        </template>
      </i18n-t>
    </div>
  </div>
</template>

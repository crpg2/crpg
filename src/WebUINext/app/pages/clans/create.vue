<script setup lang="ts">
import { UContainer } from '#components'

import type { Clan } from '~/models/clan'

import { usePageLoading } from '~/composables/app/use-page-loading'
import { useAsyncCallback } from '~/composables/utils/use-async-callback'
import { SomeRole } from '~/models/role'
import { createClan } from '~/services/clan-service'
import { useUserStore } from '~/stores/user'

definePageMeta({
  roles: SomeRole,
  middleware: [
    /**
     * @description If you have a clan, you can't create a new one
     */
    () => {
      const userStore = useUserStore()
      if (userStore.clan) {
        return navigateTo({
          name: 'clans-id',
          params: { id: userStore.clan.id },
        })
      }
    },
  ],
})

const toast = useToast()
const { t } = useI18n()

const userStore = useUserStore()

const {
  execute: onCreateClan,
  isLoading: creatingClan,
} = useAsyncCallback(
  async (form: Omit<Clan, 'id'>) => {
    const clan = await createClan(form)
    await userStore.fetchUser()
    toast.add({
      title: t('clan.create.notify.success'),
      close: false,
      color: 'success',
    })
    navigateTo({ name: 'clans-id', params: { id: clan.id } }, { replace: true })
  },
)

usePageLoading({ watch: [creatingClan] })
</script>

<template>
  <UContainer
    class="space-y-12 py-6"
  >
    <AppPageHeaderGroup
      :title="$t('clan.create.page.title')"
    />

    <div class="mx-auto max-w-2xl space-y-10">
      <ClanForm @submit="onCreateClan" />
    </div>
  </UContainer>
</template>

<script setup lang="ts">
import type { Clan } from '~/models/clan'

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

const { $notify } = useNuxtApp()
const { t } = useI18n()

const userStore = useUserStore()
const router = useRouter()

const {
  execute: onCreateClan,
  loading: creatingClan,
} = useAsyncCallback(
  async (form: Omit<Clan, 'id'>) => {
    const clan = await createClan(form)
    await userStore.fetchUser()
    $notify(t('clan.create.notify.success'))
    return router.replace({ name: 'clans-id', params: { id: clan.id } })
  },
)
</script>

<template>
  <div class="p-6">
    <OLoading
      full-page
      :active="creatingClan"
      icon-size="xl"
    />
    <div class="mx-auto max-w-2xl py-6">
      <h1 class="mb-14 text-center text-xl text-content-100">
        {{ $t('clan.create.page.title') }}
      </h1>

      <div class="container">
        <div class="mx-auto max-w-3xl">
          <ClanForm @submit="onCreateClan" />
        </div>
      </div>
    </div>
  </div>
</template>

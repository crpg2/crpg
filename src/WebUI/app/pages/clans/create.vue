<script setup lang="ts">
import type { Clan } from '~/models/clan'

import { useUser } from '~/composables/user/use-user'
import { useAsyncCallback } from '~/composables/utils/use-async-callback'
import { SomeRole } from '~/models/role'
import { createClan } from '~/services/clan-service'

definePageMeta({
  roles: SomeRole,
  middleware: [
    /**
     * @description If you have a clan, you can't create a new one
     */
    () => {
      const { clan } = useUser()
      if (clan.value) {
        return navigateTo({
          name: 'clans-id',
          params: { id: clan.value.id },
        })
      }
    },
  ],
})

const toast = useToast()
const { t } = useI18n()

const { fetchUser } = useUser()

const [onCreateClan] = useAsyncCallback(
  async (form: Omit<Clan, 'id'>) => {
    const clan = await createClan(form)
    await fetchUser()
    toast.add({ title: t('clan.create.notify.success'), close: false, color: 'success' })
    navigateTo({ name: 'clans-id', params: { id: clan.id } }, { replace: true })
  },
  {
    pageLoading: true,
  },
)
</script>

<template>
  <UContainer
    class="space-y-12 py-6"
  >
    <AppPageHeaderGroup
      :title="$t('clan.create.page.title')"
      :back-to="{ name: 'clans' }"
    />

    <div class="mx-auto max-w-2xl space-y-10">
      <ClanForm @submit="onCreateClan" />
    </div>
  </UContainer>
</template>

<script setup lang="ts">
import type { NavigationMenuItem } from '@nuxt/ui'

import type { UserPrivate } from '~/models/user'

import { useModerationUserProvider } from '~/composables/moderator/use-moderation-user'
import { getUserById } from '~/services/restriction-service'

const route = useRoute('moderator-user-id')

const { state: user, execute: loadUser } = await useAsyncState(
  () => getUserById(Number(route.params.id)),
  null,
  { immediate: true, resetOnExecute: false },
)

// TODO:
if (!user.value) {
  navigateTo({ name: 'moderator' })
}

useModerationUserProvider(user as Ref<UserPrivate>)

const { t } = useI18n()

const navigationItems = computed<NavigationMenuItem[]>(() => [
  {
    label: 'Information',
    to: { name: 'moderator-user-id-information' },
    active: route.name === 'moderator-user-id-information', // hack
  },
  {
    label: t('restriction.title'),
    to: { name: 'moderator-user-id-restrictions' },
  },
  {
    label: 'Logs',
    to: { name: 'moderator-user-id-activity-logs' },
  },
])
</script>

<template>
  <UContainer>
    <div v-if="user" class="mb-14 flex items-center justify-center gap-8">
      <UserMedia :user size="xl" />

      <UNavigationMenu
        color="primary"
        :items="navigationItems"
      />
    </div>

    <NuxtPage @update="loadUser" />
  </UContainer>
</template>

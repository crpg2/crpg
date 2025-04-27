<script setup lang="ts">
import type { RouteLocationRaw } from 'vue-router'
import type { RouteNamedMap } from 'vue-router/auto-routes'

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

const links: { name: keyof RouteNamedMap, label: string }[] = [
  {
    name: 'moderator-user-id-information',
    label: 'Information',
  },
  {
    name: 'moderator-user-id-restrictions',
    label: t('restriction.title'),
  },
  {
    name: 'moderator-user-id-activity-logs',
    label: 'Logs',
  },
]
</script>

<template>
  <UContainer>
    <div v-if="user" class="mb-14 flex items-center justify-center gap-8">
      <UserMedia :user size="xl" />

      <div class="flex items-center justify-center gap-2">
        <NuxtLink
          v-for="{ name, label } in links"
          :key="name"
          v-slot="{ isExactActive }"
          :to="({ name } as RouteLocationRaw)"
        >
          <UButton
            color="secondary"
            :variant="isExactActive ? 'solid' : 'soft'"
            size="lg"
            :label
          />
        </NuxtLink>
      </div>
    </div>

    <NuxtPage @update="loadUser" />
  </UContainer>
</template>

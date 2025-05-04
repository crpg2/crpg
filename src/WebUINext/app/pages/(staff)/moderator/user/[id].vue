<script setup lang="ts">
import type { RouteNamedMap } from 'vue-router/auto-routes'

import { getUserById } from '~/services/restriction-service'
import { moderationUserKey } from '~/symbols/moderator'

const props = defineProps<{ id: string }>()

definePageMeta({
  props: true,
})

const { state: user, execute: loadUser } = await useAsyncState(
  () => getUserById(Number(props.id)),
  null,
  { resetOnExecute: false },
)

provide(moderationUserKey, user)

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
  <div class="container">
    <div v-if="user" class="mb-14 flex items-center justify-center gap-8">
      <h1 class="text-lg text-content-100">
        <UserMedia
          :user
          size="xl"
        />
      </h1>

      <div class="flex items-center justify-center gap-2">
        <NuxtLink
          v-for="{ name, label } in links"
          :key="name"
          v-slot="{ isExactActive }"
          :to="{ name }"
        >
          <OButton
            :variant="isExactActive ? 'transparent-active' : 'secondary'"
            size="lg"
            :label
          />
        </NuxtLink>
      </div>
    </div>

    <NuxtPage @update="loadUser" />
  </div>
</template>

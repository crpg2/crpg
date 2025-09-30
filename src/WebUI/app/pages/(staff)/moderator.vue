<script setup lang="ts">
import type { RouteNamedMap } from 'vue-router/auto-routes'

import { ROLE } from '~/models/role'

definePageMeta({
  roles: [ROLE.Admin, ROLE.Moderator],
})

const { t } = useI18n()

const links = [
  {
    name: 'moderator',
    label: t('restriction.title'),
  },
  {
    name: 'moderator-find-user',
    label: t('findUser.title'),
  },
] satisfies Array<{
  name: keyof RouteNamedMap
  label: string
}>
</script>

<template>
  <div class="space-y-8 py-6">
    <nav class="flex items-center justify-center gap-2">
      <NuxtLink
        v-for="{ name, label } in links"
        :key="name"
        v-slot="{ isExactActive }"
        :to="({ name })"
      >
        <UButton
          color="neutral"
          variant="link"
          active-variant="soft"
          active-color="primary"
          :active="isExactActive "
          size="xl"
          :label
        />
      </NuxtLink>
    </nav>

    <NuxtPage />
  </div>
</template>

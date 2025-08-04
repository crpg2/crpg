<script setup lang="ts">
import type { RouteLocationRaw } from 'vue-router'
import type { RouteNamedMap } from 'vue-router/auto-routes'

import { ROLE } from '~/models/role'

definePageMeta({
  roles: [ROLE.Admin, ROLE.Moderator],
})

const { t } = useI18n()

const links: { name: keyof RouteNamedMap, label: string }[] = [
  {
    name: 'moderator',
    label: t('restriction.title'),
  },
  {
    name: 'moderator-find-user',
    label: t('findUser.title'),
  },
]
</script>

<template>
  <div class="py-6">
    <div class="mb-12 flex items-center justify-center gap-2">
      <NuxtLink
        v-for="{ name, label } in links"
        :key="name"
        v-slot="{ isExactActive }"
        :to="({ name } as RouteLocationRaw)"
      >
        <UButton
          color="neutral"
          :active="isExactActive"
          active-variant="soft"
          variant="ghost"
          size="lg"
          :label
        />
      </NuxtLink>
    </div>

    <NuxtPage />
  </div>
</template>

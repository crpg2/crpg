<script setup lang="ts">
import type { RouteLocationRaw } from 'vue-router'
import type { RouteNamedMap } from 'vue-router/auto-routes'

import { Role } from '~/models/role'

definePageMeta({
  roles: [Role.Admin, Role.Moderator],
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
          color="secondary"
          :variant="isExactActive ? 'solid' : 'soft'"
          size="lg"
          :label
        />
      </NuxtLink>
    </div>

    <NuxtPage />
  </div>
</template>

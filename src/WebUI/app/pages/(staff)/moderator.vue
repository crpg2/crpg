<script setup lang="ts">
import type { NavigationMenuItem } from '@nuxt/ui'

import { ROLE } from '~/models/role'

definePageMeta({
  roles: [ROLE.Admin, ROLE.Moderator],
})

const route = useRoute('moderator')

const { t } = useI18n()

const navigationItems = computed<NavigationMenuItem[]>(() => [
  {
    label: t('restriction.title'),
    to: { name: 'moderator' },
    active: route.name === 'moderator', // hack
  },
  {
    label: t('findUser.title'),
    to: { name: 'moderator-find-user' },
  },
])
</script>

<template>
  <div class="space-y-6 py-6">
    <UNavigationMenu
      color="primary"
      class="w-full justify-center"
      :items="navigationItems"
    />
    <NuxtPage />
  </div>
</template>

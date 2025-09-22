<script setup lang="ts">
import type { RouteLocationNormalizedLoaded } from 'vue-router'

import { CLAN_QUERY_KEYS } from '~/queries'
import { getClan } from '~/services/clan-service'

definePageMeta({
  middleware: [
    /**
     * @description basic validate clanId
     */
    async (to) => {
      if (Number.isNaN(Number((to as RouteLocationNormalizedLoaded<'clans-id'>).params.id))) {
        return navigateTo({ name: 'clans' })
      }
    },
  ],
})

const route = useRoute('clans-id')

const { data: clan, execute: loadClan } = useAsyncDataCustom(
  CLAN_QUERY_KEYS.byId(Number(route.params.id)),
  () => getClan(Number(route.params.id)),
  {
    immediate: false,
    poll: false,
    loadingIndicator: false,
  },
)

await loadClan()

if (!clan.value) {
  await navigateTo({ name: 'clans' })
}
</script>

<template>
  <div>
    <NuxtPage />
  </div>
</template>

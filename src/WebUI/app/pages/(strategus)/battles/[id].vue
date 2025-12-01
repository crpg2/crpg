<script setup lang="ts">
import type { RouteLocationNormalizedLoaded } from 'vue-router'

import { BATTLE_QUERY_KEYS } from '~/queries'
import { getBattle } from '~/services/strategus/battle-service'

definePageMeta({
  middleware: [
    /**
     * @description basic validate battleId
     */
    async (to) => {
      const battleId = Number((to as RouteLocationNormalizedLoaded<'battles-id'>).params.id)

      if (Number.isNaN(battleId)) {
        return navigateTo({ name: 'battles' })
      }

      const { data: battle, execute: loadBattle, error } = useAsyncDataCustom(
        BATTLE_QUERY_KEYS.byId(battleId),
        () => getBattle(battleId),
        {
          immediate: false,
          poll: false,
          loadingIndicator: false,
        },
      )

      await loadBattle()

      if (!battle.value || error.value) {
        return navigateTo({ name: 'battles' })
      }
    },
  ],
})
</script>

<template>
  <div>
    <NuxtPage />
  </div>
</template>

<script setup lang="ts">
import type { RouteLocationNormalizedLoaded } from 'vue-router'

import { usePartyState } from '~/composables/strategus/use-party'
import { PARTY_STATUS } from '~/models/strategus/party'
import { MAP_BATTLE_QUERY_KEYS } from '~/queries'
import { getBattle } from '~/services/strategus/battle-service'

definePageMeta({
  middleware: [
    async (to) => {
      const { party } = usePartyState().value

      if (!party.targetedBattle && party.status !== PARTY_STATUS.InBattle) {
        return navigateTo({ name: 'strategus' })
      }

      const battleId = Number((to as RouteLocationNormalizedLoaded<'strategus-battle-id'>).params.id)

      if (Number.isNaN(battleId)) {
        return navigateTo({ name: 'strategus' })
      }

      const { data: battle, error } = await useAsyncData(
        toCacheKey(MAP_BATTLE_QUERY_KEYS.byId(battleId)),
        () => getBattle(battleId),
      )

      if (!battle.value || error.value) {
        return navigateTo({ name: 'strategus' })
      }
    },
  ],
})
</script>

<template>
  <!-- TODO: to cmp -->
  <UCard
    variant="subtle"
    :ui="{
      root: 'bg-elevated',
    }"
  >
    <div class="h-full overflow-x-hidden overflow-y-auto">
      <RouterView />
    </div>

    <footer class="border-border-200 flex items-center gap-5 border-t pt-2">
      TODO:
    </footer>
  </UCard>
</template>

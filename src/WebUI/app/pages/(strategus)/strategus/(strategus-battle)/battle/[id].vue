<script setup lang="ts">
import type { RouteLocationNormalizedLoaded } from 'vue-router'

import { useMapBattle } from '~/composables/strategus/map/use-map-battle'
import { usePartyState } from '~/composables/strategus/use-party'
import { useUser } from '~/composables/user/use-user'
import { PARTY_STATUS } from '~/models/strategus/party'
import { MAP_BATTLE_QUERY_KEYS } from '~/queries'
import { getBattle } from '~/services/strategus/battle-service'

definePageMeta({
  middleware: [
    async (to) => {
      const { partyState } = usePartyState()

      const { party } = partyState.value

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

const { battle, battleTitle } = useMapBattle()

function leaveFromBattle() {
  // remove battle figter
}

const { user } = useUser()
const route = useRoute<'strategus-battle-id'>()

const { t } = useI18n()

const selfCommanderFighter = computed(() => {
  if (battle.value.attacker.commander.party?.id === user.value!.id) {
    return battle.value.attacker.commander
  }
  if (battle.value.defender.commander.party?.id === user.value!.id) {
    return battle.value.defender.commander
  }
  return null
})
</script>

<template>
  <MapSidePage class="min-w-4xl">
    <template #header>
      <UiTextView variant="h2" tag="h1" class="text-center">
        {{ battleTitle }}
      </UiTextView>

      <!-- v-if="selfCommanderFighter?.commander" -->
    </template>

    <div class="space-y-4">
      <BattleSideViewGroup
        :battle
        :can-apply="{
          Attacker: null,
          Defender: null,
        }"
        :can-manage="{
          Attacker: false,
          Defender: false,
        }"
      />

      <UNavigationMenu
        variant="pill"
        color="neutral"
        class="flex w-full justify-center gap-4"
        :items="[
          {
            label: 'Fighters',
            to: { name: 'strategus-battle-id', params: { id: route.params.id } },
            active: route.name === 'strategus-battle-id', //
          },
          {
            label: 'Applications',
            to: { name: 'strategus-battle-id-applications', params: { id: route.params.id } },
          },
          {
            label: 'Inventory',
            to: { name: 'strategus-battle-id-inventory', params: { id: route.params.id } },
          },
        ]"
      />

      <NuxtPage />
    </div>

    <template #footer>
      <div class="flex justify-end">
        <UButton label="Leave" icon="crpg:logout" size="xl" variant="subtle" @click="leaveFromBattle" />
      </div>
    </template>
  </MapSidePage>
</template>

<script setup lang="ts">
import type { NavigationMenuItem } from '@nuxt/ui'
import type { RouteLocationNormalizedLoaded } from 'vue-router'

import { BattleFighterApplicationStatusBadge } from '#components'

import type { BattleSide } from '~/models/strategus/battle'

import { useBattleFighterApplications, useBattleFighterApplicationsProvider, useMapBattle, useMapBattleProvider } from '~/composables/strategus/map/use-map-battle'
import { useParty } from '~/composables/strategus/use-party'
import { useUser } from '~/composables/user/use-user'
import { BATTLE_FIGHTER_APPLICATION_STATUS } from '~/models/strategus/battle'
import { shouldPartyBeInBattle } from '~/services/strategus/party-service'

definePageMeta({
  middleware: [
    async (to) => {
      const { partyState } = useParty()

      const { party } = partyState.value

      if (!shouldPartyBeInBattle(party)) {
        return navigateTo({ name: 'strategus' })
      }

      const battleId = Number((to as RouteLocationNormalizedLoaded<'strategus-battle-id'>).params.id)

      if (Number.isNaN(battleId)) {
        return navigateTo({ name: 'strategus' })
      }

      const { execute, data: battle, error } = useMapBattleProvider(battleId)
      await execute()

      if (!battle.value || error.value) {
        return navigateTo({ name: 'strategus' })
      }

      const { execute: l } = useBattleFighterApplicationsProvider()
      await l()
    },
  ],
})

const { battle, battleTitle } = useMapBattle()

const { partyState, updateParty } = useParty()
const { user } = useUser()
const route = useRoute<'strategus-battle-id'>()
const { t } = useI18n() // TODO:

const selfFighter = computed(() => partyState.value.party.targetedBattle?.fighters.find(f => f.party?.id === partyState.value.party.id))

const selfCommanderFighter = computed(() => {
  if (battle.value.attacker.commander.party?.id === user.value!.id) {
    return battle.value.attacker.commander
  }
  if (battle.value.defender.commander.party?.id === user.value!.id) {
    return battle.value.defender.commander
  }
  return null
})

const { fighterApplications, removeBattleFighterApplication } = useBattleFighterApplications()

const pendingFigterApplications = computed(() => fighterApplications.value.filter(fa => fa.status === BATTLE_FIGHTER_APPLICATION_STATUS.Pending))

const renderBattleFighterApplicationStatusBadge = (side: BattleSide) => {
  if (selfFighter.value?.commander) {
    return null
  }

  const fighterApplication = fighterApplications.value.find(fa => fa.side === side)

  if (!fighterApplication
    /**
     * There may be a case where an application is accepted, but the fighter was kicked out of the battle, but then re-created the application for the other side.
     */
    || (fighterApplication.status === BATTLE_FIGHTER_APPLICATION_STATUS.Accepted && !selfFighter.value)) {
    return null
  }

  return h(BattleFighterApplicationStatusBadge, { status: fighterApplication.status, onDelete: async () => {
    await removeBattleFighterApplication(side)
    await updateParty()
  } })
}

const navigationItems = computed<NavigationMenuItem[]>(() => [
  {
    label: 'Fighters',
    to: { name: 'strategus-battle-id', params: { id: route.params.id } },
    active: route.name === 'strategus-battle-id', // hack, [id].vue conflict with [id]/index.vue
    icon: 'crpg:member',
  },
  {
    label: 'Inventory',
    to: { name: 'strategus-battle-id-inventory', params: { id: route.params.id } },
    icon: 'crpg:chest',
  },
  ...(selfCommanderFighter.value
    ? [
        {
          label: 'Applications',
          to: { name: 'strategus-battle-id-applications', params: { id: route.params.id } },
          ...(pendingFigterApplications.value.length && {
            badge: {
              variant: 'subtle',
              color: 'info',
              label: pendingFigterApplications.value.length,
            },
          }),
        } as NavigationMenuItem,
      ]
    : []),
])
</script>

<template>
  <MapSidePage class="w-4xl">
    <template #header>
      <UiHeading variant="h2" tag="h1" :title="battleTitle" />
    </template>

    <div class="space-y-6">
      <div class="flex flex-wrap items-center justify-center gap-4.5">
        <BattlePhaseBadge :phase="battle.phase" size="lg" />
        <UBadge icon="crpg:region" :label="$t(`region.${battle.region}`, 0)" size="lg" variant="soft" color="neutral" />
      </div>

      <BattleSideViewGroup
        :battle-type="battle.type"
        :defender="battle.defender"
        :attacker="battle.attacker"
      >
        <template #topbar-prepend="{ side }">
          <component :is="renderBattleFighterApplicationStatusBadge(side)" />
        </template>
      </BattleSideViewGroup>

      <UNavigationMenu
        v-if="selfFighter"
        variant="pill"
        color="neutral"
        class="flex w-full justify-center gap-4"
        :items="navigationItems"
      />

      <NuxtPage />
    </div>
  </MapSidePage>
</template>

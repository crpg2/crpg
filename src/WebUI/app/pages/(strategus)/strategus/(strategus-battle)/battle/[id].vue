<script setup lang="ts">
import type { NavigationMenuItem } from '@nuxt/ui'
import type { RouteLocationNormalizedLoaded } from 'vue-router'

import { AppApplicationStatusBadge, UTooltip } from '#components'

import type { BattleSide } from '~/models/strategus/battle'

import { useBattleFighterApplications, useMapBattle } from '~/composables/strategus/map/use-map-battle'
import { usePartyState } from '~/composables/strategus/use-party'
import { useUser } from '~/composables/user/use-user'
import { BATTLE_FIGHTER_APPLICATION_STATUS } from '~/models/strategus/battle'
import { PARTY_STATUS } from '~/models/strategus/party'
import { MAP_BATTLE_QUERY_KEYS } from '~/queries'
import { getBattle, getBattleFighterApplications } from '~/services/strategus/battle-service'

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

      await useAsyncData(
        toCacheKey(MAP_BATTLE_QUERY_KEYS.figterApplicationsById(battleId)),
        () => getBattleFighterApplications(battleId, [
          BATTLE_FIGHTER_APPLICATION_STATUS.Pending,
          BATTLE_FIGHTER_APPLICATION_STATUS.Accepted,
          BATTLE_FIGHTER_APPLICATION_STATUS.Declined,
        ]),
      )
    },
  ],
})

const { battle, battleTitle } = useMapBattle()

const { partyState } = usePartyState()
const { user } = useUser()
const route = useRoute<'strategus-battle-id'>()
const { t } = useI18n() // TODO:

const selfCommanderFighter = computed(() => {
  if (battle.value.attacker.commander.party?.id === user.value!.id) {
    return battle.value.attacker.commander
  }
  if (battle.value.defender.commander.party?.id === user.value!.id) {
    return battle.value.defender.commander
  }
  return null
})

const selfFighter = computed(() => partyState.value.party.targetedBattle?.fighters.find(f => f.party?.id === partyState.value.party.id))

const { fighterApplications } = useBattleFighterApplications()

const pendingFigterApplications = computed(() => fighterApplications.value.filter(fa => fa.status === BATTLE_FIGHTER_APPLICATION_STATUS.Pending))
const selfPendingFigterApplications = computed(() => pendingFigterApplications.value.filter(fa => fa.party.id === partyState.value.party.id))

const renderBattleFighterApplicationStatusBadge = (side: BattleSide) => {
  const fighterApplication = selfPendingFigterApplications.value.find(fa => fa.side === side)

  if (!fighterApplication) {
    return null
  }

  return h(UTooltip, { text: 'Battle fighter application status' }, () => h(AppApplicationStatusBadge, {
    status: fighterApplication.status,
  }))
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

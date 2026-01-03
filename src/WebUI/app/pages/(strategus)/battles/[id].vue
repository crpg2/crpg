<script setup lang="ts">
import type { RouteLocationNormalizedLoaded } from 'vue-router'
import type { RouteNamedMap } from 'vue-router/auto-routes'

import { useBattle } from '~/composables/strategus/battle/use-battle'
import { BATTLE_QUERY_KEYS } from '~/queries'
import { getBattle, getBattleTitle } from '~/services/strategus/battle-service'

definePageMeta({
  layoutOptions: {
    bg: 'background-3.webp',
  },
  middleware: [
    /**
     * @description basic validate battleId
     */
    async (to) => {
      const battleId = Number((to as RouteLocationNormalizedLoaded<'battles-id'>).params.id)

      if (Number.isNaN(battleId)) {
        return navigateTo({ name: 'battles' })
      }

      const { data: battle, error } = await useAsyncData(
        toCacheKey(BATTLE_QUERY_KEYS.byId(battleId)),
        () => getBattle(battleId),
      )

      if (!battle.value || error.value) {
        return navigateTo({ name: 'battles' })
      }
    },
  ],
})

const { t } = useI18n()

const { battle } = useBattle()
const battleTitle = computed(() => getBattleTitle(battle.value))

const nav = [
  {
    name: 'battles-id',
    label: 'Roster',
  },
  {
    name: 'battles-id-manage',
    label: 'Manage',
  },
] satisfies Array<{
  name: keyof RouteNamedMap
  label: string
}>
</script>

<template>
  <UContainer
    class="space-y-8 py-12"
  >
    <AppPageHeaderGroup
      :title="battleTitle"
      decorated
      :back-to="{ name: 'battles' }"
    />

    <div class="mx-auto max-w-xl space-y-6">
      <div class="flex flex-wrap items-center justify-center gap-4.5">
        <BattlePhaseBadge :phase="battle.phase" />
        <UBadge
          v-if="battle.scheduledFor"
          icon="i-lucide-calendar-check"
          :label="$d(battle.scheduledFor, 'short')" size="xl" variant="soft" color="neutral"
        />
        <UBadge icon="crpg:region" :label="$t(`region.${battle.region}`)" size="xl" variant="soft" color="neutral" />
      </div>

      <UiDecorSeparator />

      <BattleSideViewGroup :battle />

      <!-- <BattleSideComparison
        :battle
        :my-side
        can-view-mercenaries
        :attacker-mercenary-count="battleMercenariesAttackers.length"
        :defender-mercenary-count="battleMercenariesDefenders.length"
      /> -->

      <UiDecorSeparator />
    </div>

    <nav v-if="nav.length > 1" class="flex justify-center gap-2">
      <NuxtLink
        v-for="{ name, label } in nav"
        :key="name"
        v-slot="{ isExactActive }"
        :to="({ name, params: { id: battle.id } })"
      >
        <UButton
          color="neutral"
          variant="link"
          active-variant="soft"
          active-color="primary"
          :active="isExactActive"
          size="xl"
          :label
        />
      </NuxtLink>
    </nav>

    <NuxtPage />
  </UContainer>
</template>

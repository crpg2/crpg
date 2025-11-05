<script setup lang="ts">
import type { SelectItem, TabsItem } from '@nuxt/ui'

import { useRouteQuery } from '@vueuse/router'

import type { BattlePhase } from '~/models/strategus/battle'

import { useRegionQuery } from '~/composables/use-region'
import { BATTLE_PHASE } from '~/models/strategus/battle'
import { getBattles, SEARCHABLE_BATTLE_PHASE } from '~/services/strategus/battle-service'

const { regionModel, regions } = useRegionQuery()

const battlePhaseModel = useRouteQuery<BattlePhase[]>('battlePhases', [BATTLE_PHASE.Scheduled, BATTLE_PHASE.Hiring])

const {
  data: battles,
  // execute: loadBattles,
} = useAsyncData(
  () => getBattles(regionModel.value, battlePhaseModel.value),
  {
    default: () => [],
    watch: [regionModel, battlePhaseModel],
  },
)
</script>

<template>
  <UContainer class="space-y-8 py-12">
    <div
      class="mx-auto max-w-4xl"
    >
      <UiHeading class="mb-14" title="Upcoming Battles" />
    </div>
    <div class="mx-auto max-w-2xl space-y-6">
      <div class="mb-4 flex gap-6">
        <UTabs
          v-model="regionModel"
          :items="regions.map<TabsItem>(item => ({
            label: $t(`region.${item}`),
            value: item,
          }))"
          size="xl"
          color="neutral"
          :content="false"
        />

        <USelect
          v-model="battlePhaseModel"
          :items="Object.values(SEARCHABLE_BATTLE_PHASE).map<SelectItem>(item => ({
            // label: $t(`region.${item}`),
            label: item,
            value: item,
          }))"
          multiple
          size="xl"
          color="neutral"
          class="w-32"
          :ui="{
            content: 'w-auto',
          }"
        />
        <!-- <UTabs
          v-model="battlePhaseModel"
          :items="Object.values(SEARCHABLE_BATTLE_PHASE).map<TabsItem>(item => ({
            label: $t(`region.${item}`),
            value: item,
          }))"
          size="xl"
          color="neutral"
          :content="false"
        /> -->

      <!-- <UTabs
          v-model="gameModeModel"
          :items="rankedGameModes.map<TabsItem>(mode => ({
            label: $t(`game-mode.${mode}`),
            icon: `crpg:${gameModeToIcon[mode]}`,
            value: mode,
          }))"
          size="xl"
          color="neutral"
          :content="false"
        /> -->
      </div>

      <div class="space-y-6">
        <BattleEventCard
          v-for="battle in battles"
          :key="battle.id"
          :battle
        />
      </div>
    </div>
  </UContainer>
</template>

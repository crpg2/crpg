<script setup lang="ts">
import type { SelectItem, TabsItem } from '@nuxt/ui'

import { useRouteQuery } from '@vueuse/router'

import type { BattlePhase, BattleType } from '~/models/strategus/battle'

import { useRegionQuery } from '~/composables/use-region'
import { SomeRole } from '~/models/role'
import { BATTLE_PHASE, BATTLE_TYPE } from '~/models/strategus/battle'
import { getBattles, SEARCHABLE_BATTLE_PHASE } from '~/services/strategus/battle-service'

definePageMeta({
  roles: SomeRole,
})

const { regionModel, actualRegions } = useRegionQuery()

const battlePhaseModel = useRouteQuery<BattlePhase[]>('battlePhases', [BATTLE_PHASE.Live, BATTLE_PHASE.Scheduled, BATTLE_PHASE.Hiring])
const battleTypeModel = useRouteQuery<BattleType | 'All'>('battleType', 'All')

const {
  data: battles,
  pending: isLoadingBattles,
} = useAsyncData(
  () => getBattles(
    regionModel.value,
    battlePhaseModel.value,
    battleTypeModel.value !== 'All' ? battleTypeModel.value : undefined,
  ),
  {
    default: () => [],
    watch: [regionModel, battleTypeModel, battlePhaseModel],
  },
)
</script>

<template>
  <UContainer class="space-y-8 py-12">
    <div class="mx-auto max-w-4xl">
      <UiHeading class="mb-14" :title="$t('strategus.battle.upcoming battles')" />
    </div>

    <div class="mx-auto max-w-2xl space-y-6">
      <div class="mb-6 flex justify-between gap-4">
        <UTabs
          v-model="regionModel"
          :items="actualRegions.map<TabsItem>(item => ({
            label: $t(`region.${item}`),
            value: item,
          }))"
          size="xl"
          color="neutral"
          :content="false"
        />

        <div class="flex gap-4">
          <USelect
            v-model="battleTypeModel"
            :items="['All', ...Object.values(BATTLE_TYPE).map<SelectItem>(item => ({
              label: $t(`strategus.battle.type.${item}`),
              value: item,
            })),
            ]"
            size="xl"
            placeholder="Type"
            color="neutral"
            variant="soft"
            :ui="{
              content: 'w-auto',
            }"
          />

          <USelect
            v-model="battlePhaseModel"
            :items="Object.values(SEARCHABLE_BATTLE_PHASE).map<SelectItem>(item => ({
              label: $t(`strategus.battle.phase.${item}`),
              value: item,
            }))"
            multiple
            size="xl"
            placeholder="Phase"
            variant="soft"
            color="neutral"
            class="w-48"
            :ui="{
              content: 'w-auto',
            }"
          />
        </div>
      </div>

      <div v-if="battles.length" class="space-y-7">
        <BattleEventCard
          v-for="battle in battles"
          :key="battle.id"
          :battle
        />
      </div>

      <UCard v-else variant="subtle" :ui="{ body: 'relative min-h-56' }">
        <UiResultNotFound v-if="!isLoadingBattles" />
        <UiLoading v-else active />
      </UCard>
    </div>
  </UContainer>
</template>

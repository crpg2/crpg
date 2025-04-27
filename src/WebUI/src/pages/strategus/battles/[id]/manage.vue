<script setup lang="ts">
import BackButton from '~/components/app/BackButton.vue'
import { useBattleMercenaries } from '~/composables/strategus/battle/use-battle-mercenaries'
import { useBattle } from '~/composables/strategus/battle/use-battles'
import { BattlePhase } from '~/models/strategus/battle'

const props = defineProps<{
  id: string
}>()

definePage({
  meta: {
    layout: 'default',
    middleware: '', // TODO: FIXME: ['canManageBattle']
    roles: ['User', 'Moderator', 'Admin'],
  },
  props: true,
})

const battleId = Number(props.id)

const { battleMercenaries, loadBattleMercenaries } = useBattleMercenaries()
const { battle, loadBattle } = useBattle()
const canManageMercenaries = computed(() => battle.value?.phase === BattlePhase.Hiring)

// TODO:

const fetchPageData = async (battleId: number) => {
  await loadBattle(0, { id: battleId })
  if (canManageMercenaries.value) {
    await loadBattleMercenaries(0, { id: battleId })
  }
}

fetchPageData(Number(props.id))
</script>

<template>
  <div class="p-6">
    <BackButton :to="{ name: 'StrategusBattlesId', params: { id: battleId } }" />

    <div class="mx-auto max-w-2xl space-y-10 py-6">
      <div class="space-y-14">
        <h1 class="text-center text-xl text-content-100">
          {{ $t('strategus.battle.manage.title') }}
        </h1>

        <BattleMercenaryManagement
          v-if="battle && canManageMercenaries"
          :battle
          :battle-mercenaries
          @mercenary-removed="() => loadBattleMercenaries(0, { id: battleId })"
        />
      </div>
    </div>
  </div>
</template>

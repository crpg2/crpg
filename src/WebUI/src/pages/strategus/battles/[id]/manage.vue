<script setup lang="ts">
import { useBattleMercenaries } from '~/composables/strategus/use-battle-mercenaries'
import { useBattle } from '~/composables/strategus/use-battles'

const props = defineProps<{
  id: string
}>()

definePage({
  meta: {
    layout: 'default',
    middleware: '', // TODO: ['canManageBattle']
    roles: ['User', 'Moderator', 'Admin'],
  },
  props: true,
})

const { battleMercenaries, loadBattleMercenaries } = useBattleMercenaries()
const { battle, battleId, loadBattle } = useBattle(props.id)

const fetchPageData = async (battleId: number) => {
  await Promise.all([loadBattle(0, { id: battleId }), loadBattleMercenaries(0, { id: battleId })])
}

await fetchPageData(Number(props.id))
</script>

<template>
  <div class="p-6">
    <RouterLink :to="{ name: 'StrategusBattlesId', params: { id: battleId } }">
      <OButton
        v-tooltip.bottom="$t('nav.back')"
        variant="secondary"
        size="xl"
        outlined
        rounded
        icon-left="arrow-left"
      />
    </RouterLink>

    <div class="mx-auto max-w-2xl space-y-10 py-6">
      <div class="space-y-14">
        <h1 class="text-center text-xl text-content-100">
          {{ $t('strategus.battle.manage.title') }}
        </h1>
        <BattleMercenaryManagement
          v-if="battle"
          :battle
          :battle-mercenaries @mercenary-removed="
            () => {
              loadBattleMercenaries(0, { id: battleId });
            }"
        />
      </div>
    </div>
  </div>
</template>

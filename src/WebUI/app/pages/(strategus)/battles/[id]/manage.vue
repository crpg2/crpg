<script setup lang="ts">
import type { BattleMercenary, BattleMercenaryApplication, BattleSideBriefing } from '~/models/strategus/battle'

import { useBattle, useBattleFighters, useBattleMercenaries, useBattleMercenaryApplications, useBattleSideBriefing } from '~/composables/strategus/battle/use-battle'
import { useUser } from '~/composables/user/use-user'
import { BATTLE_PHASE } from '~/models/strategus/battle'
import { getBattleFighterByUserId } from '~/services/strategus/battle-service'

definePageMeta({
  middleware: [
    /**
     * @description basic validate battleId
     */
    async (to) => {
      const { battle } = useBattle()

      // console.log('battle', battle.value)

      // const battleId = Number((to as RouteLocationNormalizedLoaded<'battles-id'>).params.id)

      // if (Number.isNaN(battleId)) {
      //   return navigateTo({ name: 'battles' })
      // }

      // const { data: battle, error } = await useAsyncData(
      //   toCacheKey(BATTLE_QUERY_KEYS.byId(battleId)),
      //   () => getBattle(battleId),
      // )

    // if (!battle.value || error.value) {
    //   return navigateTo({ name: 'battles' })
    // }
    },
  ],
})
const { t } = useI18n()
const toast = useToast()
const { user } = useUser()
const { battle } = useBattle()

const selfFighter = computed(() => getBattleFighterByUserId([
  battle.value.attacker.fighter,
  battle.value.defender.fighter,
], user.value!.id)!)

const battleSideDetail = computed(() => selfFighter.value.side === 'Attacker' ? battle.value.attacker : battle.value.defender)

const { updateBattleBriefing, updatingBattleBriefing } = useBattleSideBriefing()

const onSaveBattleBriefing = async (value: BattleSideBriefing) => {
  await updateBattleBriefing(battle.value.id, selfFighter.value.side, value)
}

const {
  mercenaryApplications,
  mercenaryApplicationsCount,
  loadBattleMercenaryApplications,
  respondToBattleMercenaryApplication,
} = useBattleMercenaryApplications()

const [onRespond, responding] = useAsyncCallback(
  async (applicationId: number, status: boolean) => {
    await respondToBattleMercenaryApplication(applicationId, status)
    await loadBattleMercenaryApplications()
    toast.add({
      title: status
        ? 'TODO:'
        : 'TODO:',
      close: false,
      color: 'success',
    })
  },
)
</script>

<template>
  <div class="mx-auto max-w-3xl">
    <BattleSideBriefingForm :briefing="battleSideDetail.briefing" @save="onSaveBattleBriefing" />

    <BattleSideMercenaryApplications :mercenary-applications @respond="onRespond" />
    <!-- <UiTextView variant="h3" class="mb-6 text-center">
      Mercenaries
    </UiTextView> -->
  </div>
</template>

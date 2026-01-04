<script setup lang="ts">
import type { RouteLocationNormalizedLoaded } from 'vue-router'

import { LazyBattleManageDrawer, LazyBattleMercenaryApplicationDialog } from '#components'

import type { BattleSide } from '~/models/strategus/battle'

import { useBattle, useBattleFighters, useBattleMercenaries, useBattleMercenaryApplication } from '~/composables/strategus/battle/use-battle'
import { useUser } from '~/composables/user/use-user'
import { BATTLE_QUERY_KEYS } from '~/queries'
import {
  getBattle,
  getBattleFighterByUserId,
  getBattleTitle,
} from '~/services/strategus/battle-service'

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

const { battle, refreshBattle } = useBattle()
const { battleFighters } = useBattleFighters()
const { editBattleApplication, editingBattleApplication, removeBattleApplication, removingBattleApplication } = useBattleMercenaryApplication()

const {
  loadBattleMercenaries,
  loadingBattleMercenaries,
  battleMercenariesAttackers,
  battleMercenariesDefenders,
} = useBattleMercenaries()

const battleTitle = computed(() => getBattleTitle(battle.value))

const { user } = useUser()

const selfFighter = computed(() => getBattleFighterByUserId(battleFighters.value, user.value!.id))

const overlay = useOverlay()

const battleManageDrawer = overlay.create(LazyBattleManageDrawer)

const battleMercenaryApplicationDialog = overlay.create(LazyBattleMercenaryApplicationDialog)

const onApplyToBattleAsMercenary = (side: BattleSide) => {
  battleMercenaryApplicationDialog.open({
    side,
    sideInfo: side === 'Attacker' ? battle.value.attacker : battle.value.defender,
    onApply: async (value) => {
      await editBattleApplication(battle.value.id, {
        side,
        // characterId: value.characterId, // TODO: FIXME:
        characterId: 6,
        note: value.note,
        wage: value.wage,
      })
      await refreshBattle()
      battleMercenaryApplicationDialog.close()
    },
    onDelete: async () => {
      await removeBattleApplication(battle.value.id, side)
      await refreshBattle()
      battleMercenaryApplicationDialog.close()
    },
  })
}
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

      <BattleSideViewGroup
        :battle
        :can-apply="!selfFighter"
        @open-application="onApplyToBattleAsMercenary"
      />

      <UiDecorSeparator />

      <!-- <BattleSideComparison
        :battle
        :my-side
        can-view-mercenaries
        :attacker-mercenary-count="battleMercenariesAttackers.length"
        :defender-mercenary-count="battleMercenariesDefenders.length"
      /> -->
    </div>

    <div class="mx-auto max-w-5xl">
      <div class="grid grid-cols-2 gap-20">
        <BattleSideMercenariesTable
          :battle-mercenaries="battleMercenariesAttackers"
          :loading="loadingBattleMercenaries"
        />

        <BattleSideMercenariesTable
          :battle-mercenaries="battleMercenariesDefenders"
          :loading="loadingBattleMercenaries"
        />
      </div>
    </div>
  </UContainer>
</template>

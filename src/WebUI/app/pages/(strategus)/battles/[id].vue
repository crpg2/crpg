<script setup lang="ts">
import type { RouteLocationNormalizedLoaded } from 'vue-router'

import { LazyBattleManageDrawer, LazyBattleMercenaryApplicationDialog } from '#components'

import type { BattleParticipant, BattleSide } from '~/models/strategus/battle'

import { useBattle, useBattleMercenaryApplication, useBattleParticipants } from '~/composables/strategus/battle/use-battle'
import { BATTLE_MERCENARY_APPLICATION_STATUS, BATTLE_PARTICIPANT_TYPE, BATTLE_SIDE } from '~/models/strategus/battle'
import { BATTLE_QUERY_KEYS } from '~/queries'
import { getBattle,
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

const { battle, refreshBattle, battleTitle } = useBattle()

const { createBattleApplication, removeBattleApplication } = useBattleMercenaryApplication()

const {
  battleParticipantAttackers,
  battleParticipantDefenders,
  loadBattleParticipants,
  loadingBattleParticipants,
  removeBattleParticipant,
  removingBattleParticipant,
  selfBattleParticipant,
  isSelfBattleParticipantCaptain,
} = useBattleParticipants()

const overlay = useOverlay()

const getSideInfo = (side: BattleSide) => side === BATTLE_SIDE.Attacker ? battle.value.attacker : battle.value.defender

const openManageDrawer = (side: BattleSide, mercenaryApplicationId?: number) => {
  const { open, patch } = overlay.create(LazyBattleManageDrawer, {
    props: {
      battleId: battle.value.id,
      side,
      sideInfo: getSideInfo(side),
      participantCount: side === BATTLE_SIDE.Attacker
        ? battleParticipantAttackers.value.length
        : battleParticipantDefenders.value.length,
      mercenaryApplicationId,
      async onResponded() {
        await Promise.all([
          refreshBattle(),
          loadBattleParticipants(),
        ])
        patch({
          participantCount: side === BATTLE_SIDE.Attacker
            ? battleParticipantAttackers.value.length
            : battleParticipantDefenders.value.length,
        })
      },
    },
  })
  open()
}

const openBattleAsMercenaryDialog = (side: BattleSide) => {
  const { open, close } = overlay.create(LazyBattleMercenaryApplicationDialog)
  open({
    side,
    sideInfo: getSideInfo(side),
    async onApply(value) {
      await createBattleApplication({
        side,
        // characterId: value.characterId, // TODO: FIXME:
        characterId: 6,
        note: value.note,
        wage: value.wage,
      })
      close()
    },
    async onDeleteApplication() {
      await removeBattleApplication(side)
      close()
    },
    async onLeaveFromBattle() {
      if (!selfBattleParticipant.value) {
        return
      }
      await removeBattleParticipant(selfBattleParticipant.value.id)
      close()
    },
  })
}

interface ApplyState {
  disabled: {
    reason: string
  } | null
}

const checkCanApply = (side: BattleSide): ApplyState | null => {
  if (isSelfBattleParticipantCaptain.value) {
    return null
  }

  const currentSideStatus = getSideInfo(side).mercenaryApplication?.status
  const otherSideStatus = getSideInfo(side === BATTLE_SIDE.Attacker ? BATTLE_SIDE.Defender : BATTLE_SIDE.Attacker).mercenaryApplication?.status

  if (otherSideStatus === BATTLE_MERCENARY_APPLICATION_STATUS.Accepted) {
    return {
      disabled: {
        reason: 'First leave the fight for the other side.',
      },
    }
  }

  if (!currentSideStatus) {
    return { disabled: null }
  }

  return null
}

const checkCanManage = (side: BattleSide) => Boolean(isSelfBattleParticipantCaptain.value && selfBattleParticipant.value?.side === side)

const canManageAttackerSide = computed(() => checkCanManage(BATTLE_SIDE.Attacker))
const canManageDefenderSide = computed(() => checkCanManage(BATTLE_SIDE.Defender))

const checkCanManageParticipant = (participant: BattleParticipant) => participant.type !== BATTLE_PARTICIPANT_TYPE.Party
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
        :can-apply="{
          Attacker: checkCanApply(BATTLE_SIDE.Attacker),
          Defender: checkCanApply(BATTLE_SIDE.Defender),
        }"
        :can-manage="{
          Attacker: canManageAttackerSide,
          Defender: canManageDefenderSide,
        }"
        @open-mercenary-application="openBattleAsMercenaryDialog"
        @open-manage="openManageDrawer"
      />

      <BattleSideComparison :battle />
    </div>

    <div class="grid grid-cols-2 gap-8">
      <BattleRosterTable
        v-for="side in [BATTLE_SIDE.Attacker, BATTLE_SIDE.Defender]"
        :key="side"
        :participants="side === BATTLE_SIDE.Attacker ? battleParticipantAttackers : battleParticipantDefenders"
        :loading="loadingBattleParticipants || removingBattleParticipant"
        :can-manage="side === BATTLE_SIDE.Attacker ? canManageAttackerSide : canManageDefenderSide"
        :can-manage-by-participant="checkCanManageParticipant"
        :total-participant-slots="side === BATTLE_SIDE.Attacker ? battle.attacker.totalParticipantSlots : battle.defender.totalParticipantSlots"
        @kick-participant="removeBattleParticipant"
        @show-mercinary-application="(mercinaryApplicationId) => openManageDrawer(side, mercinaryApplicationId)"
      />
    </div>
  </UContainer>
</template>

import type { MaybeRefOrGetter } from 'vue'

import { getAsyncData, refreshAsyncData, useRoute } from '#imports'

import type { Battle, BattleMercenaryApplicationCreation, BattleSide } from '~/models/strategus/battle'

import { useUser } from '~/composables/user/use-user'
import { BATTLE_FIGHTER_APPLICATION_STATUS, BATTLE_MERCENARY_APPLICATION_STATUS, BATTLE_PARTICIPANT_TYPE, BATTLE_SIDE } from '~/models/strategus/battle'
import { BATTLE_QUERY_KEYS } from '~/queries'
import {
  removeBattleParticipant as _removeBattleParticipant,
  respondToBattleMercenaryApplication as _respondToBattleMercenaryApplication,
  applyToBattleAsMercenary,
  getBattleFighterApplications,
  getBattleMercenaryApplications,
  getBattleParticipants,
  removeBattleMercenaryApplication,
  updateBattleSideBriefing,
} from '~/services/strategus/battle-service'

export const useBattleTitle = (battle: MaybeRefOrGetter) => {
  const { t } = useI18n()

  if (toValue(battle).type === 'Siege') {
    return t('strategus.battle.titleByType.Siege', { settlement: toValue(battle).nearestSettlement.name })
  }

  return t('strategus.battle.titleByType.Battle', { nearestSettlement: toValue(battle).nearestSettlement.name, terrain: toValue(battle).terrain.type })
}

export const useBattle = () => {
  const route = useRoute('battles-id')
  const _key = BATTLE_QUERY_KEYS.byId(Number(route.params.id))

  const battle = getAsyncData<Battle>(_key)
  const refreshBattle = refreshAsyncData(_key)

  const battleTitle = useBattleTitle(battle)

  return {
    battle,
    refreshBattle,

    battleTitle,
  }
}

export const useBattleSideBriefing = () => {
  const [updateBattleBriefing, updatingBattleBriefing] = useAsyncCallback(updateBattleSideBriefing, {
    successMessage: 'TODO:',
  })

  return {
    updateBattleBriefing,
    updatingBattleBriefing,
  }
}

// export const useBattleFighters = (immediate = true) => {
//   const { battle } = useBattle()
//   const { user } = useUser()

//   const {
//     state: battleFighters,
//     executeImmediate: loadBattleFighters,
//   } = useAsyncState(
//     () => getBattleFighters(battle.value.id),
//     [],
//     { immediate, resetOnExecute: false },
//   )

//   const battleFightersCount = computed(() => battleFighters.value.length)

//   const selfFighter = computed(() => getBattleFighterByUserId(battleFighters.value, user.value!.id))

//   return {
//     battleFighters,
//     battleFightersCount,
//     loadBattleFighters,
//     selfFighter,
//   }
// }

export const useBattleFighterApplications = (immediate = true) => {
  const { battle } = useBattle()

  const {
    state: fighterApplications,
    executeImmediate: loadBattleFighterApplications,
  } = useAsyncState(
    () => getBattleFighterApplications(battle.value.id, [BATTLE_FIGHTER_APPLICATION_STATUS.Pending]),
    [],
    { immediate, resetOnExecute: false },
  )

  const fighterApplicationsCount = computed(() => fighterApplications.value.length)

  return {
    fighterApplications,
    fighterApplicationsCount,
    loadBattleFighterApplications,
  }
}

export const useBattleParticipants = (immediate = true) => {
  const { battle, refreshBattle } = useBattle()
  const { user } = useUser()

  const {
    state: battleParticipants,
    executeImmediate: loadBattleParticipants,
    isLoading: loadingBattleParticipants,
  } = useAsyncState(
    () => getBattleParticipants(battle.value.id),
    [],
    { immediate, resetOnExecute: false },
  )

  const battleParticipantAttackers = computed(() => battleParticipants.value.filter(p => p.side === BATTLE_SIDE.Attacker))

  const battleParticipantDefenders = computed(() => battleParticipants.value.filter(p => p.side === BATTLE_SIDE.Defender))

  const [removeBattleParticipant, removingBattleParticipant] = useAsyncCallback(async (participantId: number) => {
    await _removeBattleParticipant(battle.value.id, participantId)
    await Promise.all([
      loadBattleParticipants(),
      refreshBattle(),
    ])
  }, {
    successMessage: 'TODO:',
  })

  const selfBattleParticipant = computed(() => battleParticipants.value.find(p => p.user.id === user.value!.id) ?? null)

  const isSelfBattleParticipantCaptain = computed(() => selfBattleParticipant.value?.type === BATTLE_PARTICIPANT_TYPE.Party)

  return {
    battleParticipants,
    battleParticipantAttackers,
    battleParticipantDefenders,
    loadBattleParticipants,
    loadingBattleParticipants,

    removeBattleParticipant,
    removingBattleParticipant,

    selfBattleParticipant,
    isSelfBattleParticipantCaptain,
  }
}

export const useBattleMercenaryApplications = (immediate = true) => {
  const { battle } = useBattle()

  const {
    state: mercenaryApplications,
    executeImmediate: loadBattleMercenaryApplications,
    isLoading: loadingBattleMercenaryApplications,
    isReady: loadedBattleMercenaryApplications,
  } = useAsyncState(
    () => getBattleMercenaryApplications(battle.value.id, [
      BATTLE_MERCENARY_APPLICATION_STATUS.Pending,
      BATTLE_MERCENARY_APPLICATION_STATUS.Accepted,
      BATTLE_MERCENARY_APPLICATION_STATUS.Declined,
    ]),
    [],
    { immediate, resetOnExecute: false },
  )

  const mercenaryApplicationsCount = computed(() => mercenaryApplications.value.length)

  const mercenaryPendingApplicationsCount = computed(() => mercenaryApplications.value.filter(a => a.status === 'Pending').length)

  const respondToBattleMercenaryApplication = (invitationId: number, accept: boolean) => _respondToBattleMercenaryApplication(battle.value.id, invitationId, accept)

  return {
    mercenaryApplications,
    mercenaryApplicationsCount,
    mercenaryPendingApplicationsCount,
    loadBattleMercenaryApplications,
    respondToBattleMercenaryApplication,
    loadingBattleMercenaryApplications,
    loadedBattleMercenaryApplications,
  }
}

export const useBattleMercenaryApplication = () => {
  const { battle, refreshBattle } = useBattle()

  const [createBattleApplication, creatingBattleApplication] = useAsyncCallback(async (
    payload: BattleMercenaryApplicationCreation,
  ) => {
    await applyToBattleAsMercenary(battle.value.id, payload)
    await refreshBattle()
  }, {
    successMessage: 'TODO:',
  })

  const [removeBattleApplication, removingBattleApplication] = useAsyncCallback(async (
    side: BattleSide,
  ) => {
    await removeBattleMercenaryApplication(battle.value.id, side)
    await refreshBattle()
  }, {
    successMessage: 'TODO:',
  })

  return {
    createBattleApplication,
    creatingBattleApplication,

    removeBattleApplication,
    removingBattleApplication,
  }
}

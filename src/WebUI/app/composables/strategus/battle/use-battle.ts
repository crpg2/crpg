import type { MaybeRefOrGetter } from 'vue'

import { getAsyncData, refreshAsyncData, useRoute } from '#imports'

import type { Battle, BattleMercenaryApplicationCreation, BattleSide } from '~/models/strategus/battle'

import { useUser } from '~/composables/user/use-user'
import { BATTLE_MERCENARY_APPLICATION_STATUS, BATTLE_PARTICIPANT_TYPE, BATTLE_SIDE, BATTLE_TYPE } from '~/models/strategus/battle'
import { BATTLE_QUERY_KEYS } from '~/queries'
import {
  removeBattleParticipant as _removeBattleParticipant,
  respondToBattleMercenaryApplication as _respondToBattleMercenaryApplication,
  applyToBattleAsMercenary,
  getBattleMercenaryApplications,
  getBattleParticipants,
  removeBattleMercenaryApplication,
  updateBattleSideBriefing,
} from '~/services/strategus/battle-service'

export const useBattleTitle = (battle: MaybeRefOrGetter<Battle>) => {
  const { $i18n } = useNuxtApp() // I didn't use i18n because it works outside of setup, for example in route middleware

  if (toValue(battle).type === BATTLE_TYPE.Siege) {
    return $i18n.t('strategus.battle.titleByType.Siege', { settlement: toValue(battle).nearestSettlement?.name ?? 'TODO:' })
  }

  return $i18n.t('strategus.battle.titleByType.Battle', {
    nearestSettlement: toValue(battle).nearestSettlement?.name ?? 'TODO:',
    terrain: $i18n.t(`strategus.terrainType.${toValue(battle).terrain.type}`).toLowerCase(),
  })
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
  const { t } = useI18n()

  const [updateBattleBriefing, updatingBattleBriefing] = useAsyncCallback(updateBattleSideBriefing, {
    successMessage: t('strategus.battle.manage.briefing.notify.success'),
  })

  return {
    updateBattleBriefing,
    updatingBattleBriefing,
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

  const mercenaryPendingApplicationsCount = computed(() => mercenaryApplications.value.filter(a => a.status === BATTLE_MERCENARY_APPLICATION_STATUS.Pending).length)

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

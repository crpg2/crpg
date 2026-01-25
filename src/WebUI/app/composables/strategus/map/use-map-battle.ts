import { getAsyncData, refreshAsyncData, useRoute } from '#imports'

import type { Battle } from '~/models/strategus/battle'

import { useUser } from '~/composables/user/use-user'
import { BATTLE_FIGHTER_APPLICATION_STATUS, BATTLE_MERCENARY_APPLICATION_STATUS, BATTLE_SIDE } from '~/models/strategus/battle'
import { MAP_BATTLE_QUERY_KEYS } from '~/queries'
import {
  removeBattleFighter as _removeBattleFighter,
  respondToBattleFighterApplication as _respondToBattleFighterApplication,
  getBattleFighterApplications,
  getBattleFighters,
  getBattleItems,
} from '~/services/strategus/battle-service'

import { useBattleTitle } from '../battle/use-battle'

export const useMapBattle = () => {
  const route = useRoute('strategus-battle-id')
  const _key = MAP_BATTLE_QUERY_KEYS.byId(Number(route.params.id))

  const battle = getAsyncData<Battle>(_key)
  const refreshBattle = refreshAsyncData(_key)

  const battleTitle = useBattleTitle(battle)

  return {
    battle,
    refreshBattle,

    battleTitle,
  }
}

export const useBattleFighterApplications = (immediate = true) => {
  const { battle } = useMapBattle()

  const {
    state: fighterApplications,
    executeImmediate: loadBattleFighterApplications,
    isLoading: loadingBattleFighterApplications,
  } = useAsyncState(
    () => getBattleFighterApplications(battle.value.id, [
      BATTLE_FIGHTER_APPLICATION_STATUS.Pending,
      BATTLE_MERCENARY_APPLICATION_STATUS.Accepted,
      BATTLE_MERCENARY_APPLICATION_STATUS.Declined,
    ]),
    [],
    { immediate, resetOnExecute: false },
  )

  const fighterApplicationsCount = computed(() => fighterApplications.value.length)

  const respondToBattleFighterApplication = (invitationId: number, accept: boolean) => _respondToBattleFighterApplication(battle.value.id, invitationId, accept)

  return {
    fighterApplications,
    fighterApplicationsCount,
    loadBattleFighterApplications,
    respondToBattleFighterApplication,
    loadingBattleFighterApplications,
  }
}

export const useBattleFighters = (immediate = true) => {
  const { battle, refreshBattle } = useMapBattle()
  const { user } = useUser()

  const {
    state: battleFighters,
    executeImmediate: loadBattleFighters,
    isLoading: loadingBattleFighters,
  } = useAsyncState(
    () => getBattleFighters(battle.value.id),
    [],
    { immediate, resetOnExecute: false },
  )

  const battleFightersCount = computed(() => battleFighters.value.length)

  const selfBattleFighter = computed(() => battleFighters.value.find(f => f.party?.user.id === user.value!.id) ?? null)

  const isSelfBattleFighterCommander = computed(() => selfBattleFighter.value?.commander)

  const battleFighterAttackers = computed(() => battleFighters.value.filter(f => f.side === BATTLE_SIDE.Attacker))

  const battleFighterDefenders = computed(() => battleFighters.value.filter(f => f.side === BATTLE_SIDE.Defender))

  const [removeBattleFigter, removingBattleFigter] = useAsyncCallback(async (fighterId: number) => {
    await _removeBattleFighter(battle.value.id, fighterId)
    await Promise.all([
      loadBattleFighters(),
      refreshBattle(),
    ])
  }, {
    successMessage: 'TODO:',
  })

  return {
    battleFighters,
    battleFighterAttackers,
    battleFighterDefenders,
    battleFightersCount,
    loadBattleFighters,
    loadingBattleFighters,
    selfBattleFighter,
    isSelfBattleFighterCommander,
    removeBattleFigter,
    removingBattleFigter,
  }
}

export const useBattleItems = (immediate = true) => {
  const { battle } = useMapBattle()

  const {
    state: battleItems,
    executeImmediate: loadBattleItems,
    isLoading: loadingBattleItems,
  } = useAsyncState(
    () => getBattleItems(battle.value.id),
    [],
    { immediate, resetOnExecute: false },
  )

  return {
    battleItems,
    loadBattleItems,
    loadingBattleItems,
  }
}

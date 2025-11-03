import { getAsyncData, refreshAsyncData, useRoute } from '#imports'

import type { Battle } from '~/models/strategus/battle'

import { BATTLE_FIGHTER_APPLICATION_STATUS, BATTLE_MERCENARY_APPLICATION_STATUS, BATTLE_SIDE } from '~/models/strategus/battle'
import { BATTLE_QUERY_KEYS } from '~/queries'
import { getBattleFighterApplications, getBattleFighters, getBattleMercenaries, getBattleMercenaryApplications } from '~/services/strategus/battle-service'

export const useBattle = () => {
  const route = useRoute('battles-id')
  const _key = BATTLE_QUERY_KEYS.byId(Number(route.params.id))

  const battle = getAsyncData<Battle>(_key)
  const refreshBattle = refreshAsyncData(_key)

  return {
    battle,
    refreshBattle,
  }
}

export const useBattleFighters = (immediate = true) => {
  const { battle } = useBattle()

  const {
    state: battleFighters,
    executeImmediate: loadBattleFighters,
  } = useAsyncState(
    () => getBattleFighters(battle.value.id),
    [],
    { immediate, resetOnExecute: false },
  )

  const battleFightersCount = computed(() => battleFighters.value.length)

  return {
    battleFighters,
    battleFightersCount,
    loadBattleFighters,
  }
}

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

export const useBattleMercenaries = (immediate = true) => {
  const { battle } = useBattle()

  const {
    state: battleMercenaries,
    executeImmediate: loadBattleMercenaries,
  } = useAsyncState(
    () => getBattleMercenaries(battle.value.id),
    [],
    { immediate, resetOnExecute: false },
  )

  const battleMercenariesCount = computed(() => battleMercenaries.value.length)

  const battleMercenariesAttackers = computed(() =>
    battleMercenaries.value.filter(mercenary => mercenary.side === BATTLE_SIDE.Attacker))

  const battleMercenariesDefenders = computed(() =>
    battleMercenaries.value.filter(mercenary => mercenary.side === BATTLE_SIDE.Defender))

  return {
    battleMercenaries,
    battleMercenariesCount,
    battleMercenariesAttackers,
    battleMercenariesDefenders,
    loadBattleMercenaries,
  }
}

export const useBattleMercenaryApplications = (immediate = true) => {
  const { battle } = useBattle()

  const {
    state: mercenaryApplications,
    executeImmediate: loadBattleMercenaryApplications,
  } = useAsyncState(
    () => getBattleMercenaryApplications(battle.value.id, [BATTLE_MERCENARY_APPLICATION_STATUS.Pending]),
    [],
    { immediate, resetOnExecute: false },
  )

  const mercenaryApplicationsCount = computed(() => mercenaryApplications.value.length)

  return {
    mercenaryApplications,
    mercenaryApplicationsCount,
    loadBattleMercenaryApplications,
  }
}

import { getAsyncData, refreshAsyncData, useRoute } from '#imports'

import type { Battle, BattleFighter, BattleFighterApplication, BattleSide } from '~/models/strategus/battle'

import { useBattleTitle } from '~/composables/strategus/battle/use-battle'
import { useUser } from '~/composables/user/use-user'
import { BATTLE_FIGHTER_APPLICATION_STATUS, BATTLE_MERCENARY_APPLICATION_STATUS, BATTLE_SIDE } from '~/models/strategus/battle'
import { MAP_BATTLE_QUERY_KEYS } from '~/queries'
import {
  removeBattleFighter as _removeBattleFighter,
  removeBattleFighterApplication as _removeBattleFighterApplication,
  respondToBattleFighterApplication as _respondToBattleFighterApplication,
  getBattle,
  getBattleFighterApplications,
  getBattleFighters,
  getBattleItems,
} from '~/services/strategus/battle-service'

export const useMapBattleProvider = (battleId: number) => {
  console.log('battleId', battleId)

  return useAsyncDataCustom(
    MAP_BATTLE_QUERY_KEYS.byId(battleId),
    () => getBattle(battleId),
    {
      default: () => [],
      poll: true,
      loadingIndicator: true,
    },
  )
}

export const useMapBattle = () => {
  const route = useRoute('strategus-battle-id')
  const _key = MAP_BATTLE_QUERY_KEYS.byId(Number(route.params.id))

  const battle = getAsyncData<Battle>(_key) // TODO: Battle -> MapDetailBattle
  const refreshBattle = refreshAsyncData(_key)

  const battleTitle = useBattleTitle(battle)

  return {
    battle,
    refreshBattle,

    battleTitle,
  }
}

export const useBattleFighterApplicationsProvider = () => {
  const { battle } = useMapBattle()
  console.log(battle.value.id)

  return useAsyncDataCustom(
    MAP_BATTLE_QUERY_KEYS.figterApplicationsById(battle.value.id),
    () => getBattleFighterApplications(battle.value.id, [
      BATTLE_FIGHTER_APPLICATION_STATUS.Pending,
      BATTLE_FIGHTER_APPLICATION_STATUS.Accepted,
      BATTLE_FIGHTER_APPLICATION_STATUS.Declined,
    ]),
    {
      default: () => [],
      poll: true,
      loadingIndicator: true,
    },
  )
}

export const useBattleFighterApplications = () => {
  const { battle } = useMapBattle()

  const _key = MAP_BATTLE_QUERY_KEYS.figterApplicationsById(battle.value.id)
  const fighterApplications = getAsyncData<BattleFighterApplication[]>(_key)
  const refreshFighterApplications = refreshAsyncData(_key)

  const fighterApplicationsCount = computed(() => fighterApplications.value.length)

  const respondToBattleFighterApplication = (invitationId: number, accept: boolean) => _respondToBattleFighterApplication(battle.value.id, invitationId, accept)

  const [removeBattleFighterApplication, removingBattleFighterApplication] = useAsyncCallback(async (
    side: BattleSide,
  ) => {
    await _removeBattleFighterApplication(battle.value.id, side)
    await refreshFighterApplications()
  })

  return {
    fighterApplications,
    fighterApplicationsCount,
    refreshFighterApplications,
    respondToBattleFighterApplication,
    removeBattleFighterApplication,
    removingBattleFighterApplication,
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

  const [removeBattleFigter, removingBattleFigter] = useAsyncCallback(async (fighterId: number, cause: 'kick' | 'leave') => {
    await _removeBattleFighter(battle.value.id, fighterId)

    if (cause === 'kick') {
      await Promise.all([
        loadBattleFighters(),
        refreshBattle(),
      ])
    }
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

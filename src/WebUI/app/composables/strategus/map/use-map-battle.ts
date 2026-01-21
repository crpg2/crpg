import { getAsyncData, refreshAsyncData, useRoute } from '#imports'

import type { Battle } from '~/models/strategus/battle'

import { useUser } from '~/composables/user/use-user'
import { BATTLE_FIGHTER_APPLICATION_STATUS, BATTLE_MERCENARY_APPLICATION_STATUS } from '~/models/strategus/battle'
import { MAP_BATTLE_QUERY_KEYS } from '~/queries'
import {
  respondToBattleFighterApplication as _respondToBattleFighterApplication,
  getBattleFighterApplications,
  getBattleFighters,
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
  }
}

// TODO:  FIXME: не нужно?
export const useBattleFighters = (immediate = true) => {
  const { battle } = useMapBattle()
  const { user } = useUser()

  const {
    state: battleFighters,
    executeImmediate: loadBattleFighters,
  } = useAsyncState(
    () => getBattleFighters(battle.value.id),
    [],
    { immediate, resetOnExecute: false },
  )

  const battleFightersCount = computed(() => battleFighters.value.length)

  const selfFighter = computed(() => battleFighters.value.find(f => f.party?.user.id === user.value!.id) ?? null)

  return {
    battleFighters,
    battleFightersCount,
    loadBattleFighters,
    selfFighter,
  }
}

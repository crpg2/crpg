import {
  getBattles as _getBattles,
  deleteBattlesByBattleIdMercenariesByMercenaryId,
  deleteBattlesByBattleIdMercenaryApplications,
  getBattlesByBattleId,
  getBattlesByBattleIdFighterApplications,
  getBattlesByBattleIdFighters,
  getBattlesByBattleIdMercenaries,
  getBattlesByBattleIdMercenaryApplications,
  postBattlesByBattleIdMercenaryApplications,
  putBattlesByBattleIdMercenaryApplicationsByApplicationIdResponse,
} from '#api/sdk.gen'

import type { Region } from '~/models/region'
import type { Battle, BattleFighter, BattleFighterApplicationStatus, BattleMercenary, BattleMercenaryApplicationCreation, BattleMercenaryApplicationStatus, BattlePhase, BattleType } from '~/models/strategus/battle'

import { BATTLE_PHASE, BATTLE_TYPE } from '~/models/strategus/battle'

// need a name
export const SEARCHABLE_BATTLE_PHASE = [
  BATTLE_PHASE.Scheduled,
  BATTLE_PHASE.Hiring,
  BATTLE_PHASE.End,
  BATTLE_PHASE.Live,
]

export const battleIconByType: Record<BattleType, string> = {
  [BATTLE_TYPE.Battle]: 'game-mode-battle',
  [BATTLE_TYPE.Siege]: 'game-mode-conquest',
}

export const getBattles = async (
  region: Region,
  phases: BattlePhase[],
  type?: BattleType,
): Promise<Battle[]> => (await _getBattles({ query: { region, 'phase[]': phases, type } })).data! as Battle[] // TODO:

export const getBattle = async (
  battleId: number,
) => (await getBattlesByBattleId({ path: { battleId } })).data

export const getBattleTitle = (battle: Battle) => {
  const { t } = useI18n()

  if (battle.type === 'Siege' && battle.defender?.settlement) {
    return t('strategus.battle.titleByType.Siege', { settlement: battle.defender.settlement.name })
  }

  if (battle.type === 'Battle') {
    return t('strategus.battle.titleByType.Battle', {
      nearestSettlement: 'nearestSettlement', // TODO: get nearest settlement to point
      terrain: 'terrain', // TODO: terrain service get terrain at point
    })
  }

  return 'TODO: FIXME:'
}

export const getBattleFighters = async (
  battleId: number,
): Promise<BattleFighter[]> => (await getBattlesByBattleIdFighters({ path: { battleId } })).data!

export const getBattleFighterByUserId = (
  battleFighters: BattleFighter[],
  userId: number,
) => battleFighters.find(f => (f.party?.user.id || f.settlement?.owner) === userId) || null

export const getBattleFighterApplications = async (
  battleId: number,
  statuses: BattleFighterApplicationStatus[],
) => (await getBattlesByBattleIdFighterApplications({ path: { battleId }, query: { 'status[]': statuses } })).data!

export const getBattleMercenaryApplications = async (
  battleId: number,
  statuses: BattleMercenaryApplicationStatus[],
) => (await getBattlesByBattleIdMercenaryApplications({ path: { battleId }, query: { 'status[]': statuses } })).data!

export const applyToBattleAsMercenary = (
  battleId: number,
  payload: BattleMercenaryApplicationCreation,
) => postBattlesByBattleIdMercenaryApplications({ path: { battleId }, body: payload })

export const respondToBattleMercenaryApplication = (
  battleId: number,
  applicationId: number,
  accept: boolean,
) => putBattlesByBattleIdMercenaryApplicationsByApplicationIdResponse({ path: { battleId, applicationId }, body: { accept } })

export const getBattleMercenaries = async (
  battleId: number,
): Promise<BattleMercenary[]> => (await getBattlesByBattleIdMercenaries({ path: { battleId } })).data!

export const removeBattleMercenary = (
  battleId: number,
  mercenaryId: number,
) => deleteBattlesByBattleIdMercenariesByMercenaryId({ path: { battleId, mercenaryId } })

export const removeBattleMercenaryApplication = (
  battleId: number,
) => deleteBattlesByBattleIdMercenaryApplications({ path: { battleId } })

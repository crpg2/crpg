import {
  getBattles as _getBattles,
  deleteBattlesByBattleIdMercenaryApplications,
  deleteBattlesByBattleIdParticipantsByParticipantId,
  getBattlesByBattleId,
  getBattlesByBattleIdFighterApplications,
  getBattlesByBattleIdFighters,
  getBattlesByBattleIdMercenaryApplications,
  getBattlesByBattleIdParticipants,
  postBattlesByBattleIdMercenaryApplications,
  putBattlesByBattleIdMercenaryApplicationsByApplicationIdResponse,
  putBattlesByBattleIdSideBriefing,
} from '#api/sdk.gen'

import type { Region } from '~/models/region'
import type { Battle, BattleFighter, BattleFighterApplicationStatus, BattleMercenaryApplication, BattleMercenaryApplicationCreation, BattleMercenaryApplicationStatus, BattleParticipant, BattlePhase, BattleSide, BattleSideBriefing, BattleType } from '~/models/strategus/battle'

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
): Promise<Battle[]> => {
  return (await _getBattles({ query: { region, 'phase[]': phases, type } })).data!
}

export const getBattle = async (
  battleId: number,
) => (await getBattlesByBattleId({ path: { battleId } })).data

export const updateBattleSideBriefing = async (
  battleId: number,
  side: BattleSide,
  briefing: BattleSideBriefing,
) => putBattlesByBattleIdSideBriefing({ path: { battleId }, body: { side, ...briefing } })

export const getBattleFighters = async (
  battleId: number,
): Promise<BattleFighter[]> => (await getBattlesByBattleIdFighters({ path: { battleId } })).data!

// export const getBattleFighterByUserId = (
//   battleFighters: BattleFighter[],
//   userId: number,
// ) => battleFighters.find(f => f.party?.user.id === userId) ?? null

export const getBattleFighterApplications = async (
  battleId: number,
  statuses: BattleFighterApplicationStatus[],
) => (await getBattlesByBattleIdFighterApplications({ path: { battleId }, query: { 'status[]': statuses } })).data!

export const getBattleMercenaryApplications = async (
  battleId: number,
  statuses: BattleMercenaryApplicationStatus[],
): Promise<BattleMercenaryApplication[]> => (await getBattlesByBattleIdMercenaryApplications({ path: { battleId }, query: { 'status[]': statuses } })).data!

export const applyToBattleAsMercenary = (
  battleId: number,
  payload: BattleMercenaryApplicationCreation,
) => postBattlesByBattleIdMercenaryApplications({ path: { battleId }, body: payload })

export const removeBattleMercenaryApplication = (
  battleId: number,
  side: BattleSide,
) => deleteBattlesByBattleIdMercenaryApplications({ path: { battleId }, body: { side } })

export const respondToBattleMercenaryApplication = (
  battleId: number,
  applicationId: number,
  accept: boolean,
) => putBattlesByBattleIdMercenaryApplicationsByApplicationIdResponse({ path: { battleId, applicationId }, body: { accept } })

export const getBattleParticipants = async (
  battleId: number,
): Promise<BattleParticipant[]> => (await getBattlesByBattleIdParticipants({ path: { battleId } })).data!

export const removeBattleParticipant = (
  battleId: number,
  participantId: number,
) => deleteBattlesByBattleIdParticipantsByParticipantId({ path: { battleId, participantId } })

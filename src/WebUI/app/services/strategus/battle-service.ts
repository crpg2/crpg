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
import type { BattleFighter, BattleFighterApplicationStatus, BattleMercenaryApplicationCreation, BattleMercenaryApplicationStatus, BattlePhase } from '~/models/strategus/battle'

export const getBattles = async (
  region: Region,
  phases: BattlePhase[],
) => (await _getBattles({ query: { region, 'phase[]': phases } })).data!

export const getBattle = async (
  battleId: number,
) => (await getBattlesByBattleId({ path: { battleId } })).data

export const getBattleFighters = async (
  battleId: number,
) => (await getBattlesByBattleIdFighters({ path: { battleId } })).data!

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
) => (await getBattlesByBattleIdMercenaries({ path: { battleId } })).data!

export const removeBattleMercenary = (
  battleId: number,
  mercenaryId: number,
) => deleteBattlesByBattleIdMercenariesByMercenaryId({ path: { battleId, mercenaryId } })

export const removeBattleMercenaryApplication = (
  battleId: number,
) => deleteBattlesByBattleIdMercenaryApplications({ path: { battleId } })

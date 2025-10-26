import {
  getBattles as _getBattles,
  getBattlesByBattleId,
  getBattlesByBattleIdFighterApplications,
  getBattlesByBattleIdFighters,
} from '#api/sdk.gen'

import type { Region } from '~/models/region'
import type { BattleFighter, BattleFighterApplicationStatus, BattlePhase } from '~/models/strategus/battle'

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

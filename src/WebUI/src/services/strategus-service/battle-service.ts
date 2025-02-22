import type { Region } from '~/models/region'
import type {
  Battle,
  BattleFighter,
  BattleFighterApplication,
  BattleFighterApplicationStatus,
  BattleMercenary,
  BattleMercenaryApplication,
  BattleMercenaryApplicationCreation,
  BattleMercenaryApplicationStatus,
  BattlePhase,
} from '~/models/strategus/battle'
import type { Party } from '~/models/strategus/party'
import type { SettlementPublic } from '~/models/strategus/settlement'

import { BattleSide } from '~/models/strategus/battle'
import { del, get, post, put } from '~/services/crpg-client'

export const battleSideToIcon: Record<BattleSide, string> = {
  [BattleSide.Attacker]: 'game-mode-duel',
  [BattleSide.Defender]: 'shield',
}

export const getBattles = async (
  region: Region,
  phases: BattlePhase[],
) => {
  const params = new URLSearchParams()
  params.append('region', region)
  phases.forEach(p => params.append('phase[]', p))
  return await get<Battle[]>(`/battles?${params}`)
}

export const getBattle = async (
  id: number,
) => {
  return await get<Battle>(`/battles/${id}`)
}

export const getBattleFighter = (battleFighters: BattleFighter[], userId: number) =>
  battleFighters.find(f => (f.party?.user.id || f.settlement?.owner?.user) === userId) || null

export const getBattleFighters = async (id: number) => get<BattleFighter[]>(`/battles/${id}/fighters`)

export const getBattleFighterApplications = async (
  battleId: number,
  statuses: BattleFighterApplicationStatus[],
) => {
  const params = new URLSearchParams()
  statuses.forEach(s => params.append('status[]', s))
  return get<BattleFighterApplication[]>(`/battles/${battleId}/fighter-applications?${params}`)
}

export const respondToBattleFighterApplication = async (
  battleId: number,
  applicationId: number,
  accept: boolean,
) => put<BattleFighterApplication>(`/battles/${battleId}/fighter-applications/${applicationId}/response`, { accept })

export const getBattleMercenaryApplications = async (
  battleId: number,
  statuses: BattleMercenaryApplicationStatus[],
) => {
  const params = new URLSearchParams()
  statuses.forEach(s => params.append('status[]', s))
  return get<BattleMercenaryApplication[]>(`/battles/${battleId}/mercenary-applications?${params}`)
}

export const applyToBattleAsMercenary = async (
  battleId: number,
  payload: BattleMercenaryApplicationCreation,
) => post<BattleMercenaryApplication>(`/battles/${battleId}/mercenary-applications`, payload)

export const respondToBattleMercenaryApplication = async (
  battleId: number,
  applicationId: number,
  accept: boolean,
) => put<BattleMercenaryApplication>(`/battles/${battleId}/mercenary-applications/${applicationId}/response`, { accept })

export const getBattleMercenaries = async (id: number) => get<BattleMercenary[]>(`/battles/${id}/mercenaries`)

export const removeBattleMercenary = async (
  battleId: number,
  mercenaryId: number,
) => del(`/battles/${battleId}/mercenaries/${mercenaryId}`)

export const removeBattleMercenaryApplication = async (
  battleId: number,
) => del(`/battles/${battleId}/mercenary-applications`)

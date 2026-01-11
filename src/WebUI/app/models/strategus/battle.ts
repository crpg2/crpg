import type {
  BattleFighterApplicationStatus as _BattleFighterApplicationStatus,
  BattleMercenaryApplicationStatus as _BattleMercenaryApplicationStatus,
  BattleParticipantType as _BattleParticipantType,
  BattlePhase as _BattlePhase,
  BattleSide as _BattleSide,
  BattleType as _BattleType,
} from '#api'
import type { Point } from 'geojson'
import type { ValueOf } from 'type-fest'

import type { CharacterPublic } from '~/models/character'
import type { Region } from '~/models/region'
import type { PartyPublic } from '~/models/strategus/party'
import type { SettlementPublic } from '~/models/strategus/settlement'
import type { UserPublic } from '~/models/user'

import type { Terrain } from './terrain'

export const BATTLE_TYPE = {
  Battle: 'Battle',
  Siege: 'Siege',
} as const satisfies Record<_BattleType, _BattleType>

export type BattleType = ValueOf<typeof BATTLE_TYPE>

export const BATTLE_PHASE = {
  Preparation: 'Preparation',
  Hiring: 'Hiring',
  Scheduled: 'Scheduled',
  Live: 'Live',
  End: 'End',
} as const satisfies Record<_BattlePhase, _BattlePhase>

export type BattlePhase = ValueOf<typeof BATTLE_PHASE>

export const BATTLE_SIDE = {
  Attacker: 'Attacker',
  Defender: 'Defender',
} as const satisfies Record<_BattleSide, _BattleSide>

export type BattleSide = ValueOf<typeof BATTLE_SIDE>

export interface Battle {
  id: number
  phase: BattlePhase
  type: BattleType
  region: Region
  position: Point
  scheduledFor: Date | null
  createdAt: Date
  attacker: BattleSideDetailed
  defender: BattleSideDetailed
  terrain: Terrain
  nearestSettlement: SettlementPublic | null
}

export interface BattleSideDetailed {
  commander: BattleFighter
  settlement: SettlementPublic | null
  totalTroops: number
  totalParticipantSlots: number
  mercenaryApplication: BattleMercenaryApplication | null
  briefing: BattleSideBriefing
};
export interface BattleSideBriefing {
  note: string
};

export interface BattleFighter {
  id: number
  commander: boolean
  side: BattleSide
  party: PartyPublic | null
  settlement: SettlementPublic | null
  participantSlots: number
}

export const BATTLE_FIGHTER_APPLICATION_STATUS = {
  Pending: 'Pending',
  Declined: 'Declined',
  Accepted: 'Accepted',
} as const satisfies Record<_BattleFighterApplicationStatus, _BattleFighterApplicationStatus>

export type BattleFighterApplicationStatus = ValueOf<typeof BATTLE_FIGHTER_APPLICATION_STATUS>

export interface BattleParticipant {
  id: number
  user: UserPublic
  character: CharacterPublic
  side: BattleSide
  type: BattleParticipantType
  mercenaryApplicationId: number | null
  statistic: BattleParticipantStatistic | null
}

export const BATTLE_PARTICIPANT_TYPE = {
  Party: 'Party',
  Mercenary: 'Mercenary',
  ClanMember: 'ClanMember',
} as const satisfies Record<_BattleParticipantType, _BattleParticipantType>

export type BattleParticipantType = ValueOf<typeof BATTLE_PARTICIPANT_TYPE>

export interface BattleParticipantStatistic {
  participated: boolean
  kills: number
  assists: number
  deaths: number
}

export interface BattleMercenaryApplicationCreation {
  characterId: number
  side: BattleSide
  wage: number
  note: string
}

export const BATTLE_MERCENARY_APPLICATION_STATUS = {
  Accepted: 'Accepted',
  Declined: 'Declined',
  Pending: 'Pending',
} as const satisfies Record<_BattleMercenaryApplicationStatus, _BattleMercenaryApplicationStatus>

export type BattleMercenaryApplicationStatus = ValueOf<typeof BATTLE_MERCENARY_APPLICATION_STATUS>

export interface BattleMercenaryApplication {
  id: number
  user: UserPublic
  createdAt: Date
  character: CharacterPublic
  side: BattleSide
  wage: number
  note: string
  status: BattleMercenaryApplicationStatus
}

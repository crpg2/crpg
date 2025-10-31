import type {
  BattleFighterApplicationStatus as _BattleFighterApplicationStatus,
  BattleMercenaryApplicationStatus as _BattleMercenaryApplicationStatus,
  BattlePhase as _BattlePhase,
  BattleSide as _BattleSide,
} from '#api'
import type { Point } from 'geojson'
import type { ValueOf } from 'type-fest'

import type { Character } from '~/models/character'
import type { Region } from '~/models/region'
import type { PartyCommon } from '~/models/strategus/party'
import type { SettlementPublic } from '~/models/strategus/settlement'
import type { UserPublic } from '~/models/user'

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
  region: Region
  position: Point
  scheduledFor: Date | null
  createdAt: Date
  attacker: BattleFighter
  attackerTotalTroops: number
  defender: BattleFighter | null // TODO: no defender? PvE?
  defenderTotalTroops: number
}

export interface BattleFighter {
  id: number
  commander: boolean // TODO: who?
  side: BattleSide
  party: PartyCommon | null
  settlement: SettlementPublic | null
  mercenarySlots: number
}

export const BATTLE_APPLICATION_TYPE = {
  Fighter: 'Fighter',
  Mercenary: 'Mercenary',
} as const

export type BattleApplicationType = ValueOf<typeof BATTLE_APPLICATION_TYPE>

export const BATTLE_FIGHTER_APPLICATION_STATUS = {
  Pending: 'Pending',
  Declined: 'Declined',
  Accepted: 'Accepted',
} as const satisfies Record<_BattleFighterApplicationStatus, _BattleFighterApplicationStatus>

export type BattleFighterApplicationStatus = ValueOf<typeof BATTLE_FIGHTER_APPLICATION_STATUS>

export interface BattleMercenary {
  id: number
  user: UserPublic
  character: Character
  captain: BattleFighter
  side: BattleSide
}

export interface BattleMercenaryApplicationCreation {
  userId: number
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
  character: Character
  side: BattleSide
  wage: number
  note: string
  status: BattleMercenaryApplicationStatus
}

export interface BattleMercenaryApplicationCreation {
  userId: number
  characterId: number
  side: BattleSide
  wage: number
  note: string
}

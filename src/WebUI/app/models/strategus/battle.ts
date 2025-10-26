import type {
  BattleFighterApplicationStatus as _BattleFighterApplicationStatus,
  BattlePhase as _BattlePhase,
  BattleSide as _BattleSide,
} from '#api'
import type { Point } from 'geojson'
import type { ValueOf } from 'type-fest'

import type { Region } from '~/models/region'
import type { PartyCommon } from '~/models/strategus/party'
import type { SettlementPublic } from '~/models/strategus/settlement'

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

// export const BATTLE_APPLICATION_TYPE = {
//   Fighter: 'Fighter',
//   Mercenary: 'Mercenary',
// } as const satisfies Record<_BattleSide, _BattleSide>

// export type BattleApplicationType = ValueOf<typeof BATTLE_APPLICATION_TYPE>

export const BATTLE_FIGHTER_APPLICATION_STATUS = {
  Pending: 'Pending',
  Declined: 'Declined',
  Accepted: 'Accepted',
} as const satisfies Record<_BattleFighterApplicationStatus, _BattleFighterApplicationStatus>

export type BattleFighterApplicationStatus = ValueOf<typeof BATTLE_FIGHTER_APPLICATION_STATUS>

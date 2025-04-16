import type { Point } from 'geojson'

import type { Character } from '~/models/character'
import type { Region } from '~/models/region'
import type { PartyCommon } from '~/models/strategus/party'
import type { SettlementPublic } from '~/models/strategus/settlement'
import type { UserPublic } from '~/models/user'

export enum BattlePhase {
  Preparation = 'Preparation',
  Hiring = 'Hiring',
  Scheduled = 'Scheduled',
  Live = 'Live',
  End = 'End',
}

export enum BattleSide {
  Attacker = 'Attacker',
  Defender = 'Defender',
}

// TODO:
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
  commander: boolean // who?
  side: BattleSide
  party: PartyCommon | null
  settlement: SettlementPublic | null
  mercenarySlots: number
}

export enum BattleApplicationType {
  Fighter = 'Fighter',
  Mercenary = 'Mercenary',
}

export enum BattleFighterApplicationStatus {
  Pending = 'Pending',
  Declined = 'Declined',
  Accepted = 'Accepted',
}

export interface BattleFighterApplication {
  id: number
  party: PartyCommon
  battleSide: BattleSide
  status: BattleFighterApplicationStatus
}

export interface BattleMercenary {
  id: number
  user: UserPublic
  character: Character
  captain: BattleFighter
  side: BattleSide
}

export enum BattleMercenaryApplicationStatus {
  Pending = 'Pending',
  Declined = 'Declined',
  Accepted = 'Accepted',
}

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

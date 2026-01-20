import type {
  PartyStatus as _PartyStatus,
} from '#api'
import type { MultiPoint, Point } from 'geojson'
import type { ValueOf } from 'type-fest'

import type { SettlementPublic } from '~/models/strategus/settlement'
import type { UserPublic } from '~/models/user'

import type { BattleJoinIntent, MapBattle } from './battle'

export const PARTY_STATUS = {
  Idle: 'Idle',
  IdleInSettlement: 'IdleInSettlement',
  RecruitingInSettlement: 'RecruitingInSettlement',
  MovingToPoint: 'MovingToPoint',
  FollowingParty: 'FollowingParty',
  MovingToSettlement: 'MovingToSettlement',
  MovingToAttackParty: 'MovingToAttackParty',
  MovingToAttackSettlement: 'MovingToAttackSettlement',
  MovingToBattle: 'MovingToBattle',
  InBattle: 'InBattle',
} as const satisfies Record<_PartyStatus, _PartyStatus>

export type PartyStatus = ValueOf<typeof PARTY_STATUS>

// TODO: rename
export interface PartyPublic {
  id: number
  user: UserPublic
}

export interface PartyCommon {
  id: number
  troops: number
  position: Point
  user: UserPublic
}

export interface Party extends PartyCommon {
  gold: number
  status: PartyStatus
  waypoints: MultiPoint
  targetedParty: PartyCommon | null
  targetedSettlement: SettlementPublic | null
  battleJoinIntents: Array<BattleJoinIntent>
  battleId: number | null
}

export interface StrategusUpdate {
  party: Party
  visibleParties: PartyCommon[]
  visibleSettlements: SettlementPublic[]
  visibleBattles: MapBattle[] // TODO: FIXME:
}

export interface UpdatePartyStatus {
  status: PartyStatus
  waypoints: MultiPoint
  targetedPartyId: number | null
  targetedSettlementId: number | null
  battleJoinIntents: Array<BattleJoinIntent>
}

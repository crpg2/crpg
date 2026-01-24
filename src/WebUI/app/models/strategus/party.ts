import type {
  PartyStatus as _PartyStatus,
} from '#api'
import type { MultiPoint, Point } from 'geojson'
import type { ValueOf } from 'type-fest'

import type { SettlementPublic } from '~/models/strategus/settlement'
import type { UserPublic } from '~/models/user'

import type { Item } from '../item'
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

export interface PartyCommon {
  id: number
  troops: number
  user: UserPublic
}

export interface PartyPublic extends PartyCommon {
}

export interface PartyVisible extends PartyCommon {
  position: Point
}

export interface Party extends PartyCommon {
  gold: number
  status: PartyStatus
  position: Point
  waypoints: MultiPoint
  targetedParty: PartyCommon | null
  targetedSettlement: SettlementPublic | null
  targetedBattle: MapBattle | null
  battleJoinIntents: Array<BattleJoinIntent>
}

export interface StrategusUpdate {
  party: Party
  visibleParties: PartyVisible[]
  visibleSettlements: SettlementPublic[]
  visibleBattles: MapBattle[] // TODO: FIXME:
}

export interface UpdatePartyStatus {
  status: PartyStatus
  waypoints: MultiPoint
  targetedPartyId: number
  targetedSettlementId: number
  targetedBattletId: number
  battleJoinIntents: Array<BattleJoinIntent>
}

export interface ItemStack {
  item: Item
  count: number
}

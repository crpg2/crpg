import type {
  PartyOrderType as _PartyOrderType,
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
  AwaitingBattleJoinDecision: 'AwaitingBattleJoinDecision',
  InBattle: 'InBattle',
  AwaitingPartyOfferDecision: 'AwaitingPartyOfferDecision',
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
  speed: number
  orders: PartyOrder[]
  currentParty: PartyVisible | null
  currentSettlement: SettlementPublic | null
  currentBattle: MapBattle | null
}

export interface StrategusUpdate {
  party: Party
  visibleParties: PartyVisible[]
  visibleSettlements: SettlementPublic[]
  visibleBattles: MapBattle[] // TODO: FIXME:
}

export interface ItemStack {
  item: Item
  count: number
}

export const PARTY_ORDER_TYPE = {
  MoveToPoint: 'MoveToPoint',
  FollowParty: 'FollowParty',
  AttackParty: 'AttackParty',
  MoveToSettlement: 'MoveToSettlement',
  AttackSettlement: 'AttackSettlement',
  JoinBattle: 'JoinBattle',
} as const satisfies Record<_PartyOrderType, _PartyOrderType>

export type PartyOrderType = ValueOf<typeof PARTY_ORDER_TYPE>

export interface PartyOrder {
  type: PartyOrderType
  orderIndex: number
  distance: number
  waypoints: MultiPoint
  targetedParty: PartyVisible | null
  targetedSettlement: SettlementPublic | null
  targetedBattle: MapBattle | null
  battleJoinIntents: Array<BattleJoinIntent>
}

export interface UpdatePartyOrder {
  type: PartyOrderType
  orderIndex: number
  waypoints: MultiPoint
  targetedPartyId: number
  targetedSettlementId: number
  targetedBattleId: number
  battleJoinIntents: Array<BattleJoinIntent>
}

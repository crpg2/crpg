import type {
  PartyOrderType as _PartyOrderType,
  PartyStatus as _PartyStatus,
  PartyTransferOfferStatus as _PartyTransferOfferStatus,
} from '#api'
import type { MultiPoint, Point } from 'geojson'
import type { ValueOf } from 'type-fest'

import type { Item } from '~/models/item'
import type { BattleJoinIntent, MapBattle } from '~/models/strategus/battle'
import type { SettlementPublic } from '~/models/strategus/settlement'
import type { TerrainType } from '~/models/strategus/terrain'
import type { UserPublic } from '~/models/user'

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
  speed: PartySpeed
  orders: PartyOrder[]
  currentParty: PartyVisible | null
  currentSettlement: SettlementPublic | null
  currentBattle: MapBattle | null
  currentTransferOffers: Array<TransferOfferParty>
}

export interface StrategusUpdate {
  party: Party
  visibleParties: PartyVisible[]
  visibleSettlements: SettlementPublic[]
  visibleBattles: MapBattle[] // TODO: FIXME:
}

export interface PartySpeed {
  baseSpeed: number
  terrainSpeedFactor: number
  currentTerrainType?: TerrainType | null
  weightFactor: number
  mountInfluence: number
  troopInfluence: number
  baseSpeedWithoutTerrain: number
  finalSpeed: number
}

export interface ItemStack {
  item: Item
  count: number
}

export interface ItemStackUpdate {
  itemId: string
  count: number
}

export const PARTY_ORDER_TYPE = {
  MoveToPoint: 'MoveToPoint',
  FollowParty: 'FollowParty',
  AttackParty: 'AttackParty',
  MoveToSettlement: 'MoveToSettlement',
  AttackSettlement: 'AttackSettlement',
  JoinBattle: 'JoinBattle',
  TransferOfferParty: 'TransferOfferParty',
} as const satisfies Record<_PartyOrderType, _PartyOrderType>

export type PartyOrderType = ValueOf<typeof PARTY_ORDER_TYPE>

export interface PartyOrder {
  type: PartyOrderType
  orderIndex: number
  waypoints: MultiPoint
  targetedParty: PartyVisible | null
  targetedSettlement: SettlementPublic | null
  targetedBattle: MapBattle | null
  battleJoinIntents: Array<BattleJoinIntent>
  transferOfferPartyIntent: TransferOfferParty | null
  pathSegments: Array<PartyOrderPathSegment>
}

export interface PartyOrderPathSegment {
  startPoint: Point
  endPoint: Point
  distance: number
  speedMultiplier: number
  speed: number
}

export const PARTY_TRANSFER_STATUS = {
  Pending: 'Pending',
  Intent: 'Intent',
} as const satisfies Record<_PartyTransferOfferStatus, _PartyTransferOfferStatus>

export type PartyTransferOfferStatus = ValueOf<typeof PARTY_TRANSFER_STATUS>

export interface TransferOfferParty {
  id: number
  party: PartyVisible
  targetParty: PartyVisible
  status: PartyTransferOfferStatus
  gold: number
  troops: number
  items: Array<ItemStack>
}

export interface TransferOfferPartyUpdate {
  gold: number
  troops: number
  items: Array<ItemStackUpdate>
}

export interface UpdatePartyOrder {
  type: PartyOrderType
  orderIndex: number
  waypoints: MultiPoint
  targetedPartyId: number
  targetedSettlementId: number
  targetedBattleId: number
  battleJoinIntents: Array<BattleJoinIntent>
  transferOfferPartyIntent: TransferOfferPartyUpdate | null
}

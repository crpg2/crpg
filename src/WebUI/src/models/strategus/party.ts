import type { MultiPoint, Point } from 'geojson'

import type { Clan } from '~/models/clan'
import type { SettlementPublic } from '~/models/strategus/settlement'
import type { UserPublic } from '~/models/user'

export enum PartyStatus {
  Idle = 'Idle',
  IdleInSettlement = 'IdleInSettlement',
  RecruitingInSettlement = 'RecruitingInSettlement',
  MovingToPoint = 'MovingToPoint',
  FollowingParty = 'FollowingParty',
  MovingToSettlement = 'MovingToSettlement',
  MovingToAttackParty = 'MovingToAttackParty',
  MovingToAttackSettlement = 'MovingToAttackSettlement',
  InBattle = 'InBattle',
}

export interface PartyCommon {
  id: number
  name: string
  troops: number
  position: Point
  user: UserPublic
  clan: Clan
}

export interface Party extends PartyCommon {
  gold: number
  status: PartyStatus
  waypoints: MultiPoint
  targetedParty: PartyCommon | null
  targetedSettlement: SettlementPublic | null
}

export interface PartyStatusUpdateRequest {
  status: PartyStatus
  waypoints: MultiPoint
  targetedPartyId: number
  targetedSettlementId: number
}

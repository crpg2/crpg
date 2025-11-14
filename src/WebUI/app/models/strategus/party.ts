import type {
  PartyStatus as _PartyStatus,
} from '#api'
import type { MultiPoint, Point } from 'geojson'
import type { ValueOf } from 'type-fest'

import type { SettlementPublic } from '~/models/strategus/settlement'
import type { UserPublic } from '~/models/user'

export const PARTY_STATUS = {
  Idle: 'Idle',
  IdleInSettlement: 'IdleInSettlement',
  RecruitingInSettlement: 'RecruitingInSettlement',
  MovingToPoint: 'MovingToPoint',
  FollowingParty: 'FollowingParty',
  MovingToSettlement: 'MovingToSettlement',
  MovingToAttackParty: 'MovingToAttackParty',
  MovingToAttackSettlement: 'MovingToAttackSettlement',
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
  name: string
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
}

import type { ValueOf } from 'type-fest'

import type { RestrictionType as _RestrictionType } from '~/api'

import type { Clan, ClanMemberRole } from './clan'
import type { Item, ItemSlot, ItemType } from './item'
import type { NotificationState } from './notifications'
import type { Platform } from './platform'
import type { Region } from './region'
import type { Role } from './role'

export interface User {
  id: number
  platform: Platform
  platformUserId: string
  name: string
  gold: number
  avatar?: string | null // TODO: remove ?
  region: Region
  isDonor: boolean
  role: Role
  heirloomPoints: number
  experienceMultiplier: number
  activeCharacterId?: number | null // TODO: remove ?
  unreadNotificationsCount: number
  clanMembership: UserClanMembership | null
}

export interface UserPublic
  extends Pick<User, 'id' | 'platform' | 'platformUserId' | 'name' | 'region' | 'avatar' | 'clanMembership'> {}

export interface UserPrivate extends UserPublic {
  gold: number
  note: string
  createdAt: Date
  updatedAt: Date
  isDonor: boolean
  heirloomPoints: number
  experienceMultiplier: number
  activeCharacterId: number | null
}

export interface UserClanMembership {
  clan: Clan
  role: ClanMemberRole
}

export interface UserItem {
  id: number
  item: Item
  userId: number
  createdAt: Date
  isBroken: boolean
  isPersonal: boolean
  isArmoryItem: boolean
}

export interface UserItemsByType {
  type: ItemType
  items: UserItem[]
}

export type UserItemsBySlot = Record<ItemSlot, UserItem>

export interface UserNotification {
  id: number
  createdAt: Date
  type: string
  state: NotificationState
  metadata: Record<string, string>
}

export const USER_RESTRICTION_TYPE = {
  Join: 'Join',
  Chat: 'Chat',
  All: 'All',
} as const satisfies Record<_RestrictionType, _RestrictionType>

export type UserRestrictionType = ValueOf<typeof USER_RESTRICTION_TYPE>

export interface UserRestriction {
  id: number
  reason: string
  createdAt: Date
  duration: string // seconds
  publicReason: string
  type: UserRestrictionType
  restrictedUser: UserPrivate
  restrictedByUser: UserPublic
  status: UserRestrictionStatus
}

export interface UserRestrictionPublic {
  id: number
  reason: string
  createdAt: Date
  duration: string // seconds
}

export const USER_RESTRICTION_STATUS = {
  Active: 'Active',
  NonActive: 'NonActive',
} as const

export type UserRestrictionStatus = ValueOf<typeof USER_RESTRICTION_STATUS>

export interface UserRestrictionCreation {
  reason: string
  duration: number // seconds
  publicReason: string
  type: UserRestrictionType
  restrictedUserId: number
}

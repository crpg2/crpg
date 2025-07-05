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

export enum UserRestrictionType {
  Join = 'Join',
  Chat = 'Chat',
  All = 'All',
}

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

export enum UserRestrictionStatus {
  Active = 'Active',
  NonActive = 'NonActive',
}

export interface UserRestrictionCreation {
  reason: string
  duration: number // seconds
  publicReason: string
  type: UserRestrictionType
  restrictedUserId: number
}

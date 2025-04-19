import type { Clan, ClanMemberRole } from './clan'
import type { Item, ItemSlot, ItemType } from './item'
import type { NotificationState } from './notifications'
import type { Platform } from './platform'
import type { Region } from './region'
import type Role from './role'

export interface User {
  id: number
  platform: Platform
  platformUserId: string
  name: string
  gold: number
  avatar: string
  region: Region
  isDonor: boolean
  role: Role
  heirloomPoints: number
  experienceMultiplier: number
  activeCharacterId: number | null
  unreadNotificationsCount: number
  clanMembership: UserClanMembership | null
}

export interface UserPublic
  extends Pick<User, 'id' | 'platform' | 'platformUserId' | 'name' | 'region' | 'avatar' | 'clanMembership'> {
}

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

// TODO: FIXME: RENAME??
export interface UserClanMembership {
  clan: Clan
  role: ClanMemberRole
}

// TODO: to /models/item.ts
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

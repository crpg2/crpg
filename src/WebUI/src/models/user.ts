import type { ActivityLog, ActivityLogMetadataDicts } from './activity-logs'
import type { Clan } from './clan'
import type { Item, ItemSlot, ItemType } from './item'
import type { NotificationState, UserNotificationType } from './notificatios'
import type { Platform } from './platform'
import type { Region } from './region'
import type Role from './role'

export interface User {
  id: number
  platform: Platform
  platformUserId: string
  name: string
  gold: number
  heirloomPoints: number
  role: Role
  avatar: string
  activeCharacterId: number | null
  region: Region
  experienceMultiplier: number
  isDonor: boolean
  unreadNotificationsCount: number
}

export interface UserPublic
  extends Pick<User, 'id' | 'platform' | 'platformUserId' | 'name' | 'region'> {
  avatar: string
  clan: Clan | null
}

export interface UserPrivate extends UserPublic {
  gold: number
  note: string
  createdAt: Date
  updatedAt: Date
  heirloomPoints: number
  experienceMultiplier: number
  activeCharacterId: number | null
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
  type: UserNotificationType
  state: NotificationState
  activityLog: ActivityLog
}

export interface UserNotificationsWithDicts {
  notifications: UserNotification[]
  dict: ActivityLogMetadataDicts
}

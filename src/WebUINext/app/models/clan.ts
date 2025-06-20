import type { Language } from '~/models/language'
import type { Region } from '~/models/region'
import type { UserItem, UserPublic } from '~/models/user'

export interface Clan {
  id: number
  tag: string
  name: string
  region: Region
  bannerKey: string
  description: string
  primaryColor: number
  secondaryColor: number
  languages: Language[]
  armoryTimeout: number
  discord: string | null
}

export interface ClanWithMemberCount {
  clan: Clan
  memberCount: number
}

export interface ClanMember {
  user: UserPublic
  role: ClanMemberRole
}

export enum ClanMemberRole {
  Member = 'Member',
  Officer = 'Officer',
  Leader = 'Leader',
}

export enum ClanInvitationType {
  Request = 'Request',
  Offer = 'Offer',
}

export enum ClanInvitationStatus {
  Pending = 'Pending',
  Declined = 'Declined',
  Accepted = 'Accepted',
}

export interface ClanInvitation {
  id: number
  invitee: UserPublic
  inviter: UserPublic
  type: ClanInvitationType
  status: ClanInvitationStatus
}

export interface BorrowedItem {
  updatedAt: Date
  userItemId: number
  borrowerUserId: number
}

export interface ClanArmoryItem {
  updatedAt: Date
  userItem: UserItem
  borrowedItem: BorrowedItem | null
}

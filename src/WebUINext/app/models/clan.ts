import type { Language } from '~/models/language'
import type { Region } from '~/models/region'
import type { UserPublic } from '~/models/user'

import type { Item } from './item'

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

export type ClanUpdate = Omit<Clan, 'id'>

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

export interface ClanArmoryItem {
  item: Item
  borrowerUserId: number // 0 = empty
  userId: number
  userItemId: number
}

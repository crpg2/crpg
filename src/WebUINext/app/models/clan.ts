import type { ValueOf } from 'type-fest'

import type { ClanMemberRole as _ClanMemberRole } from '~/api'
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
  armoryTimeout: string // TODO: contribute convert date-span format to number type
  discord?: string | null // TODO: remove ?
}

export interface ClanPublic
  extends Pick<Clan, 'id' | 'tag' | 'primaryColor' | 'secondaryColor' | 'name' | 'bannerKey' | 'region' | 'languages'> {}

export type ClanUpdate = Omit<Clan, 'id'>

export interface ClanWithMemberCount {
  clan: Clan
  memberCount: number
}

export interface ClanMember {
  user: UserPublic
  role: ClanMemberRole
}

export const CLAN_MEMBER_ROLE = {
  Member: 'Member',
  Officer: 'Officer',
  Leader: 'Leader',
} as const satisfies Record<_ClanMemberRole, _ClanMemberRole>

export type ClanMemberRole = ValueOf<typeof CLAN_MEMBER_ROLE>

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

import type { ActivityLogType as _ActivityLogType } from '#api'
import type { ValueOf } from 'type-fest'

export const ACTIVITY_LOG_TYPE = {
  UserCreated: 'UserCreated',
  UserDeleted: 'UserDeleted',
  UserRenamed: 'UserRenamed',
  UserRewarded: 'UserRewarded',
  ItemBought: 'ItemBought',
  ItemSold: 'ItemSold',
  ItemBroke: 'ItemBroke',
  ItemRepaired: 'ItemRepaired',
  ItemUpgraded: 'ItemUpgraded',
  ItemReturned: 'ItemReturned',
  ItemReforged: 'ItemReforged',
  CharacterCreated: 'CharacterCreated',
  CharacterDeleted: 'CharacterDeleted',
  CharacterRespecialized: 'CharacterRespecialized',
  CharacterRetired: 'CharacterRetired',
  CharacterRewarded: 'CharacterRewarded',
  CharacterEarned: 'CharacterEarned',
  CharacterRatingReset: 'CharacterRatingReset',
  ServerJoined: 'ServerJoined',
  ChatMessageSent: 'ChatMessageSent',
  TeamHit: 'TeamHit',
  ClanCreated: 'ClanCreated',
  ClanDeleted: 'ClanDeleted',
  ClanMemberKicked: 'ClanMemberKicked',
  ClanMemberLeaved: 'ClanMemberLeaved',
  ClanMemberRoleEdited: 'ClanMemberRoleEdited',
  ClanApplicationCreated: 'ClanApplicationCreated',
  ClanApplicationAccepted: 'ClanApplicationAccepted',
  ClanApplicationDeclined: 'ClanApplicationDeclined',
  ClanArmoryAddItem: 'ClanArmoryAddItem',
  ClanArmoryRemoveItem: 'ClanArmoryRemoveItem',
  ClanArmoryReturnItem: 'ClanArmoryReturnItem',
  ClanArmoryBorrowItem: 'ClanArmoryBorrowItem',
  TeamHitReported: 'TeamHitReported',
  TeamHitReportedUserKicked: 'TeamHitReportedUserKicked',
  BattleApplyAsMercenary: 'BattleApplyAsMercenary',
  BattleMercenaryApplicationAccepted: 'BattleMercenaryApplicationAccepted',
  BattleMercenaryApplicationDeclined: 'BattleMercenaryApplicationDeclined',
  BattleParticipantKicked: 'BattleParticipantKicked',
  BattleParticipantLeaved: 'BattleParticipantLeaved',
} as const satisfies Record<_ActivityLogType, _ActivityLogType>

export type ActivityLogType = ValueOf<typeof ACTIVITY_LOG_TYPE>

export interface ActivityLog<TMetaData = { [key: string]: string }> {
  id: number
  type: ActivityLogType
  userId: number
  createdAt: Date
  metadata: TMetaData
}

export interface CharacterEarnedMetadata {
  characterId: string
  gameMode: string
  experience: string
  gold: string
  timeEffort: string // seconds
}

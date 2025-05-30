export enum ActivityLogType {
  UserCreated = 'UserCreated',
  UserDeleted = 'UserDeleted',
  UserRenamed = 'UserRenamed',
  UserRewarded = 'UserRewarded',
  ItemBought = 'ItemBought',
  ItemSold = 'ItemSold',
  ItemBroke = 'ItemBroke',
  ItemRepaired = 'ItemRepaired',
  ItemUpgraded = 'ItemUpgraded',
  ItemReturned = 'ItemReturned',
  CharacterCreated = 'CharacterCreated',
  CharacterDeleted = 'CharacterDeleted',
  CharacterRespecialized = 'CharacterRespecialized',
  CharacterRetired = 'CharacterRetired',
  CharacterRewarded = 'CharacterRewarded',
  CharacterEarned = 'CharacterEarned',
  ServerJoined = 'ServerJoined',
  ChatMessageSent = 'ChatMessageSent',
  TeamHit = 'TeamHit',
  ClanCreated = 'ClanCreated',
  ClanDeleted = 'ClanDeleted',
  ClanMemberKicked = 'ClanMemberKicked',
  ClanMemberLeaved = 'ClanMemberLeaved',
  ClanMemberRoleEdited = 'ClanMemberRoleEdited',
  ClanApplicationCreated = 'ClanApplicationCreated',
  ClanApplicationAccepted = 'ClanApplicationAccepted',
  ClanApplicationDeclined = 'ClanApplicationDeclined',
  ClanArmoryAddItem = 'ClanArmoryAddItem',
  ClanArmoryRemoveItem = 'ClanArmoryRemoveItem',
  ClanArmoryReturnItem = 'ClanArmoryReturnItem',
  ClanArmoryBorrowItem = 'ClanArmoryBorrowItem',
}

export interface ActivityLog<T = { [key: string]: string }> {
  id: number
  type: ActivityLogType
  userId: number
  createdAt: Date
  metadata: T
}

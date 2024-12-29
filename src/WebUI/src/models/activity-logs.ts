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
  CharacterCreated = 'CharacterCreated',
  CharacterDeleted = 'CharacterDeleted',
  CharacterRespecialized = 'CharacterRespecialized',
  CharacterRetired = 'CharacterRetired',
  CharacterRewarded = 'CharacterRewarded',
  CharacterEarned = 'CharacterEarned',
  ServerJoined = 'ServerJoined',
  ChatMessageSent = 'ChatMessageSent',
  TeamHit = 'TeamHit',
  ClanArmoryAddItem = 'ClanArmoryAddItem',
  ClanArmoryRemoveItem = 'ClanArmoryRemoveItem',
  ClanArmoryReturnItem = 'ClanArmoryReturnItem',
  ClanArmoryBorrowItem = 'ClanArmoryBorrowItem',
}

export interface CharacterEarnedMetadata {
  characterId: string
  gameMode: string
  experience: string
  gold: string
  timeEffort: string
}

export interface ActivityLog<T = { [key: string]: string }> {
  id: number
  type: ActivityLogType
  userId: number
  createdAt: Date
  metadata: T
}

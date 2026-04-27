import type {
  GameEventField as _GameEventField,
  GameEventType as _GameEventType,
  QuestAggregationType as _QuestAggregationType,
  QuestType as _QuestType,
} from '#api'
import type { ValueOf } from 'type-fest'

export const QUEST_TYPE = {
  Daily: 'Daily',
  Weekly: 'Weekly',
} as const satisfies Record<_QuestType, _QuestType>

export type QuestType = ValueOf<typeof QUEST_TYPE>

export const QUEST_AGGREGATION_TYPE = {
  Count: 'Count',
  Sum: 'Sum',
} as const satisfies Record<_QuestAggregationType, _QuestAggregationType>

export type QuestAggregationType = ValueOf<typeof QUEST_AGGREGATION_TYPE>

export interface UserQuest {
  id: number
  isRewardClaimed: boolean
  expiresAt: Date
  currentValue: number
  questDefinition: QuestDefinition
}

export const GAME_EVENT_TYPE = {
  Hit: 'Hit',
  Kill: 'Kill',
  Block: 'Block',
} as const satisfies Partial<Record<_GameEventType, _GameEventType>>

export type GameEventType = ValueOf<typeof GAME_EVENT_TYPE>

export const GAME_EVENT_FIELD = {
  WeaponClass: 'WeaponClass',
  ItemId: 'ItemId',
  HitType: 'HitType',
  DamageType: 'DamageType',
  Damage: 'Damage',
  TargetType: 'TargetType',
  BodyPart: 'BodyPart',
} as const satisfies Partial<Record<_GameEventField, _GameEventField>>

export type GameEventField = ValueOf<typeof GAME_EVENT_FIELD>

export interface QuestDefinition {
  id: number
  type: QuestType
  eventType: GameEventType
  aggregationType: QuestAggregationType
  aggregationField: GameEventField | null
  eventFiltersJson: Record<GameEventField, string>[]
  requiredValue: number
  rewardGold: number
  rewardExperience: number
}

/*

Filters
+ WeaponClass

+ ItemId

+ HitType
-- Ranged
-- Melee

Damage

TargetType
-- Mount
-- Character

+ BodyPart
-- Head
-- Neck
-- Chest
-- Abdomen
-- ShoulderLeft
-- ShoulderRight
-- ArmLeft
-- ArmRight
-- Legs

+ DamageType
*/

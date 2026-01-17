import type { ValueOf } from 'type-fest'

export const MOVEMENT_TYPE = {
  Move: 'Move',
  Follow: 'Follow',
  Attack: 'Attack',
  JoinToBattleForAttacker: 'JoinToBattleForAttacker',
  JoinToBattleForDefender: 'JoinToBattleForDefender',
} as const

export type MovementType = ValueOf<typeof MOVEMENT_TYPE>

export const MOVEMENT_TARGET_TYPE = {
  Party: 'Party',
  Settlement: 'Settlement',
  Battle: 'Battle',
} as const

export type MovementTargetType = ValueOf<typeof MOVEMENT_TARGET_TYPE>

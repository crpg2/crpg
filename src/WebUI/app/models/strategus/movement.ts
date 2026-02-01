import type { ValueOf } from 'type-fest'

// TODO: FIXME: переделать в приказы
export const MOVEMENT_TYPE = {
  Move: 'Move',
  Follow: 'Follow',
  Attack: 'Attack',
  // TODO: временно
  JoinToBattleForAttacker: 'JoinToBattleForAttacker',
  JoinToBattleForDefender: 'JoinToBattleForDefender',
  JoinToBattleForBoth: 'JoinToBattleForBoth',
  // TODO: временно
  TransferOfferParty: 'TransferOfferParty',
} as const

export type MovementType = ValueOf<typeof MOVEMENT_TYPE>

export const MOVEMENT_TARGET_TYPE = {
  Party: 'Party',
  Settlement: 'Settlement',
  Battle: 'Battle',
} as const

export type MovementTargetType = ValueOf<typeof MOVEMENT_TARGET_TYPE>

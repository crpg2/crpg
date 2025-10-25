import type { ValueOf } from 'type-fest'

import type { CharacterClass, CharacterStatistics } from '~/models/character'
import type { UserPublic } from '~/models/user'

export interface CharacterCompetitive {
  id: number
  level: number
  class: CharacterClass
  statistics: CharacterStatistics[]
  user: UserPublic
}

export interface CharacterCompetitiveNumbered extends CharacterCompetitive {
  position: number
}

export interface Rank {
  min: number
  max: number
  title: string
  color: string
  groupTitle: string
}

export const RANK_GROUP = {
  Iron: 'Iron',
  Copper: 'Copper',
  Bronze: 'Bronze',
  Silver: 'Silver',
  Gold: 'Gold',
  Platinum: 'Platinum',
  Diamond: 'Diamond',
  Champion: 'Champion',
} as const

export type RankGroup = ValueOf<typeof RANK_GROUP>

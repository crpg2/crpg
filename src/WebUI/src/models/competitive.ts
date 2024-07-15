import type { CharacterPublic, CharacterStatistics } from '~/models/character'
import type { UserPublic } from '~/models/user'

export interface CharacterCompetitive extends CharacterPublic {
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

export enum RankGroup {
  Iron = 'Iron',
  Copper = 'Copper',
  Bronze = 'Bronze',
  Silver = 'Silver',
  Gold = 'Gold',
  Platinum = 'Platinum',
  Diamond = 'Diamond',
  Champion = 'Champion',
}

import type { GameMode } from '~/models/game-mode'
import type { ItemSlot } from '~/models/item'
import type { UserItem } from '~/models/user'

export interface CharacterPublic {
  id: number
  name: string
  level: number
  class: CharacterClass
}

export interface Character extends CharacterPublic {
  generation: number
  experience: number
  forTournament: boolean
}

export enum CharacterClass {
  Peasant = 'Peasant',
  Infantry = 'Infantry',
  ShockInfantry = 'ShockInfantry',
  Skirmisher = 'Skirmisher',
  Crossbowman = 'Crossbowman',
  Archer = 'Archer',
  Cavalry = 'Cavalry',
  MountedArcher = 'MountedArcher',
}

export interface CharacterAttributes {
  points: number
  agility: number
  strength: number
}

export interface CharacterSkills {
  points: number
  riding: number
  shield: number
  ironFlesh: number
  powerDraw: number
  athletics: number
  powerThrow: number
  powerStrike: number
  weaponMaster: number
  mountedArchery: number
}

export interface CharacterWeaponProficiencies {
  bow: number
  points: number
  polearm: number
  throwing: number
  crossbow: number
  oneHanded: number
  twoHanded: number
}

export interface CharacterCharacteristics {
  skills: CharacterSkills
  attributes: CharacterAttributes
  weaponProficiencies: CharacterWeaponProficiencies
}

export type CharacteristicSectionKey = keyof CharacterCharacteristics
export type AttributeKey = keyof CharacterAttributes
export type SkillKey = keyof CharacterSkills
export type WeaponProficienciesKey = keyof CharacterWeaponProficiencies
export type CharacteristicKey = AttributeKey | SkillKey | WeaponProficienciesKey

export enum CharacteristicConversion {
  AttributesToSkills = 'AttributesToSkills',
  SkillsToAttributes = 'SkillsToAttributes',
}

export interface CharacterLimitations {
  lastRespecializeAt: Date
}

export interface CharacterStatistics {
  kills: number
  deaths: number
  assists: number
  playTime: number
  gameMode: GameMode
  rating: CharacterRating
}

export interface CharacterRating {
  value: number
  deviation: number
  volatility: number
  competitiveValue: number
}

export interface CharacterSpeedStats {
  freeWeight: number
  nakedSpeed: number
  currentSpeed: number
  timeToMaxSpeed: number
  perceivedWeight: number
  maxWeaponLength: number

  weightReductionFactor: number
  movementSpeedPenaltyWhenAttacking: number
}

export interface UpdateCharacterRequest {
  name: string
}

export interface EquippedItem {
  slot: ItemSlot
  userItem: UserItem
}

export type EquippedItemsBySlot = Record<ItemSlot, UserItem>

export interface EquippedItemId {
  slot: ItemSlot
  userItemId: number | null
}

export interface CharacterOverallItemsStats {
  price: number
  weight: number
  armArmor: number
  legArmor: number
  bodyArmor: number
  headArmor: number
  mountArmor: number
  longestWeaponLength: number
  averageRepairCostByHour: number
}

export enum CharacterArmorOverallKey {
  ArmArmor = 'ArmArmor',
  BodyArmor = 'BodyArmor',
  HeadArmor = 'HeadArmor',
  LegArmor = 'LegArmor',
  MountArmor = 'MountArmor',
}

export interface CharacterArmorOverall {
  value: number
  key: CharacterArmorOverallKey
}

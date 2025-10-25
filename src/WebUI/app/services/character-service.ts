import type { PartialDeep } from 'type-fest'

import {
  deleteUsersSelfCharactersById,
  getUsersByUserIdCharacters,
  getUsersSelfCharacters,
  getUsersSelfCharactersByIdCharacteristics,
  getUsersSelfCharactersByIdEarningStatistics,
  getUsersSelfCharactersByIdItems,
  getUsersSelfCharactersByIdLimitations,
  getUsersSelfCharactersByIdStatistics,
  putUsersSelfCharactersById,
  putUsersSelfCharactersByIdActive,
  putUsersSelfCharactersByIdCharacteristics,
  putUsersSelfCharactersByIdCharacteristicsConvert,
  putUsersSelfCharactersByIdItems,
  putUsersSelfCharactersByIdRespecialize,
  putUsersSelfCharactersByIdRetire,
  putUsersSelfCharactersByIdTournament,
} from '#api/sdk.gen'
import {
  attributePointsPerLevel,
  damageFactorForPowerDraw,
  damageFactorForPowerStrike,
  damageFactorForPowerThrow,
  defaultAgility,
  defaultAttributePoints,
  defaultHealthPoints,
  defaultSkillPoints,
  defaultStrength,
  experienceForLevelCoefs,
  experienceMultiplierByGeneration,
  freeRespecializeIntervalDays,
  freeRespecializePostWindowHours,
  healthPointsForIronFlesh,
  healthPointsForStrength,
  highLevelCutoff,
  maxExperienceMultiplierForGeneration,
  maximumLevel,
  minimumLevel,
  minimumRetirementLevel,
  respecializePriceForLevel30,
  respecializePriceHalfLife,
  skillPointsPerLevel,
  weaponProficiencyCostCoefs,
  weaponProficiencyPointsForAgility,
  weaponProficiencyPointsForLevelCoefs,
  weaponProficiencyPointsForWeaponMasterCoefs,
} from '~root/data/constants.json'
import { defu } from 'defu'
import { clamp } from 'es-toolkit'

import type { ActivityLog } from '~/models/activity-logs'
import type {
  Character,
  CharacterArmorOverall,
  CharacterCharacteristics,
  CharacterClass,
  CharacterEarnedData,
  CharacterEarningType,
  CharacteristicConversion,
  CharacteristicKey,
  CharacteristicSectionKey,
  CharacterLimitations,
  CharacterMountSpeedStats,
  CharacterOverallItemsStats,
  CharacterSpeedStats,
  CharacterStatistics,
  EquippedItem,
  EquippedItemId,
  SkillKey,
  UpdateCharacterRequest,
} from '~/models/character'
import type { GameMode } from '~/models/game-mode'
import type { Item, ItemArmorComponent, ItemSlot, ItemType } from '~/models/item'
import type { TimeSeries, TimeSeriesItem } from '~/models/time-series'

import {
  CHARACTER_ARMOR_OVERALL_KEY,
  CHARACTER_CLASS,
  CHARACTER_EARNING_TYPE,
} from '~/models/character'
import { GAME_MODE } from '~/models/game-mode'
import { ITEM_SLOT, ITEM_TYPE } from '~/models/item'
import { armorTypes, computeAverageRepairCostPerHour } from '~/services/item-service'
import { getIndexToInsert, range } from '~/utils/array'
import { applyPolynomialFunction, roundFLoat } from '~/utils/math'

export const getCharacters = async (): Promise<Character[]> => (await getUsersSelfCharacters({})).data!

export const getCharactersByUserId = async (
  userId: number,
): Promise<Character[]> => (await getUsersByUserIdCharacters({ path: { userId } })).data!

export const updateCharacter = (
  characterId: number,
  req: UpdateCharacterRequest,
) => putUsersSelfCharactersById({ path: { id: characterId }, body: { name: req.name } })

export const activateCharacter = (
  characterId: number,
) => putUsersSelfCharactersByIdActive({ path: { id: characterId }, body: { active: true } })

export const deactivateCharacter = (
  characterId: number,
) => putUsersSelfCharactersByIdActive({ path: { id: characterId }, body: { active: false } })

export const deleteCharacter = (
  characterId: number,
) => deleteUsersSelfCharactersById({ path: { id: characterId } })

export const respecializeCharacter = (
  characterId: number,
) => putUsersSelfCharactersByIdRespecialize({ path: { id: characterId } })

export const tournamentLevelThreshold = 20

export const canSetCharacterForTournamentValidate = (
  character: Character,
) => !(
  character.forTournament
  || character.generation > 0
  || character.level >= tournamentLevelThreshold
)

export const setCharacterForTournament = (
  characterId: number,
) => putUsersSelfCharactersByIdTournament({ path: { id: characterId } })

export const canRetireValidate = (
  level: number,
) => level >= minimumRetirementLevel

export const retireCharacter = (
  characterId: number,
) => putUsersSelfCharactersByIdRetire({ path: { id: characterId } })

export const getCharacterCharacteristics = async (
  characterId: number,
): Promise<CharacterCharacteristics> => (await getUsersSelfCharactersByIdCharacteristics({ path: { id: characterId } })).data

export const convertCharacterCharacteristics = async (
  characterId: number,
  conversion: CharacteristicConversion,
): Promise<CharacterCharacteristics> => (await putUsersSelfCharactersByIdCharacteristicsConvert({ path: { id: characterId }, body: { conversion } })).data

export const updateCharacterCharacteristics = async (
  characterId: number,
  req: CharacterCharacteristics,
) => (await putUsersSelfCharactersByIdCharacteristics({ path: { id: characterId }, body: req })).data

export const getCharacterStatistics = async (
  characterId: number,
): Promise<Partial<Record<GameMode, CharacterStatistics>>> => (await getUsersSelfCharactersByIdStatistics({ path: { id: characterId } })).data!

export const getDefaultCharacterStatistics = (): CharacterStatistics => ({
  assists: 0,
  deaths: 0,
  gameMode: GAME_MODE.CRPGBattle,
  kills: 0,
  playTime: 0,
  rating: { competitiveValue: 0, deviation: 0, value: 0, volatility: 0 },
})

export const getCompetitiveValueByGameMode = (
  statistics: CharacterStatistics[],
  gameMode: GameMode,
): number => {
  const statisticByGameMode = statistics.find(s => s.gameMode === gameMode)
  return statisticByGameMode ? statisticByGameMode.rating.competitiveValue : 0
}

export const getCharacterLimitations = async (
  characterId: number,
): Promise<CharacterLimitations> => {
  const { data } = await getUsersSelfCharactersByIdLimitations({ path: { id: characterId } })
  return data
}

export interface CharacterEarnedMetadata {
  characterId: string
  gameMode: string
  experience: string
  gold: string
  timeEffort: string // seconds
  [key: string]: string
}

export const getCharacterEarningStatistics = async (
  characterId: number,
  from: Date,
): Promise<ActivityLog<CharacterEarnedMetadata>[]> => {
  const { data } = await getUsersSelfCharactersByIdEarningStatistics({
    path: { id: characterId },
    query: { from, to: new Date() },
    onRequest: ({ options }) => {
      options.query = options.query || {}
      options.query.from = from.toISOString()
      options.query.to = new Date().toISOString()
    },
  })
  return data!.map(log => ({
    ...log,
    metadata: {
      characterId: log.metadata.characterId ?? '',
      gameMode: log.metadata.gameMode ?? '',
      experience: log.metadata.experience ?? '',
      gold: log.metadata.gold ?? '',
      timeEffort: log.metadata.timeEffort ?? '',
    },
  }))
}

export const convertCharacterEarningStatisticsToTimeSeries = (
  logs: ActivityLog<CharacterEarnedMetadata>[],
  type: CharacterEarningType,
): TimeSeries[] => {
  return logs.reduce((out, l) => {
    const timeSeriaItem: TimeSeriesItem = [
      l.createdAt,
      Number.parseInt(type === CHARACTER_EARNING_TYPE.Exp ? l.metadata.experience : l.metadata.gold, 10),
    ]

    const currentEl = out.find(el => el.name === l.metadata.gameMode)

    if (currentEl) {
      currentEl.data.push(timeSeriaItem)
    }
    else {
      out.push({
        data: [timeSeriaItem],
        name: l.metadata.gameMode,
      })
    }
    return out
  }, [] as TimeSeries[])
}

// // TODO: FIXME: SPEC
export const summaryByGameModeCharacterEarningStatistics = (logs: ActivityLog<CharacterEarnedMetadata>[]) => {
  return logs.reduce((out, l) => {
    const gameMode = l.metadata.gameMode as GameMode
    const timeEffort = Number(l.metadata.timeEffort) || 0
    const gold = Number(l.metadata.gold)
    const experience = Number(l.metadata.experience)

    if (gameMode in out) {
      out[gameMode].timeEffort = out[gameMode].timeEffort + timeEffort
      out[gameMode].gold = out[gameMode].gold + gold
      out[gameMode].experience = out[gameMode].experience + experience
    }
    else {
      out[gameMode] = {
        timeEffort,
        gold,
        experience,
      }
    }
    return out
  }, {} as Record<GameMode, CharacterEarnedData>)
}

const computeExperienceDistribution = (level: number): number => {
  const [a, b] = experienceForLevelCoefs as [number, number]
  return (level - 1) ** a + b ** (a / 2.0) * (level - 1)
}

// TODO: to const
const experienceForLevel30 = 4420824

export const getExperienceForLevel = (level: number): number => {
  if (level <= 0) {
    return 0
  }

  if (level <= 30) {
    return Math.trunc(
      (experienceForLevel30 * computeExperienceDistribution(level))
      / computeExperienceDistribution(30),
    )
  }

  return getExperienceForLevel(30) * 2 ** (level - 30)
}

const computeExperienceTable = (): number[] => {
  const table: number[] = [maximumLevel - minimumLevel + 1]

  for (let lvl = minimumLevel; lvl <= 30; lvl += 1) {
    table[lvl - minimumLevel] = Math.trunc(
      (experienceForLevel30 * computeExperienceDistribution(lvl))
      / computeExperienceDistribution(30),
    )
  }

  for (let lvl = minimumRetirementLevel; lvl <= maximumLevel; lvl += 1) {
    table[lvl - minimumLevel] = (table[lvl - minimumLevel - 1] ?? 0) * 2 // changing this require to change how much heirloompoint you get above level 31
  }

  return table
}

// // TODO: SPEC
export const getLevelByExperience = (
  exp: number,
  expTable: number[] = computeExperienceTable(),
): number => {
  return getIndexToInsert(expTable, exp)
}

// TODO: spec
// from: src/Application/Characters/Commands/RewardCharacterCommand.cs:67
export const getAutoRetireCount = (
  exp: number,
  characterExperience: number,
) => {
  let retireCount = 0
  let remainingExperienceToGive = exp
  let remainExperience = characterExperience

  const totalExperienceForRetirementLevel = getExperienceForLevel(minimumRetirementLevel)

  while (remainingExperienceToGive > 0) {
    const experienceNeededToRetirementLevel = Math.max(
      totalExperienceForRetirementLevel - remainExperience,
      0,
    )

    const [experienceToGive, retirementLevelReached]
      = remainingExperienceToGive >= experienceNeededToRetirementLevel
        ? [experienceNeededToRetirementLevel, true]
        : [remainingExperienceToGive, false]

    remainExperience += experienceToGive

    if (retirementLevelReached) {
      retireCount++
      remainExperience = 0
    }

    remainingExperienceToGive -= experienceToGive
  }

  return {
    remainExperience,
    retireCount,
  }
}

export const getMaximumExperience = () => getExperienceForLevel(maximumLevel)

export const attributePointsForLevel = (level: number): number => {
  if (level <= 0) {
    level = minimumLevel
  }

  let points = defaultAttributePoints

  for (let i = 1; i < level; i++) {
    if (i < highLevelCutoff) {
      points += attributePointsPerLevel
    }
  }
  return points
}

export const skillPointsForLevel = (level: number): number => {
  if (level <= 0) {
    level = minimumLevel
  }
  return defaultSkillPoints + (level - 1) * skillPointsPerLevel
}

export const wppForLevel = (level: number): number =>
  Math.floor(applyPolynomialFunction(level, weaponProficiencyPointsForLevelCoefs))

export const wppForAgility = (agility: number): number =>
  agility * weaponProficiencyPointsForAgility

export const wppForWeaponMaster = (weaponMaster: number): number =>
  Math.floor(applyPolynomialFunction(weaponMaster, weaponProficiencyPointsForWeaponMasterCoefs))

export const getCharacteristicCost = (
  section: CharacteristicSectionKey,
  key: CharacteristicKey,
  value: number,
): number => {
  if (section === 'weaponProficiencies') {
    return Math.floor(applyPolynomialFunction(value, weaponProficiencyCostCoefs))
  }

  return value
}

export const skillRequirementsSatisfied = (
  key: SkillKey,
  value: number,
  characteristics: CharacterCharacteristics,
): boolean => {
  switch (key) {
    case 'ironFlesh':
    case 'powerStrike':
    case 'powerDraw':
    case 'powerThrow':
      return value <= Math.floor(characteristics.attributes.strength / 3) // TODO: move to constants.json

    case 'athletics':
    case 'riding':
    case 'weaponMaster':
      return value <= Math.floor(characteristics.attributes.agility / 3) // TODO: move to constants.json

    case 'mountedArchery':
    case 'shield':
      return value <= Math.floor(characteristics.attributes.agility / 6) // TODO: move to constants.json

    default:
      return false
  }
}

export const characteristicRequirementsSatisfied = (
  section: CharacteristicSectionKey,
  key: CharacteristicKey,
  value: number,
  characteristics: CharacterCharacteristics,
): boolean => {
  switch (section) {
    case 'skills':
      return skillRequirementsSatisfied(
        key as SkillKey,
        value,
        characteristics,
      )
    default:
      return true
  }
}

export const createEmptyCharacteristic = (): CharacterCharacteristics => ({
  attributes: {
    agility: 0,
    points: 0,
    strength: 0,
  },
  skills: {
    athletics: 0,
    ironFlesh: 0,
    mountedArchery: 0,
    points: 0,
    powerDraw: 0,
    powerStrike: 0,
    powerThrow: 0,
    riding: 0,
    shield: 0,
    weaponMaster: 0,
  },
  weaponProficiencies: {
    bow: 0,
    crossbow: 0,
    oneHanded: 0,
    points: 0,
    polearm: 0,
    throwing: 0,
    twoHanded: 0,
  },
})

export const createCharacteristics = (
  payload?: PartialDeep<CharacterCharacteristics>,
): CharacterCharacteristics => defu(payload, createEmptyCharacteristic())

export const createDefaultCharacteristic = (level = minimumLevel): CharacterCharacteristics =>
  createCharacteristics({
    attributes: {
      points: attributePointsForLevel(level),
      agility: defaultAgility,
      strength: defaultStrength,
    },
    skills: {
      points: skillPointsForLevel(level),
    },
    weaponProficiencies: {
      points: wppForLevel(level),
    },
  })

export interface CharacteristicBonus { value: number, style: 'percent' | 'decimal' }
export const characteristicBonusByKey: Partial<Record<CharacteristicKey, CharacteristicBonus>> = {
  ironFlesh: {
    style: 'decimal',
    value: healthPointsForIronFlesh,
  },
  powerDraw: {
    style: 'percent',
    value: damageFactorForPowerDraw,
  },
  powerStrike: {
    style: 'percent',
    value: damageFactorForPowerStrike,
  },
  powerThrow: {
    style: 'percent',
    value: damageFactorForPowerThrow,
  },
  strength: {
    style: 'decimal',
    value: healthPointsForStrength,
  },
}

export const ATTRIBUTES_TO_SKILLS_RATE = 1

export const SKILLS_TO_ATTRIBUTES_RATE = 2

export const computeHealthPoints = (ironFlesh: number, strength: number): number =>
  defaultHealthPoints + ironFlesh * healthPointsForIronFlesh + strength * healthPointsForStrength

// copy from Module.Server/Common/Models/CrpgAgentStatCalculateModel.cs
export const computeSpeedStats = ({
  strength,
  athletics,
  agility,
  totalEncumbrance,
  longestWeaponLength,
}: { strength: number, athletics: number, agility: number, totalEncumbrance: number, longestWeaponLength: number },
): CharacterSpeedStats => {
  const awfulScaler = 3231477.548
  const weightReductionPolynomialFactor = [
    30 / awfulScaler,
    0.00005 / awfulScaler,
    1000000 / awfulScaler,
    0,
  ]
  const weightReductionFactor
    = 1 / (1 + applyPolynomialFunction(strength - 3, weightReductionPolynomialFactor))
  const freeWeight = 2.5 * (1 + (strength - 3) / 30)
  const perceivedWeight = Math.max(totalEncumbrance - freeWeight, 0) * weightReductionFactor
  const nakedSpeed = 0.58 + (0.034 * (20 * athletics + 2 * agility)) / 26.0
  const currentSpeed = clamp(
    nakedSpeed * (361 / (361 + perceivedWeight ** 5)) ** 0.055,
    0.1,
    1.5,
  )
  const maxWeaponLength = Math.min(
    22 + (strength - 3) * 7.5 + (Math.min(strength - 3, 24) * 0.115) ** 7.75,
    650,
  )
  const timeToMaxSpeedWeaponLenghthTerm = Math.max(
    (1.2 * (longestWeaponLength - maxWeaponLength)) / maxWeaponLength,
    0,
  )

  const timeToMaxSpeed = 0.8
    * (1 + perceivedWeight / 15)
    * (20 / (20 + ((20 * athletics + 3 * agility) / 120) ** 2))
    + timeToMaxSpeedWeaponLenghthTerm

  const movementSpeedPenaltyWhenAttacking
    = 100 * (Math.min(0.8 + (0.2 * (maxWeaponLength + 1)) / (longestWeaponLength + 1), 1) - 1)

  return {
    currentSpeed,
    freeWeight,
    maxWeaponLength,
    movementSpeedPenaltyWhenAttacking,
    nakedSpeed,
    perceivedWeight,
    timeToMaxSpeed,
    weightReductionFactor,
  }
}

// copy from Module.Server/Common/Models/CrpgAgentStatCalculateModel.cs
export function computeMountSpeedStats(
  baseSpeed: number,
  harnessWeight: number,
  riderPerceivedWeight: number,
): CharacterMountSpeedStats {
  const totalEffectiveLoad = harnessWeight + riderPerceivedWeight
  const maxLoadReference = 50 // to const?
  const loadPercentage = Math.min(totalEffectiveLoad / maxLoadReference, 1)

  const weightImpactOnSpeed = 1 / (1 + 0.333 * loadPercentage) // Cap at 1.0

  const effectiveSpeed = (baseSpeed + 1) * 0.209 * weightImpactOnSpeed
  const unmodifiedSpeed = (baseSpeed + 1) * 0.209

  const speedReduction = (effectiveSpeed / unmodifiedSpeed) - 1 // e.g. -0.28 means 28% slower
  const acceleration = 1 / (2 + 8 * loadPercentage)

  return {
    speedReduction,
    mountAcceleration: acceleration,
    effectiveSpeed,
    weightImpactOnSpeed,
    loadPercentage,
  }
}

// copy from src/Module.Server/Common/Models/CrpgAgentStatCalculateModel.cs
export function computeWeaponLengthMountPenalty(
  weaponLength: number,
  strength: number,
): number {
  if (!weaponLength || !strength) {
    return 1 // No penalty
  }

  const maxLength = 22 + (strength - 3) * 7.5 + (Math.min(strength - 3, 24) * 0.115) ** 7.75
  const ratio = Math.min(maxLength / weaponLength, 1)
  const penaltyFactor = 0.8 + 0.2 * ratio
  return penaltyFactor // 1 = no penalty, <1 = reduction
}

export const getCharacterItems = async (
  characterId: number,
): Promise<EquippedItem[]> =>
  (await getUsersSelfCharactersByIdItems({ path: { id: characterId } })).data!

export const updateCharacterItems = async (
  characterId: number,
  items: EquippedItemId[],
): Promise<EquippedItem[]> =>
  // @ts-expect-error TODO: FIXME:
  (await putUsersSelfCharactersByIdItems({ path: { id: characterId }, body: { items } })).data!

export const computeOverallPrice = (
  items: Item[],
) => items.reduce((total, item) => total + item.price, 0)

export const computeOverallWeight = (
  items: Item[],
) => items
  .filter(item => !([ITEM_TYPE.Mount, ITEM_TYPE.MountHarness] as ItemType[]).includes(item.type))
  .reduce((total, item) => (total += ([ITEM_TYPE.Arrows, ITEM_TYPE.Bolts, ITEM_TYPE.Bullets, ITEM_TYPE.Thrown] as ItemType[]).includes(item.type)
    ? roundFLoat(item.weight * (item.weapons[0]?.stackAmount ?? 1))
    : item.weight), 0)

interface OverallArmor extends Omit<ItemArmorComponent, 'materialType' | 'familyType'> {
  mountArmor: number
}

export const computeOverallArmor = (items: Item[]): OverallArmor =>
  items.reduce(
    (total, item) => {
      if (item.type === ITEM_TYPE.MountHarness) {
        total.mountArmor = item.armor!.bodyArmor
      }
      else if (armorTypes.includes(item.type)) {
        total.headArmor += item.armor!.headArmor
        total.bodyArmor += item.armor!.bodyArmor
        total.armArmor += item.armor!.armArmor
        total.legArmor += item.armor!.legArmor
      }
      return total
    },
    {
      armArmor: 0,
      bodyArmor: 0,
      headArmor: 0,
      legArmor: 0,
      mountArmor: 0,
    },
  )

// // TODO: SPEC
export const computeLongestWeaponLength = (items: Item[]) => {
  return items
    .filter(item => ([ITEM_TYPE.OneHandedWeapon, ITEM_TYPE.TwoHandedWeapon, ITEM_TYPE.Polearm] as ItemType[]).includes(item.type))
    .reduce((total, item) => (total += Math.max(total, item.weapons[0]?.length ?? 0)), 0)
}

export const computeOverallAverageRepairCostByHour = (items: Item[]) => Math.floor(items.reduce((total, item) => total + computeAverageRepairCostPerHour(item.price), 0))

export const computeMountSpeedBase = (items: Item[]): number => {
  return items.find(item => item.type === ITEM_TYPE.Mount)?.mount?.speed ?? 0
}

export const computeMountHarnessWeight = (items: Item[]): number => {
  return items.find(item => item.type === ITEM_TYPE.MountHarness)?.weight ?? 0
}

export const getCharacterOverallItemsStats = (): CharacterOverallItemsStats => ({
  armArmor: 0,
  averageRepairCostByHour: 0,
  bodyArmor: 0,
  headArmor: 0,
  legArmor: 0,
  longestWeaponLength: 0,
  mountArmor: 0,
  mountHarnessWeight: 0,
  mountSpeedBase: 0,
  price: 0,
  weight: 0,
})

//
export const getHeirloomPointByLevel = (
  level: number,
) => level < minimumRetirementLevel ? 0 : 2 ** (level - minimumRetirementLevel)

export interface HeirloomPointByLevelAggregation { level: number[], points: number }

export const getHeirloomPointByLevelAggregation = () =>
  range(minimumRetirementLevel, maximumLevel).reduce((out, level) => {
    const points = getHeirloomPointByLevel(level)
    const idx = out.findIndex(item => item.points === points)

    if (idx === -1) {
      out.push({ level: [level], points })
    }
    else if (out[idx]) {
      out[idx].level.push(level)
    }

    return out
  }, [] as HeirloomPointByLevelAggregation[])

export const getExperienceMultiplierBonus = (multiplier: number) => multiplier < maxExperienceMultiplierForGeneration ? experienceMultiplierByGeneration : 0

// TODO: Spec
export const getExperienceMultiplierBonusByRetireCount = (retireCount: number) => {
  let out = 0
  while (retireCount > 0) {
    out += experienceMultiplierByGeneration
    retireCount--
  }

  return out
}

// TODO: Spec
export const sumExperienceMultiplierBonus = (
  multiplierA: number,
  multiplierB: number,
) => clamp(multiplierA + multiplierB, 0, maxExperienceMultiplierForGeneration)

export interface RespecCapability {
  price: number
  enabled: boolean
  nextFreeAt: number
  freeRespecWindowRemain: number
}

export const getRespecCapability = (
  character: Character,
  limitations: CharacterLimitations,
  userGold: number,
  isRecentUser: boolean,
): RespecCapability => {
  if (isRecentUser || character.forTournament) {
    return {
      enabled: true,
      freeRespecWindowRemain: 0,
      nextFreeAt: 0,
      price: 0,
    }
  }

  const freeRespecWindow = new Date(limitations.lastRespecializeAt)
  freeRespecWindow.setUTCHours(freeRespecWindow.getUTCHours() + freeRespecializePostWindowHours)

  if (freeRespecWindow > new Date()) {
    return {
      enabled: true,
      freeRespecWindowRemain: computeLeftMs(freeRespecWindow, 0),
      nextFreeAt: 0,
      price: 0,
    }
  }

  const lastRespecDate = new Date(limitations.lastRespecializeAt)
  const nextFreeAt = new Date(limitations.lastRespecializeAt)
  nextFreeAt.setUTCDate(nextFreeAt.getUTCDate() + freeRespecializeIntervalDays)
  nextFreeAt.setUTCMinutes(nextFreeAt.getUTCMinutes() + 5) // 5 minute margin just in case

  if (nextFreeAt < new Date()) {
    return { enabled: true, freeRespecWindowRemain: 0, nextFreeAt: 0, price: 0 }
  }

  const decayDivider
    = (new Date().getTime() - lastRespecDate.getTime()) / (respecializePriceHalfLife * 1000 * 3600)

  const price = Math.floor(
    Math.floor((character.experience / getExperienceForLevel(30)) * respecializePriceForLevel30)
    / 2 ** decayDivider,
  )

  return {
    enabled: price <= userGold,
    freeRespecWindowRemain: 0,
    nextFreeAt: computeLeftMs(nextFreeAt, 0),
    price,
  }
}

interface SlotsSchema { key: ItemSlot, placeholderIcon: string }

export const getCharacterSlotsSchema = (): SlotsSchema[][] => [
  // left col
  [
    {
      key: ITEM_SLOT.Head,
      placeholderIcon: 'item-type-head-armor',
    },
    {
      key: ITEM_SLOT.Shoulder,
      placeholderIcon: 'item-type-shoulder-armor',
    },
    {
      key: ITEM_SLOT.Body,
      placeholderIcon: 'item-type-body-armor',
    },
    {
      key: ITEM_SLOT.Hand,
      placeholderIcon: 'item-type-hand-armor',
    },
    {
      key: ITEM_SLOT.Leg,
      placeholderIcon: 'item-type-leg-armor',
    },
  ],
  // center col
  [
    {
      key: ITEM_SLOT.MountHarness,
      placeholderIcon: 'item-type-mount-harness',
    },
    {
      key: ITEM_SLOT.Mount,
      placeholderIcon: 'item-type-mount',
    },
  ],
  // right col
  [
    {
      key: ITEM_SLOT.Weapon0,
      placeholderIcon: 'weapons',
    },
    {
      key: ITEM_SLOT.Weapon1,
      placeholderIcon: 'weapons',
    },
    {
      key: ITEM_SLOT.Weapon2,
      placeholderIcon: 'weapons',
    },
    {
      key: ITEM_SLOT.Weapon3,
      placeholderIcon: 'weapons',
    },
    {
      key: ITEM_SLOT.WeaponExtra,
      placeholderIcon: 'item-flag-drop-on-change',
    },
  ],
]

export const getCharacterKDARatio = (characterStatistics: CharacterStatistics): number => Math.round((100 * (characterStatistics.kills + characterStatistics.assists)) / (characterStatistics.deaths || 1)) / 100

export const characterClassToIcon: Record<CharacterClass, string> = {
  [CHARACTER_CLASS.Archer]: 'item-type-bow',
  [CHARACTER_CLASS.Cavalry]: 'char-class-cav',
  [CHARACTER_CLASS.Crossbowman]: 'item-type-crossbow',
  [CHARACTER_CLASS.Infantry]: 'weapon-class-one-handed-polearm',
  [CHARACTER_CLASS.MountedArcher]: 'char-class-ha',
  [CHARACTER_CLASS.Peasant]: 'char-class-peasant',
  [CHARACTER_CLASS.ShockInfantry]: 'weapon-class-two-handed-axe',
  [CHARACTER_CLASS.Skirmisher]: 'weapon-class-throwing-spear',
}

// // TODO: SPEC
export const getOverallArmorValueBySlot = (
  slot: ItemSlot,
  itemsStats: CharacterOverallItemsStats,
) => {
  const itemSlotToArmorValue: Partial<Record<ItemSlot, CharacterArmorOverall>> = {
    [ITEM_SLOT.Body]: {
      key: CHARACTER_ARMOR_OVERALL_KEY.BodyArmor,
      value: itemsStats.bodyArmor,
    },
    [ITEM_SLOT.Hand]: {
      key: CHARACTER_ARMOR_OVERALL_KEY.ArmArmor,
      value: itemsStats.armArmor,
    },
    [ITEM_SLOT.Leg]: {
      key: CHARACTER_ARMOR_OVERALL_KEY.LegArmor,
      value: itemsStats.legArmor,
    },
    [ITEM_SLOT.Mount]: {
      key: CHARACTER_ARMOR_OVERALL_KEY.MountArmor,
      value: itemsStats.mountArmor,
    },
    [ITEM_SLOT.Shoulder]: {
      key: CHARACTER_ARMOR_OVERALL_KEY.HeadArmor,
      value: itemsStats.headArmor,
    },
  }

  return itemSlotToArmorValue[slot]
}

// // TODO: SPEC, more complicated logic?
export const checkUpkeepIsHigh = (
  userGold: number,
  upkeepPerHour: number,
) => userGold < upkeepPerHour * 2.5

export const validateItemNotMeetRequirement = (
  item: Item,
  characterCharacteristics: CharacterCharacteristics,
) => item.requirement > characterCharacteristics.attributes.strength

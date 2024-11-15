import type { PartialDeep } from 'type-fest'

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
  weaponProficiencyPointsForAgility,
  weaponProficiencyPointsForLevelCoefs,
  weaponProficiencyPointsForWeaponMasterCoefs,
} from '~root/data/constants.json'
import { defu } from 'defu'
import qs from 'qs'

import type { ActivityLog, CharacterEarnedMetadata } from '~/models/activity-logs'
import type { TimeSeries } from '~/models/timeseries'

import {
  type Character,
  type CharacterArmorOverall,
  CharacterArmorOverallKey,
  type CharacterCharacteristics,
  CharacterClass,
  type CharacteristicConversion,
  type CharacteristicKey,
  type CharacterLimitations,
  type CharacterOverallItemsStats,
  type CharacterSpeedStats,
  type CharacterStatistics,
  type EquippedItem,
  type EquippedItemId,
  type UpdateCharacterRequest,
} from '~/models/character'
import { GameMode } from '~/models/game-mode'
import { type Item, type ItemArmorComponent, ItemSlot, ItemType } from '~/models/item'
import { del, get, put } from '~/services/crpg-client'
import { armorTypes, computeAverageRepairCostPerHour } from '~/services/item-service'
import { t } from '~/services/translate-service'
import { getIndexToIns, range } from '~/utils/array'
import { computeLeftMs } from '~/utils/date'
import { applyPolynomialFunction, clamp, roundFLoat } from '~/utils/math'

export const getCharacters = () => get<Character[]>('/users/self/characters')

export const getCharactersByUserId = (userId: number) =>
  get<Character[]>(`/users/${userId}/characters`)

export const updateCharacter = (characterId: number, req: UpdateCharacterRequest) =>
  put<Character>(`/users/self/characters/${characterId}`, req)

export const activateCharacter = (characterId: number, active: boolean) =>
  put(`/users/self/characters/${characterId}/active`, { active })

export const respecializeCharacter = (characterId: number) =>
  put<Character>(`/users/self/characters/${characterId}/respecialize`)

export const tournamentLevelThreshold = 20

export const canSetCharacterForTournamentValidate = (character: Character) =>
  !(
    character.forTournament
    || character.generation > 0
    || character.level >= tournamentLevelThreshold
  )

export const setCharacterForTournament = (characterId: number) =>
  put<Character>(`/users/self/characters/${characterId}/tournament`)

export const canRetireValidate = (level: number) => level >= minimumRetirementLevel

export const retireCharacter = (characterId: number) =>
  put<Character>(`/users/self/characters/${characterId}/retire`)

export const rewardCharacter = (
  userId: number,
  characterId: number,
  payload: { experience: number, autoRetire: boolean },
) => put(`/users/${userId}/characters/${characterId}/rewards`, payload)

export const deleteCharacter = (characterId: number) =>
  del(`/users/self/characters/${characterId}`)

export const getCharacterStatistics = (characterId: number) =>
  get<Partial<Record<GameMode, CharacterStatistics>>>(
    `/users/self/characters/${characterId}/statistics`,
  )

export const getDefaultCharacterStatistics = (): CharacterStatistics => ({
  assists: 0,
  deaths: 0,
  gameMode: GameMode.Battle,
  kills: 0,
  playTime: 0,
  rating: { competitiveValue: 0, deviation: 0, value: 0, volatility: 0 },
})

export const getCompetitiveValueByGameMode = (
  statistics: CharacterStatistics[],
  gameMode: GameMode,
) => {
  const statisticByGameMode = statistics.find(s => s.gameMode === gameMode)
  return statisticByGameMode ? statisticByGameMode.rating.competitiveValue : 0
}

export enum CharacterEarningType {
  Exp = 'Exp',
  Gold = 'Gold',
}

// TODO: spec
export const getCharacterEarningStatistics = async (
  characterId: number,
  type: CharacterEarningType,
  from: Date,
) => {
  return (
    await get<ActivityLog<CharacterEarnedMetadata>[]>(
      `/users/self/characters/${characterId}/earning-statistics?${qs.stringify({ from })}`,
    )
  ).reduce((out, l) => {
    const currentEl = out.find(el => el.name === t(`game-mode.${l.metadata.gameMode}`))

    if (currentEl) {
      currentEl.data.push([
        l.createdAt,
        Number.parseInt(type === CharacterEarningType.Exp ? l.metadata.experience : l.metadata.gold, 10),
      ])
    }
    else {
      out.push({
        data: [
          [
            l.createdAt,
            Number.parseInt(
              type === CharacterEarningType.Exp ? l.metadata.experience : l.metadata.gold,
              10,
            ),
          ],
        ],
        name: t(`game-mode.${l.metadata.gameMode}`),
      })
    }

    return out
  }, [] as TimeSeries[])
}

export const getCharacterLimitations = async (characterId: number) =>
  (await get<CharacterLimitations>(`/users/self/characters/${characterId}/limitations`)) || {
    lastRespecializeAt: new Date(),
  }

export const getCharacterCharacteristics = (characterId: number) =>
  get<CharacterCharacteristics>(`/users/self/characters/${characterId}/characteristics`)

export const convertCharacterCharacteristics = (
  characterId: number,
  conversion: CharacteristicConversion,
) =>
  put<CharacterCharacteristics>(`/users/self/characters/${characterId}/characteristics/convert`, {
    conversion,
  })

export const updateCharacterCharacteristics = (
  characterId: number,
  req: CharacterCharacteristics,
) => put<CharacterCharacteristics>(`/users/self/characters/${characterId}/characteristics`, req)

const computeExperienceDistribution = (level: number): number => {
  const [a, b] = experienceForLevelCoefs
  return (level - 1) ** a + b ** (a / 2.0) * (level - 1)
}

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

const computeExperienceTable = () => {
  const table: number[] = [maximumLevel - minimumLevel + 1]

  for (let lvl = minimumLevel; lvl <= 30; lvl += 1) {
    table[lvl - minimumLevel] = Math.trunc(
      (experienceForLevel30 * computeExperienceDistribution(lvl))
      / computeExperienceDistribution(30),
    )
  }

  for (let lvl = 31; lvl <= maximumLevel; lvl += 1) {
    table[lvl - minimumLevel] = table[lvl - minimumLevel - 1] * 2 // changing this require to change how much heirloompoint you get above level 31
  }

  return table
}

const experienceTable = computeExperienceTable()

// TODO: SPEC
export const getLevelByExperience = (exp: number, expTable: number[] = experienceTable): number => {
  return getIndexToIns(expTable, exp)
}

// TODO: spec
// from: src/Application/Characters/Commands/RewardCharacterCommand.cs:67
export const getAutoRetireCount = (exp: number, characterExperience: number) => {
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

export const createDefaultCharacteristic = (): CharacterCharacteristics =>
  createCharacteristics({
    attributes: {
      agility: defaultAgility,
      points: defaultAttributePoints,
      strength: defaultStrength,
    },
    skills: {
      points: defaultSkillPoints,
    },
    weaponProficiencies: {
      points: wppForLevel(minimumLevel),
    },
  })

export const characteristicBonusByKey: Partial<
  Record<CharacteristicKey, { value: number, style: 'percent' | 'decimal' }>
> = {
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

export const computeHealthPoints = (ironFlesh: number, strength: number): number =>
  defaultHealthPoints + ironFlesh * healthPointsForIronFlesh + strength * healthPointsForStrength

// TODO: unit?
export const computeSpeedStats = (
  strength: number,
  athletics: number,
  agility: number,
  totalEncumbrance: number,
  longestWeaponLength: number,
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
    22 + (strength - 3) * 7.5 + (Math.min(strength - 3, 24) * 0.133352143) ** 8,
    650,
  )
  const timeToMaxSpeedWeaponLenghthTerm = Math.max(
    (1.2 * (longestWeaponLength - maxWeaponLength)) / maxWeaponLength,
    0,
  )

  const timeToMaxSpeed
    = 0.8
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

export const getCharacterItems = async (characterId: number) =>
  get<EquippedItem[]>(`/users/self/characters/${characterId}/items`)

export const updateCharacterItems = (characterId: number, items: EquippedItemId[]) =>
  put<EquippedItem[]>(`/users/self/characters/${characterId}/items`, { items })

export const computeOverallPrice = (items: Item[]) =>
  items.reduce((total, item) => total + item.price, 0)

export const computeOverallWeight = (items: Item[]) =>
  items
    .filter(item => ![ItemType.Mount, ItemType.MountHarness].includes(item.type))
    .reduce(
      (total, item) =>
        (total += [ItemType.Arrows, ItemType.Bolts, ItemType.Bullets, ItemType.Thrown].includes(
          item.type,
        )
          ? roundFLoat(item.weight * item.weapons[0].stackAmount)
          : item.weight),
      0,
    )

interface OverallArmor extends Omit<ItemArmorComponent, 'materialType' | 'familyType'> {
  mountArmor: number
}

export const computeOverallArmor = (items: Item[]): OverallArmor =>
  items.reduce(
    (total, item) => {
      if (item.type === ItemType.MountHarness) {
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

// TODO: SPEC
export const computeLongestWeaponLength = (items: Item[]) => {
  return items.reduce((result, item) => {
    if (
      [ItemType.OneHandedWeapon, ItemType.TwoHandedWeapon, ItemType.Polearm].includes(item.type)
      && item.weapons.length !== 0
    ) {
      return Math.max(result, item.weapons[0].length)
    }

    return result
  }, 0 as number)
}

// TODO: handle upgrade items.
// TODO: SPEC
export const computeOverallAverageRepairCostByHour = (items: Item[]) =>
  Math.floor(items.reduce((total, item) => total + computeAverageRepairCostPerHour(item.price), 0))

export const getHeirloomPointByLevel = (level: number) =>
  level < minimumRetirementLevel ? 0 : 2 ** (level - minimumRetirementLevel)

export interface HeirloomPointByLevelAggregation { level: number[], points: number }

export const getHeirloomPointByLevelAggregation = () =>
  range(minimumRetirementLevel, maximumLevel).reduce((out, level) => {
    const points = getHeirloomPointByLevel(level)
    const idx = out.findIndex(item => item.points === points)

    if (idx === -1) {
      out.push({ level: [level], points })
    }
    else {
      out[idx].level.push(level)
    }

    return out
  }, [] as HeirloomPointByLevelAggregation[])

export const getExperienceMultiplierBonus = (multiplier: number) => {
  if (multiplier < maxExperienceMultiplierForGeneration) {
    return experienceMultiplierByGeneration
  }

  return 0
}

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
export const sumExperienceMultiplierBonus = (multiplierA: number, multiplierB: number) => {
  return clamp(multiplierA + multiplierB, 0, maxExperienceMultiplierForGeneration)
}

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

export const getCharacterSLotsSchema = (): {
  key: ItemSlot
  placeholderIcon: string
}[][] => [
  // left col
  [
    {
      key: ItemSlot.Head,
      placeholderIcon: 'item-type-head-armor',
    },
    {
      key: ItemSlot.Shoulder,
      placeholderIcon: 'item-type-shoulder-armor',
    },
    {
      key: ItemSlot.Body,
      placeholderIcon: 'item-type-body-armor',
    },
    {
      key: ItemSlot.Hand,
      placeholderIcon: 'item-type-hand-armor',
    },
    {
      key: ItemSlot.Leg,
      placeholderIcon: 'item-type-leg-armor',
    },
  ],
  // center col
  [
    {
      key: ItemSlot.MountHarness,
      placeholderIcon: 'item-type-mount-harness',
    },
    {
      key: ItemSlot.Mount,
      placeholderIcon: 'item-type-mount',
    },
  ],
  // right col
  [
    {
      key: ItemSlot.Weapon0,
      placeholderIcon: 'weapons',
    },
    {
      key: ItemSlot.Weapon1,
      placeholderIcon: 'weapons',
    },
    {
      key: ItemSlot.Weapon2,
      placeholderIcon: 'weapons',
    },
    {
      key: ItemSlot.Weapon3,
      placeholderIcon: 'weapons',
    },
    {
      key: ItemSlot.WeaponExtra,
      placeholderIcon: 'item-flag-drop-on-change',
    },
  ],
]

export const getCharacterKDARatio = (characterStatistics: CharacterStatistics) => {
  let _deaths = characterStatistics.deaths
  if (characterStatistics.deaths === 0) {
    _deaths = 1
  }

  return (
    Math.round((100 * (characterStatistics.kills + characterStatistics.assists)) / _deaths) / 100
  )
}

export const characterClassToIcon: Record<CharacterClass, string> = {
  [CharacterClass.Archer]: 'item-type-bow',
  [CharacterClass.Cavalry]: 'char-class-cav',
  [CharacterClass.Crossbowman]: 'item-type-crossbow',
  [CharacterClass.Infantry]: 'weapon-class-one-handed-polearm',
  [CharacterClass.MountedArcher]: 'char-class-ha',
  [CharacterClass.Peasant]: 'char-class-peasant',
  [CharacterClass.ShockInfantry]: 'weapon-class-two-handed-axe',
  [CharacterClass.Skirmisher]: 'weapon-class-throwing-spear',
}

// TODO: SPEC
export const getOverallArmorValueBySlot = (
  slot: ItemSlot,
  itemsStats: CharacterOverallItemsStats,
) => {
  const itemSlotToArmorValue: Partial<Record<ItemSlot, CharacterArmorOverall>> = {
    [ItemSlot.Body]: {
      key: CharacterArmorOverallKey.BodyArmor,
      value: itemsStats.bodyArmor,
    },
    [ItemSlot.Hand]: {
      key: CharacterArmorOverallKey.ArmArmor,
      value: itemsStats.armArmor,
    },
    [ItemSlot.Leg]: {
      key: CharacterArmorOverallKey.LegArmor,
      value: itemsStats.legArmor,
    },
    [ItemSlot.Mount]: {
      key: CharacterArmorOverallKey.MountArmor,
      value: itemsStats.mountArmor,
    },
    [ItemSlot.Shoulder]: {
      key: CharacterArmorOverallKey.HeadArmor,
      value: itemsStats.headArmor,
    },
  }

  return slot in itemSlotToArmorValue ? itemSlotToArmorValue[slot] : undefined
}

// TODO: SPEC, more complicated logic?
export const checkUpkeepIsHigh = (userGold: number, upkeepPerHour: number) => {
  return userGold < upkeepPerHour * 2.5
}

export const validateItemNotMeetRequirement = (
  item: Item,
  characterCharacteristics: CharacterCharacteristics,
) => {
  return item.requirement > characterCharacteristics.attributes.strength
}

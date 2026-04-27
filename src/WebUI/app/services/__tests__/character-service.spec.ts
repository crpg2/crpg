import { describe, expect, it, vi } from 'vitest'

import type {
  Character,
  CharacterCharacteristics,
  CharacterLimitations,
  CharacterStatistics,
} from '~/models/character'
import type { GameMode } from '~/models/game-mode'
import type { Item } from '~/models/item'

import { CHARACTERISTIC_CONVERSION } from '~/models/character'
import { GAME_MODE } from '~/models/game-mode'
import { ITEM_TYPE } from '~/models/item'

import {
  activateCharacter,
  attributePointsForLevel,
  canRetireValidate,
  canSetCharacterForTournamentValidate,
  computeHealthPoints,
  computeLongestWeaponLength,
  computeOverallWeight,
  convertCharacterCharacteristics,
  createCharacteristics,
  deactivateCharacter,
  deleteCharacter,
  getCharacterCharacteristics,
  getCharacterKDARatio,
  getCharacterLimitations,
  getCharacters,
  getCharactersByUserId,
  getCharacterStatistics,
  getExperienceForLevel,
  getExperienceMultiplierBonus,
  getHeirloomPointByLevel,
  getHeirloomPointByLevelAggregation,
  getRespecCapability,
  respecializeCharacter,
  retireCharacter,
  setCharacterForTournament,
  skillPointsForLevel,
  updateCharacter,
  updateCharacterCharacteristics,
  validateItemNotMeetRequirement,
  wppForAgility,
  wppForLevel,
  wppForWeaponMaster,
} from '../character-service'

const {
  mockedDeleteUsersSelfCharactersById,
  mockedGetUsersByUserIdCharacters,
  mockedGetUsersSelfCharacters,
  mockedGetUsersSelfCharactersByIdCharacteristics,
  mockedGetUsersSelfCharactersByIdEarningStatistics,
  mockedGetUsersSelfCharactersByIdItems,
  mockedGetUsersSelfCharactersByIdLimitations,
  mockedGetUsersSelfCharactersByIdStatistics,
  mockedPutUsersSelfCharactersById,
  mockedPutUsersSelfCharactersByIdActive,
  mockedPutUsersSelfCharactersByIdCharacteristics,
  mockedPutUsersSelfCharactersByIdCharacteristicsConvert,
  mockedPutUsersSelfCharactersByIdItems,
  mockedPutUsersSelfCharactersByIdRespecialize,
  mockedPutUsersSelfCharactersByIdRetire,
  mockedPutUsersSelfCharactersByIdTournament,
} = vi.hoisted(() => ({
  mockedDeleteUsersSelfCharactersById: vi.fn(),
  mockedGetUsersByUserIdCharacters: vi.fn(),
  mockedGetUsersSelfCharacters: vi.fn(),
  mockedGetUsersSelfCharactersByIdCharacteristics: vi.fn(),
  mockedGetUsersSelfCharactersByIdEarningStatistics: vi.fn(),
  mockedGetUsersSelfCharactersByIdItems: vi.fn(),
  mockedGetUsersSelfCharactersByIdLimitations: vi.fn(),
  mockedGetUsersSelfCharactersByIdStatistics: vi.fn(),
  mockedPutUsersSelfCharactersById: vi.fn(),
  mockedPutUsersSelfCharactersByIdActive: vi.fn(),
  mockedPutUsersSelfCharactersByIdCharacteristics: vi.fn(),
  mockedPutUsersSelfCharactersByIdCharacteristicsConvert: vi.fn(),
  mockedPutUsersSelfCharactersByIdItems: vi.fn(),
  mockedPutUsersSelfCharactersByIdRespecialize: vi.fn(),
  mockedPutUsersSelfCharactersByIdRetire: vi.fn(),
  mockedPutUsersSelfCharactersByIdTournament: vi.fn(),
}))

vi.mock('#api/sdk.gen', () => ({
  deleteUsersSelfCharactersById: mockedDeleteUsersSelfCharactersById,
  getUsersByUserIdCharacters: mockedGetUsersByUserIdCharacters,
  getUsersSelfCharacters: mockedGetUsersSelfCharacters,
  getUsersSelfCharactersByIdCharacteristics: mockedGetUsersSelfCharactersByIdCharacteristics,
  getUsersSelfCharactersByIdEarningStatistics: mockedGetUsersSelfCharactersByIdEarningStatistics,
  getUsersSelfCharactersByIdItems: mockedGetUsersSelfCharactersByIdItems,
  getUsersSelfCharactersByIdLimitations: mockedGetUsersSelfCharactersByIdLimitations,
  getUsersSelfCharactersByIdStatistics: mockedGetUsersSelfCharactersByIdStatistics,
  putUsersSelfCharactersById: mockedPutUsersSelfCharactersById,
  putUsersSelfCharactersByIdActive: mockedPutUsersSelfCharactersByIdActive,
  putUsersSelfCharactersByIdCharacteristics: mockedPutUsersSelfCharactersByIdCharacteristics,
  putUsersSelfCharactersByIdCharacteristicsConvert: mockedPutUsersSelfCharactersByIdCharacteristicsConvert,
  putUsersSelfCharactersByIdItems: mockedPutUsersSelfCharactersByIdItems,
  putUsersSelfCharactersByIdRespecialize: mockedPutUsersSelfCharactersByIdRespecialize,
  putUsersSelfCharactersByIdRetire: mockedPutUsersSelfCharactersByIdRetire,
  putUsersSelfCharactersByIdTournament: mockedPutUsersSelfCharactersByIdTournament,
}))

vi.mock('../item-service', () => ({
  armorTypes: [],
  computeAverageRepairCostPerHour: vi.fn(() => 0),
}))

describe('character service', () => {
  it('getCharacters', async () => {
    const characters = [{ id: 123, name: 'Twilight Sparkle' }] as Character[]
    mockedGetUsersSelfCharacters.mockResolvedValue({ data: characters })

    await expect(getCharacters()).resolves.toEqual(characters)
    expect(mockedGetUsersSelfCharacters).toHaveBeenCalledWith({})
  })

  it('getCharactersByUserId', async () => {
    const characters = [{ id: 45, name: 'Applejack' }] as Character[]
    mockedGetUsersByUserIdCharacters.mockResolvedValue({ data: characters })

    await expect(getCharactersByUserId(77)).resolves.toEqual(characters)
    expect(mockedGetUsersByUserIdCharacters).toHaveBeenCalledWith({ path: { userId: 77 } })
  })

  it('updateCharacter', async () => {
    const updatedCharacter = { id: 123, name: 'Twilight Sparkle' } as Character
    mockedPutUsersSelfCharactersById.mockResolvedValue({ data: updatedCharacter })

    await expect(updateCharacter(123, { name: 'Twilight Sparkle' })).resolves.toEqual({
      data: updatedCharacter,
    })
    expect(mockedPutUsersSelfCharactersById).toHaveBeenCalledWith({
      path: { id: 123 },
      body: { name: 'Twilight Sparkle' },
    })
  })

  it('deleteCharacter', async () => {
    mockedDeleteUsersSelfCharactersById.mockResolvedValue(null)

    await expect(deleteCharacter(123)).resolves.toEqual(null)
    expect(mockedDeleteUsersSelfCharactersById).toHaveBeenCalledWith({ path: { id: 123 } })
  })

  it('respecializeCharacter', async () => {
    const character = { id: 123, name: 'Twilight Sparkle' } as Character
    mockedPutUsersSelfCharactersByIdRespecialize.mockResolvedValue({ data: character })

    await expect(respecializeCharacter(123)).resolves.toEqual({ data: character })
    expect(mockedPutUsersSelfCharactersByIdRespecialize).toHaveBeenCalledWith({ path: { id: 123 } })
  })

  it('getCharacterLimitations', async () => {
    const limitations = { lastRespecializeAt: new Date('2026-01-01T00:00:00.000Z') } as CharacterLimitations
    mockedGetUsersSelfCharactersByIdLimitations.mockResolvedValue({ data: limitations })

    await expect(getCharacterLimitations(123)).resolves.toEqual(limitations)
    expect(mockedGetUsersSelfCharactersByIdLimitations).toHaveBeenCalledWith({
      path: { id: 123 },
    })
  })

  it('setCharacterForTournament', async () => {
    const character = { id: 123, forTournament: true } as Character
    mockedPutUsersSelfCharactersByIdTournament.mockResolvedValue({ data: character })

    await expect(setCharacterForTournament(123)).resolves.toEqual({ data: character })
    expect(mockedPutUsersSelfCharactersByIdTournament).toHaveBeenCalledWith({ path: { id: 123 } })
  })

  it('retireCharacter', async () => {
    const character = { id: 123, generation: 1 } as Character
    mockedPutUsersSelfCharactersByIdRetire.mockResolvedValue({ data: character })

    await expect(retireCharacter(123)).resolves.toEqual({ data: character })
    expect(mockedPutUsersSelfCharactersByIdRetire).toHaveBeenCalledWith({ path: { id: 123 } })
  })

  it('activateCharacter', async () => {
    mockedPutUsersSelfCharactersByIdActive.mockResolvedValue(null)

    await expect(activateCharacter(2)).resolves.toEqual(null)
    expect(mockedPutUsersSelfCharactersByIdActive).toHaveBeenCalledWith({
      path: { id: 2 },
      body: { active: true },
    })
  })

  it('deactivateCharacter', async () => {
    mockedPutUsersSelfCharactersByIdActive.mockResolvedValue(null)

    await expect(deactivateCharacter(2)).resolves.toEqual(null)
    expect(mockedPutUsersSelfCharactersByIdActive).toHaveBeenCalledWith({
      path: { id: 2 },
      body: { active: false },
    })
  })

  it('getCharacterStatistics', async () => {
    const data: Partial<Record<GameMode, CharacterStatistics>> = {
      [GAME_MODE.CRPGBattle]: {
        assists: 1,
        deaths: 2,
        gameMode: GAME_MODE.CRPGBattle,
        kills: 3,
        playTime: 100,
        rating: { competitiveValue: 0, deviation: 0, value: 0, volatility: 0 },
      },
    }
    mockedGetUsersSelfCharactersByIdStatistics.mockResolvedValue({ data })

    await expect(getCharacterStatistics(12)).resolves.toEqual(data)
    expect(mockedGetUsersSelfCharactersByIdStatistics).toHaveBeenCalledWith({ path: { id: 12 } })
  })

  it('getCharacterCharacteristics', async () => {
    const data = createCharacteristics({
      attributes: {
        agility: 1,
        points: 2,
        strength: 3,
      },
    })
    mockedGetUsersSelfCharactersByIdCharacteristics.mockResolvedValue({ data })

    await expect(getCharacterCharacteristics(5)).resolves.toEqual(data)
    expect(mockedGetUsersSelfCharactersByIdCharacteristics).toHaveBeenCalledWith({ path: { id: 5 } })
  })

  it('convertCharacterCharacteristics', async () => {
    const data = createCharacteristics({
      skills: {
        ironFlesh: 2,
      },
    })
    mockedPutUsersSelfCharactersByIdCharacteristicsConvert.mockResolvedValue({ data })

    await expect(
      convertCharacterCharacteristics(6, CHARACTERISTIC_CONVERSION.AttributesToSkills),
    ).resolves.toEqual(data)
    expect(mockedPutUsersSelfCharactersByIdCharacteristicsConvert).toHaveBeenCalledWith({
      path: { id: 6 },
      body: { conversion: CHARACTERISTIC_CONVERSION.AttributesToSkills },
    })
  })

  it('updateCharacterCharacteristics', async () => {
    const data = createCharacteristics({
      weaponProficiencies: {
        bow: 99,
      },
    })
    mockedPutUsersSelfCharactersByIdCharacteristics.mockResolvedValue({ data })

    await expect(updateCharacterCharacteristics(4, data)).resolves.toEqual(data)
    expect(mockedPutUsersSelfCharactersByIdCharacteristics).toHaveBeenCalledWith({
      path: { id: 4 },
      body: data,
    })
  })

  it.each([
    [{ forTournament: false, generation: 0, level: 19 }, true],
    [{ forTournament: false, generation: 1, level: 19 }, false],
    [{ forTournament: false, generation: 0, level: 20 }, false],
    [{ forTournament: true, generation: 0, level: 19 }, false],
  ])('canSetCharacterForTournamentValidate - character: %j', (character, expectation) => {
    expect(canSetCharacterForTournamentValidate(character as Character)).toEqual(expectation)
  })

  it.each([
    [0, 0],
    [2, 388],
    [30, 4420824],
    [31, 8841648],
    [38, 1131730944],
  ])('getExperienceForLevel - level: %s', (level, expectation) => {
    expect(getExperienceForLevel(level)).toEqual(expectation)
  })

  it.each([
    [{ assists: 0, deaths: 0, kills: 0 }, 0],
    [{ assists: 3, deaths: 6, kills: 2 }, 0.83],
    [{ assists: 1, deaths: 1, kills: 1 }, 2],
    [{ assists: 1, deaths: 0, kills: 1 }, 2],
  ])('getCharacterKDARatio - characterStatistics: %j', (characterStatistics, expectation) => {
    expect(getCharacterKDARatio(characterStatistics as CharacterStatistics)).toEqual(expectation)
  })

  it.each([
    [0, 3],
    [1, 3],
    [2, 4],
    [10, 12],
    [30, 32],
    [38, 40],
  ])('skillPointsForLevel - level: %s', (level, expectation) => {
    expect(skillPointsForLevel(level)).toEqual(expectation)
  })

  it.each([
    [0, 52],
    [1, 57],
    [2, 62],
    [10, 111],
    [38, 382],
  ])('wppForLevel - level: %s', (level, expectation) => {
    expect(wppForLevel(level)).toEqual(expectation)
  })

  it.each([
    [0, 0],
    [1, 14],
    [2, 28],
    [10, 140],
    [30, 420],
  ])('wppForAgility - agility: %s', (agility, expectation) => {
    expect(wppForAgility(agility)).toEqual(expectation)
  })

  it.each([
    [0, 0],
    [1, 75],
    [2, 170],
    [10, 1650],
  ])('wppForWeaponMaster - wm: %s', (wm, expectation) => {
    expect(wppForWeaponMaster(wm)).toEqual(expectation)
  })

  it.each([
    [0, false],
    [30, false],
    [31, true],
    [38, true],
  ])('canRetireValidate - level: %s', (level, expectation) => {
    expect(canRetireValidate(level)).toEqual(expectation)
  })

  it.each([
    [0, 1],
    [1, 1],
    [2, 2],
    [10, 10],
    [30, 30],
    [38, 30],
  ])('attributePointsForLevel - level: %s', (level, expectation) => {
    expect(attributePointsForLevel(level)).toEqual(expectation)
  })

  it.each([
    [0, 3, 63],
    [1, 3, 67],
  ])('computeHealthPoints - wm: %s', (ironFlesh, strength, expectation) => {
    expect(computeHealthPoints(ironFlesh, strength)).toEqual(expectation)
  })

  it.each([
    [1, 0],
    [30, 0],
    [31, 1],
    [32, 2],
    [33, 4],
    [34, 8],
    [35, 16],
    [36, 32],
  ])('getHeirloomPointByLevel - level: %s', (level, expectation) => {
    expect(getHeirloomPointByLevel(level)).toEqual(expectation)
  })

  it('getHeirloomPointByLevelAggregation', () => {
    expect(getHeirloomPointByLevelAggregation()).toEqual([
      { level: [31], points: 1 },
      { level: [32], points: 2 },
      { level: [33], points: 4 },
      { level: [34], points: 8 },
      { level: [35], points: 16 },
      { level: [36], points: 32 },
      { level: [37], points: 64 },
      { level: [38], points: 128 },
    ])
  })

  it.each([
    [0, 0.03],
    [1, 0.03],
    [1.47, 0.03],
    [1.48, 0],
    [2, 0],
  ])('getExperienceMultiplierBonus - multiplier: %s', (multiplier, expectation) => {
    expect(getExperienceMultiplierBonus(multiplier)).toEqual(expectation)
  })

  it('createCharacteristics', () => {
    expect(
      createCharacteristics({
        attributes: { points: 3 },
        skills: {
          ironFlesh: 3,
        },
        weaponProficiencies: {
          bow: 44,
        },
      }),
    ).toEqual({
      attributes: {
        agility: 0,
        points: 3,
        strength: 0,
      },
      skills: {
        athletics: 0,
        ironFlesh: 3,
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
        bow: 44,
        crossbow: 0,
        oneHanded: 0,
        points: 0,
        polearm: 0,
        throwing: 0,
        twoHanded: 0,
      },
    })
  })

  it.each([
    [{ requirement: 18 }, { attributes: { strength: 18 } }, false],
    [{ requirement: 17 }, { attributes: { strength: 18 } }, false],
    [{ requirement: 19 }, { attributes: { strength: 18 } }, true],
    [{ requirement: 0 }, { attributes: { strength: 18 } }, false],
  ])(
    'validateItemNotMeetRequirement - item: %j, characterCharacteristics: %j',
    (item, characterCharacteristics, expectation) => {
      expect(
        validateItemNotMeetRequirement(
          item as Item,
          characterCharacteristics as CharacterCharacteristics,
        ),
      ).toEqual(expectation)
    },
  )

  it.each([
    [
      [
        {
          type: ITEM_TYPE.Bolts,
          weapons: [{ stackAmount: 12 }],
          weight: 0.1,
        },
      ],
      1.2,
    ],
    [
      [
        {
          type: ITEM_TYPE.Shield,
          weapons: [{ stackAmount: 70 }],
          weight: 3.8,
        },
      ],
      3.8,
    ],
  ])('computeOverallWeight - items: %j, expectedWeight: %j', (items, expectation) => {
    expect(computeOverallWeight(items as Item[])).toEqual(expectation)
  })

  it.each([
    [[], 0],
    [
      [{ type: ITEM_TYPE.Shield, weapons: [{ length: 50 }] }],
      0,
    ],
    [
      [{ type: ITEM_TYPE.OneHandedWeapon, weapons: [{ length: 80 }] }],
      80,
    ],
    [
      [
        { type: ITEM_TYPE.OneHandedWeapon, weapons: [{ length: 80 }] },
        { type: ITEM_TYPE.TwoHandedWeapon, weapons: [{ length: 120 }] },
        { type: ITEM_TYPE.Polearm, weapons: [{ length: 200 }] },
      ],
      200,
    ],
    [
      [
        { type: ITEM_TYPE.Polearm, weapons: [{ length: 200 }] },
        { type: ITEM_TYPE.OneHandedWeapon, weapons: [{ length: 80 }] },
      ],
      200,
    ],
  ])('computeLongestWeaponLength - items: %j, expected: %j', (items, expectation) => {
    expect(computeLongestWeaponLength(items as Item[])).toEqual(expectation)
  })

  it.each([
    [
      { experience: 100000, forTournament: true },
      { lastRespecializeAt: new Date('2024-01-01T00:00:00.0000000Z') },
      10,
      false,
      {
        enabled: true,
        price: 0,
      },
    ],
    [
      { experience: 10, forTournament: false },
      { lastRespecializeAt: new Date('2024-01-09T00:00:00.0000000Z') },
      100000,
      true,
      {
        enabled: true,
        freeRespecWindowRemain: 0,
        nextFreeAt: 0,
        price: 0,
      },
    ],
  ])(
    'getRespecCapability - character: %j, limitations: %j, gold: %n',
    (character, limitations, gold, isRecentUser, expectation) => {
      vi.useFakeTimers()
      vi.setSystemTime('2024-01-10T00:00:00.0000000Z')

      expect(
        getRespecCapability(
          character as Character,
          limitations as CharacterLimitations,
          gold,
          isRecentUser,
        ),
      ).toMatchObject(expectation)

      vi.useRealTimers()
    },
  )
})

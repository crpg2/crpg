import type { PartialDeep } from 'type-fest'

import { mockDelete, mockGet, mockPut } from 'vi-fetch'

import type {
  Character,
  CharacterCharacteristics,
  CharacterLimitations,
  CharacterStatistics,
} from '~/models/character'
import type { Item } from '~/models/item'

import mockCharacterCharacteristics from '~/__mocks__/character-characteristics.json'
import mockCharacterStatistics from '~/__mocks__/character-statistics.json'
import mockCharacters from '~/__mocks__/characters.json'
import { response } from '~/__mocks__/crpg-client'
import { CharacteristicConversion } from '~/models/character'
import { GameMode } from '~/models/game-mode'
import { ItemType } from '~/models/item'

import type { RespecCapability } from './characters-service'

import {
  activateCharacter,
  attributePointsForLevel,
  canRetireValidate,
  canSetCharacterForTournamentValidate,
  computeHealthPoints,
  computeOverallWeight,
  convertCharacterCharacteristics,
  createCharacteristics,
  deleteCharacter,
  getCharacterCharacteristics,
  getCharacterKDARatio,
  getCharacterLimitations,
  getCharacters,
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
} from './characters-service'

vi.mock('~/services/auth-service', () => ({
  getToken: vi.fn().mockResolvedValue('mockedToken'),
}))

beforeEach(() => {
  vi.useFakeTimers()
})

afterEach(() => {
  vi.useRealTimers()
})

it('getCharacters', async () => {
  mockGet('/users/self/characters').willResolve(response(mockCharacters))

  expect(await getCharacters()).toEqual(mockCharacters)
})

it('updateCharacter', async () => {
  mockPut('/users/self/characters/123').willResolve(response(mockCharacters[0]))

  expect(await updateCharacter(123, { name: 'Twilight Sparkle' })).toEqual(mockCharacters[0])
})

it('deleteCharacter', async () => {
  mockDelete('/users/self/characters/123').willResolve(null, 204)

  expect(await deleteCharacter(123)).toEqual(null)
})

it('respecializeCharacter', async () => {
  mockPut('/users/self/characters/123/respecialize').willResolve(response(mockCharacters[0]))

  expect(await respecializeCharacter(123)).toEqual(mockCharacters[0])
})

it('getCharacterLimitations', async () => {
  mockGet('/users/self/characters/123/limitations').willResolve(response({ test: 'ok' }))

  expect(await getCharacterLimitations(123)).toEqual({ test: 'ok' })
})

it.each<[Partial<Character>, boolean]>([
  [{ forTournament: false, generation: 0, level: 19 }, true],
  [{ forTournament: false, generation: 1, level: 19 }, false],
  [{ forTournament: false, generation: 0, level: 31 }, false],
  [{ forTournament: true, generation: 0, level: 19 }, false],
  [{ forTournament: false, generation: 0, level: 20 }, false],
  [{ forTournament: false, generation: 0, level: 1 }, true],
  [{ forTournament: true, generation: 1, level: 31 }, false],
])('canSetCharacterForTournamentValidate - character: %j', (character, expectation) => {
  expect(canSetCharacterForTournamentValidate(character as Character)).toEqual(expectation)
})

it('setCharacterForTournament', async () => {
  mockPut('/users/self/characters/123/tournament').willResolve(response(mockCharacters[0]))

  expect(await setCharacterForTournament(123)).toEqual(mockCharacters[0])
})

it('retireCharacter', async () => {
  mockPut('/users/self/characters/123/retire').willResolve(response(mockCharacters[0]))

  expect(await retireCharacter(123)).toEqual(mockCharacters[0])
})

it.each([
  [0, false],
  [30, false],
  [31, true],
  [38, true],
])('canRetireValidate - level: %s', (level, expectation) => {
  expect(canRetireValidate(level)).toEqual(expectation)
})

it('activateCharacter', async () => {
  mockPut('/users/self/characters/2/active').willResolve(null, 204)

  expect(await activateCharacter(2, true)).toEqual(null)
})

it('getCharacterStatistics', async () => {
  mockGet('/users/self/characters/12/statistics').willResolve(
    response<Partial<Record<GameMode, CharacterStatistics>>>({
      [GameMode.Battle]: mockCharacterStatistics as CharacterStatistics,
    }),
  )

  expect(await getCharacterStatistics(12)).toEqual({ [GameMode.Battle]: mockCharacterStatistics })
})

it('getCharacterCharacteristics', async () => {
  mockGet('/users/self/characters/5/characteristics').willResolve(
    response<CharacterCharacteristics>(mockCharacterCharacteristics),
  )

  expect(await getCharacterCharacteristics(5)).toEqual(mockCharacterCharacteristics)
})

it('convertCharacterCharacteristics', async () => {
  mockPut('/users/self/characters/6/characteristics/convert').willResolve(
    response<CharacterCharacteristics>(mockCharacterCharacteristics),
  )

  expect(
    await convertCharacterCharacteristics(6, CharacteristicConversion.AttributesToSkills),
  ).toEqual(mockCharacterCharacteristics)
})

it('updateCharacterCharacteristics', async () => {
  mockPut('/users/self/characters/4/characteristics').willResolve(
    response<CharacterCharacteristics>(mockCharacterCharacteristics),
  )

  expect(await updateCharacterCharacteristics(4, mockCharacterCharacteristics)).toEqual(
    mockCharacterCharacteristics,
  )
})

it.each([
  [0, 0],
  [1, 0],
  [2, 388],
  [30, 4420824],
  [31, 8841648], // 30 lvl *2^1
  [36, 282932736], // 30 lvl * 2^6
  [38, 1131730944], // 30 lvl * 2^8
])('getExperienceForLevel - level: %s', (level, expectation) => {
  expect(getExperienceForLevel(level)).toEqual(expectation)
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

it.each<[Partial<CharacterStatistics>, number]>([
  [{ assists: 0, deaths: 0, kills: 0 }, 0],
  [{ assists: 3, deaths: 6, kills: 2 }, 0.83],
  [{ assists: 1, deaths: 1, kills: 1 }, 2],
  [{ assists: 1, deaths: 0, kills: 1 }, 2],
])('getCharacterKDARatio - characterStatistics: %j', (characterStatistics, expectation) => {
  expect(getCharacterKDARatio(characterStatistics as CharacterStatistics)).toEqual(expectation)
})

describe('getRespecCapability', () => {
  it.each<[Partial<Character>, CharacterLimitations, number, boolean, Partial<RespecCapability>]>([
    // tournament char - respec is always  free
    [
      { experience: 100000, forTournament: true },
      {
        lastRespecializeAt: new Date('2024-01-01T00:00:00.0000000Z'),
      },
      10,
      false,
      {
        enabled: true,
        price: 0,
      },
    ],
    // Respec was exactly a week ago + 5 minutes (5 minute margin just in case)
    [
      { experience: 10, forTournament: false },
      {
        lastRespecializeAt: new Date('2024-01-02T23:56:00.0000000Z'),
      },
      100000,
      false,
      {
        enabled: true,
        freeRespecWindowRemain: 0,
        nextFreeAt: 60000,
        price: 0,
      },
    ],
    // recent user
    [
      { experience: 10, forTournament: false },
      {
        lastRespecializeAt: new Date('2024-01-09T00:00:00.0000000Z'),
      },
      100000,
      true,
      {
        enabled: true,
        freeRespecWindowRemain: 0,
        nextFreeAt: 0,
        price: 0,
      },
    ],
    // post respec window - free
    [
      { experience: 141466368, forTournament: false },
      {
        lastRespecializeAt: new Date('2024-01-09T18:00:00.0000000Z'),
      },
      5000,
      false,
      {
        enabled: true,
        freeRespecWindowRemain: 21600000,
        nextFreeAt: 0,
        price: 0,
      },
    ],
    // post respec window - pay
    [
      { experience: 141466368, forTournament: false },
      {
        lastRespecializeAt: new Date('2024-01-09T12:00:00.0000000Z'),
      },
      500000,
      false,
      {
        enabled: true,
        freeRespecWindowRemain: 0,
        nextFreeAt: 561900000,
        price: 113137,
      },
    ],
  ])(
    'getRespecCapability - character: %j, limitations: %j, gold: %n',
    (character, charLimitations, gold, isRecentUser, expectation) => {
      vi.setSystemTime('2024-01-10T00:00:00.0000000Z')

      expect(
        getRespecCapability(character as Character, charLimitations, gold, isRecentUser),
      ).toMatchObject(expectation)
    },
  )

  it('exponential Decay', () => {
    vi.setSystemTime('2024-01-10T01:00:00.0000000Z')

    const exp = 100000000 // 100m
    const gold = 1000000

    const result1 = getRespecCapability(
      { experience: exp, forTournament: false } as Character,
      { lastRespecializeAt: new Date('2024-01-08T01:00:00.0000000Z') },
      gold,
      false,
    )

    const result2 = getRespecCapability(
      { experience: exp, forTournament: false } as Character,
      { lastRespecializeAt: new Date('2024-01-08T00:00:00.0000000Z') },
      gold,
      false,
    )

    expect(result2.price < result1.price).toBeTruthy()
  })
})

it.each<[PartialDeep<Item>, PartialDeep<CharacterCharacteristics>, boolean]>([
  [{ requirement: 18 }, { attributes: { strength: 18 } }, false],
  [{ requirement: 17 }, { attributes: { strength: 18 } }, false],
  [{ requirement: 19 }, { attributes: { strength: 18 } }, true],
  [{ requirement: 0 }, { attributes: { strength: 18 } }, false],
])(
  'validateItemNotMeetRequirement - item: %j, characterCharacteristics: %j, ',
  (item, characterCharacteristics, expectation) => {
    expect(
      validateItemNotMeetRequirement(
        item as Item,
        characterCharacteristics as CharacterCharacteristics,
      ),
    ).toEqual(expectation)
  },
)

it.each<[PartialDeep<Item[]>, number]>([
  [
    [
      {
        type: ItemType.Bolts,
        weapons: [{ stackAmount: 12 }],
        weight: 0.1,
      },
    ],
    1.2,
  ],
  [
    [
      {
        type: ItemType.Shield,
        weapons: [{ stackAmount: 70 }],
        weight: 3.8,
      },
    ],
    3.8,
  ],
])('computeOverallWeight - items: %j, expectedWeight: %j', (items, expectation) => {
  expect(computeOverallWeight(items as Item[])).toEqual(expectation)
})

import type { PartialDeep } from 'type-fest'

import { defu } from 'defu'
import { describe, expect, it, vi } from 'vitest'
import { ref } from 'vue'

import type {
  CharacterCharacteristics,
  CharacteristicKey,
  CharacteristicSectionKey,
  CharacterSkills,
  SkillKey,
} from '~/models/character'

import { useCharacterCharacteristicBuilder } from '../use-character-characteristic-builder'

const {
  mockedComputeHealthPoints,
  mockedCreateDefaultCharacteristic,
  mockedCreateEmptyCharacteristic,
  mockedWppForAgility,
  mockedWppForWeaponMaster,
  createEmptyCharacteristic,
  ATTRIBUTES_TO_SKILLS_RATE,
  SKILLS_TO_ATTRIBUTES_RATE,
} = vi.hoisted(() => {
  const ATTRIBUTES_TO_SKILLS_RATE = 1
  const SKILLS_TO_ATTRIBUTES_RATE = 2
  const WPF_FOR_AGILITY = 1
  const WPF_FOR_WEAPON_MASTER = 10

  const createEmptyCharacteristic = (): CharacterCharacteristics => ({
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

  return {
    mockedComputeHealthPoints: vi.fn(),
    mockedCreateDefaultCharacteristic: vi.fn(),
    mockedCreateEmptyCharacteristic: vi.fn().mockImplementation(createEmptyCharacteristic),
    mockedWppForAgility: vi.fn((value: number) => value * WPF_FOR_AGILITY),
    mockedWppForWeaponMaster: vi.fn((value: number) => value * WPF_FOR_WEAPON_MASTER),
    createEmptyCharacteristic,
    ATTRIBUTES_TO_SKILLS_RATE,
    SKILLS_TO_ATTRIBUTES_RATE,
  }
})

vi.mock('~/services/character-service', () => {
  return {
    computeHealthPoints: mockedComputeHealthPoints,
    createDefaultCharacteristic: mockedCreateDefaultCharacteristic,
    createEmptyCharacteristic: mockedCreateEmptyCharacteristic,
    wppForAgility: mockedWppForAgility,
    wppForWeaponMaster: mockedWppForWeaponMaster,
    ATTRIBUTES_TO_SKILLS_RATE,
    SKILLS_TO_ATTRIBUTES_RATE,
  }
})

const createCharacteristics = (payload?: PartialDeep<CharacterCharacteristics>): CharacterCharacteristics => defu(payload, createEmptyCharacteristic())

describe('useCharacterCharacteristicBuilder', () => {
  describe('canConvert', () => {
    it.each<[string, number, boolean]>([
      ['returns false when no attribute points are available', 0, false],
      ['returns true when 1 attribute point is available', 1, true],
      ['returns true when many attribute points are available', 123, true],
    ])(
      '%s',
      (_description, freeAttributesPoints, expectation) => {
        const { canConvertAttributesToSkills } = useCharacterCharacteristicBuilder(createCharacteristics({ attributes: { points: freeAttributesPoints } }))
        expect(canConvertAttributesToSkills.value).toStrictEqual(expectation)
      },
    )

    it.each<[string, number, boolean]>([
      ['returns false when no skill points are available', 0, false],
      ['returns false when only 1 skill point is available (not enough to convert)', 1, false],
      ['returns true when 2 skill points are available (enough to convert)', 2, true],
      ['returns true when many skill points are available', 12, true],
    ])(
      '%s',
      (_description, freeSkillsPoints, expectation) => {
        const { canConvertSkillsToAttributes } = useCharacterCharacteristicBuilder(createCharacteristics({ skills: { points: freeSkillsPoints } }))
        expect(canConvertSkillsToAttributes.value).toStrictEqual(expectation)
      },
    )
  })

  describe('isDirty', () => {
    it.each<
      [
        string,
        PartialDeep<CharacterCharacteristics>,
        CharacteristicSectionKey,
        CharacteristicKey,
        number,
        boolean,
      ]
    >([
      [
        'becomes dirty when Strength changes from 1 to 2',
        { attributes: { points: 1, strength: 1 } },
        'attributes',
        'strength',
        2,
        true,
      ],
      [
        'remains clean when Strength does not change',
        { attributes: { points: 1, strength: 2 } },
        'attributes',
        'strength',
        2,
        false,
      ],
      [
        'becomes dirty when a skill (Iron Flesh) changes',
        { attributes: { strength: 3 }, skills: { points: 1 } },
        'skills',
        'ironFlesh',
        1,
        true,
      ],
      [
        'becomes dirty when a weapon proficiency (One-Handed) increases by 2',
        { weaponProficiencies: { points: 20 } },
        'weaponProficiencies',
        'oneHanded',
        2,
        true,
      ],
    ])(
      '%s',
      (
        _description,
        characteristics,
        sectionCharacteristicKey,
        characteristicKey,
        value,
        expectation,
      ) => {
        const { onInput, isDirty } = useCharacterCharacteristicBuilder(createCharacteristics(characteristics))
        expect(isDirty.value).toBeFalsy()
        onInput(sectionCharacteristicKey, characteristicKey, value)
        expect(isDirty.value).toStrictEqual(expectation)
      },
    )

    it('isDirty - all at once', () => {
      const { onInput, isDirty } = useCharacterCharacteristicBuilder(
        createCharacteristics({
          attributes: { points: 3 },
          skills: { points: 1 },
          weaponProficiencies: { points: 3 },
        }),
      )

      expect(isDirty.value).toBeFalsy()

      onInput('attributes', 'strength', 3)
      onInput('skills', 'powerDraw', 1)
      onInput('weaponProficiencies', 'polearm', 3)
      expect(isDirty.value).toBeTruthy()

      onInput('attributes', 'strength', 0)
      onInput('skills', 'powerDraw', 0)
      onInput('weaponProficiencies', 'polearm', 0)
      expect(isDirty.value).toBeFalsy()
    })
  })

  describe('isChangeValid', () => {
    it.each<[string, PartialDeep<CharacterCharacteristics>, boolean]>([
      [
        'is valid when all sections have non-negative points and no unmet requirements',
        { attributes: { points: 0 }, skills: { points: 0 }, weaponProficiencies: { points: 0 } },
        true,
      ],
      [
        'is invalid when weapon proficiencies have negative points',
        { attributes: { points: 0 }, skills: { points: 0 }, weaponProficiencies: { points: -10 } },
        false,
      ],
      [
        'is invalid when a skill (Iron Flesh) does not meet its Strength requirement',
        { attributes: { strength: 2 }, skills: { ironFlesh: 1 } },
        false,
      ],
      [
        'is invalid when Power Strike skill requires more Strength than available (5 < 6)',
        { attributes: { strength: 5 }, skills: { powerStrike: 2 } },
        false,
      ],
      [
        'is valid when Power Strike skill requirement is satisfied (Strength 6)',
        { attributes: { strength: 6 }, skills: { powerStrike: 2 } },
        true,
      ],
      [
        'is invalid when Power Draw skill exceeds Strength requirement (12 < 14 required)',
        { attributes: { strength: 12 }, skills: { powerDraw: 5 } },
        false,
      ],
      [
        'is invalid when Mounted Archery skill requires higher Agility (5 < 6 required)',
        { attributes: { agility: 5 }, skills: { mountedArchery: 1 } },
        false,
      ],
      [
        'is valid when Shield skill requirement (Agility 12) is satisfied',
        { attributes: { agility: 12 }, skills: { shield: 2 } },
        true,
      ],
    // TODO: add more cases for currentSkillRequirementsSatisfied
    ])('%s', (_description, characteristics, expectation) => {
      const { isChangeValid } = useCharacterCharacteristicBuilder(createCharacteristics(characteristics))
      expect(isChangeValid.value).toStrictEqual(expectation)
    })
  })

  it.each<[PartialDeep<CharacterCharacteristics>, boolean]>(
    [
      [{ attributes: { strength: 2 }, skills: { ironFlesh: 1 } }, false],
      [{ attributes: { strength: 3 }, skills: { ironFlesh: 1 } }, true],
      [{ attributes: { strength: 5 }, skills: { powerStrike: 2 } }, false],
      [{ attributes: { strength: 6 }, skills: { powerStrike: 2 } }, true],
      [{ attributes: { strength: 12 }, skills: { powerDraw: 5 } }, false],
      [{ attributes: { strength: 6 }, skills: { powerThrow: 2 } }, true],
      [{ attributes: { agility: 6 }, skills: { athletics: 3 } }, false],
      [{ attributes: { agility: 6 }, skills: { athletics: 2 } }, true],
      [{ attributes: { agility: 21 }, skills: { athletics: 7 } }, true],
      [{ attributes: { agility: 3 }, skills: { mountedArchery: 1 } }, false],
      [{ attributes: { agility: 5 }, skills: { mountedArchery: 1 } }, false],
      [{ attributes: { agility: 6 }, skills: { mountedArchery: 1 } }, true],
      [{ attributes: { agility: 6 }, skills: { shield: 1 } }, true],
      [{ attributes: { agility: 6 }, skills: { shield: 3 } }, false],
      [{ attributes: { agility: 6 }, skills: { shield: 2 } }, false],
      [{ attributes: { agility: 12 }, skills: { shield: 2 } }, true],
    ],
  )('currentSkillRequirementsSatisfied - %j, %s', (characteristics, expectation) => {
    const { currentSkillRequirementsSatisfied } = useCharacterCharacteristicBuilder(createCharacteristics(characteristics))
    const [skillKey] = Object.keys(characteristics.skills as Partial<CharacterSkills>)
    expect(currentSkillRequirementsSatisfied(skillKey as SkillKey)).toStrictEqual(expectation)
  })

  it.each<
    [
      string,
      PartialDeep<CharacterCharacteristics>,
      CharacteristicSectionKey,
      CharacteristicKey,
      { modelValue?: number, min?: number, max: number },
    ]
  >([
    //  ATTRIBUTES
    ['attribute (Strength) has max 0 when no free points', { attributes: { points: 0, strength: 0 } }, 'attributes', 'strength', { max: 0 }],
    ['attribute (Strength) has max 1 when 1 free point available', { attributes: { points: 1, strength: 0 } }, 'attributes', 'strength', { max: 1, modelValue: 0 }],
    ['attribute (Strength) is capped at 1 per level step', { attributes: { points: 2, strength: 0 } }, 'attributes', 'strength', { max: 1 }],
    ['attribute (Strength) min and max reflect current value', { attributes: { points: 1, strength: 1 } }, 'attributes', 'strength', { max: 2, min: 1, modelValue: 1 }],
    ['attribute (Agility) range adapts with current value', { attributes: { agility: 1, points: 1 } }, 'attributes', 'agility', { max: 2, min: 1, modelValue: 1 }],

    // SKILLS
    ['skill (Iron Flesh) locked when Strength too low', { attributes: { strength: 1 }, skills: { points: 1 } }, 'skills', 'ironFlesh', { max: 0, min: 0, modelValue: 0 }],
    ['skill (Iron Flesh) available at Strength 3', { attributes: { strength: 3 }, skills: { points: 1 } }, 'skills', 'ironFlesh', { max: 1, min: 0, modelValue: 0 }],
    ['skill (Iron Flesh) fixed at current value (2)', { attributes: { strength: 1 }, skills: { ironFlesh: 2, points: 1 } }, 'skills', 'ironFlesh', { max: 2, min: 2, modelValue: 2 }],
    ['skill (Iron Flesh) unchanged at Strength 6', { attributes: { strength: 6 }, skills: { ironFlesh: 2, points: 1 } }, 'skills', 'ironFlesh', { max: 2, min: 2, modelValue: 2 }],
    ['skill (Iron Flesh) can increase at Strength 9', { attributes: { strength: 9 }, skills: { ironFlesh: 2, points: 1 } }, 'skills', 'ironFlesh', { max: 3, min: 2, modelValue: 2 }],

    ['skill (Power Strike) available at Strength 3', { attributes: { strength: 3 }, skills: { points: 1 } }, 'skills', 'powerStrike', { max: 1, min: 0, modelValue: 0 }],
    ['skill (Power Strike) limited by points', { attributes: { strength: 5 }, skills: { points: 4 } }, 'skills', 'powerStrike', { max: 1, min: 0, modelValue: 0 }],

    ['skill (Power Draw) requires Strength ≥ 3', { attributes: { strength: 3 }, skills: { points: 1 } }, 'skills', 'powerDraw', { max: 1, min: 0, modelValue: 0 }],
    ['skill (Power Draw) locked with Strength 0', { attributes: { strength: 0 }, skills: { points: 4 } }, 'skills', 'powerDraw', { max: 0, min: 0, modelValue: 0 }],

    ['skill (Power Throw) locked at Strength 2', { attributes: { strength: 2 }, skills: { points: 1 } }, 'skills', 'powerThrow', { max: 0, min: 0, modelValue: 0 }],
    ['skill (Power Throw) available at Strength 6', { attributes: { strength: 6 }, skills: { points: 1 } }, 'skills', 'powerThrow', { max: 1, min: 0, modelValue: 0 }],

    ['skill (Athletics) locked at Agility 1', { attributes: { agility: 1 }, skills: { points: 1 } }, 'skills', 'athletics', { max: 0, min: 0, modelValue: 0 }],
    ['skill (Athletics) available at Agility 3', { attributes: { agility: 3 }, skills: { points: 1 } }, 'skills', 'athletics', { max: 1, min: 0, modelValue: 0 }],
    ['skill (Athletics) fixed when no points left', { attributes: { agility: 12 }, skills: { athletics: 4, points: 0 } }, 'skills', 'athletics', { max: 4, min: 4, modelValue: 4 }],
    ['skill (Athletics) can increase with high Agility', { attributes: { agility: 16 }, skills: { athletics: 4, points: 1 } }, 'skills', 'athletics', { max: 5, min: 4, modelValue: 4 }],

    ['skill (Riding) locked at Agility 1', { attributes: { agility: 1 }, skills: { points: 1 } }, 'skills', 'riding', { max: 0, min: 0, modelValue: 0 }],
    ['skill (Weapon Master) locked at Agility 1', { attributes: { agility: 1 }, skills: { points: 1 } }, 'skills', 'weaponMaster', { max: 0, min: 0, modelValue: 0 }],
    ['skill (Mounted Archery) locked at Agility 1', { attributes: { agility: 1 }, skills: { points: 1 } }, 'skills', 'mountedArchery', { max: 0, min: 0, modelValue: 0 }],
    ['skill (Mounted Archery) still locked at Agility 5', { attributes: { agility: 5 }, skills: { points: 1 } }, 'skills', 'mountedArchery', { max: 0, min: 0, modelValue: 0 }],
    ['skill (Mounted Archery) available at Agility 6', { attributes: { agility: 6 }, skills: { points: 1 } }, 'skills', 'mountedArchery', { max: 1, min: 0, modelValue: 0 }],

    ['skill (Shield) available at Agility 9', { attributes: { agility: 9 }, skills: { points: 1 } }, 'skills', 'shield', { max: 1, min: 0, modelValue: 0 }],
    ['skill (Shield) locked when no points left', { attributes: { agility: 9 }, skills: { points: 0 } }, 'skills', 'shield', { max: 0, min: 0, modelValue: 0 }],
    ['skill (Shield) fixed at current value 2', { attributes: { agility: 9 }, skills: { points: 1, shield: 2 } }, 'skills', 'shield', { max: 2, min: 2, modelValue: 2 }],
    ['skill (Shield) can increase at Agility 18', { attributes: { agility: 18 }, skills: { points: 1, shield: 2 } }, 'skills', 'shield', { max: 3, min: 2, modelValue: 2 }],

    // WEAPON PROFICIENCIES
    ['weapon prof (One-Handed) locked with 0 points', { weaponProficiencies: { points: 0 } }, 'weaponProficiencies', 'oneHanded', { max: 0, min: 0, modelValue: 0 }],
    ['weapon prof (One-Handed) still locked with 1 point', { weaponProficiencies: { points: 1 } }, 'weaponProficiencies', 'oneHanded', { max: 0, min: 0, modelValue: 0 }],
    ['weapon prof (One-Handed) still locked with 3 points', { weaponProficiencies: { points: 3 } }, 'weaponProficiencies', 'oneHanded', { max: 0, min: 0, modelValue: 0 }],
    ['weapon prof (One-Handed) available with 217 points', { weaponProficiencies: { points: 217 } }, 'weaponProficiencies', 'oneHanded', { max: 1, min: 0, modelValue: 0 }],
    ['weapon prof (One-Handed) increases gradually from 42', { weaponProficiencies: { oneHanded: 42, points: 79 } }, 'weaponProficiencies', 'oneHanded', { max: 43, min: 42, modelValue: 42 }],
    ['weapon prof (One-Handed) fixed at 60 when capped', { weaponProficiencies: { oneHanded: 60, points: 2 } }, 'weaponProficiencies', 'oneHanded', { max: 60, min: 60, modelValue: 60 }],
    ['weapon prof (One-Handed) unchanged when already maxed', { weaponProficiencies: { oneHanded: 59, points: 6 } }, 'weaponProficiencies', 'oneHanded', { max: 59, min: 59, modelValue: 59 }],
  ])('%s', (_desc, characteristics, sectionKey, key, expectation) => {
    const { getInputProps } = useCharacterCharacteristicBuilder(createCharacteristics(characteristics))
    expect(getInputProps(sectionKey, key)).toMatchObject(expectation)
  })

  describe('onInput', () => {
    it.each<
      [
        string,
        PartialDeep<CharacterCharacteristics>,
        CharacteristicSectionKey,
        CharacteristicKey,
        number,
        PartialDeep<CharacterCharacteristics>,
      ]
    >([
      [
        'increases Strength by 1 when 1 attribute point is available',
        { attributes: { points: 1, strength: 0 } },
        'attributes',
        'strength',
        1,
        { attributes: { points: 0, strength: 1 } },
      ],
      [
        'does not increase Strength when no attribute points are available',
        { attributes: { points: 0, strength: 0 } },
        'attributes',
        'strength',
        1,
        { attributes: { points: 0, strength: 0 } },
      ],
      [
        'does not allow Strength to increase above available points',
        { attributes: { points: 1, strength: 0 } },
        'attributes',
        'strength',
        2,
        { attributes: { points: 1, strength: 0 } },
      ],
      [
        'spends 2 points to increase Strength from 2 to 4',
        { attributes: { points: 2, strength: 2 } },
        'attributes',
        'strength',
        4,
        { attributes: { points: 0, strength: 4 } },
      ],
      [
        'increases Agility by 1 and adds WPP bonus when points available',
        { attributes: { agility: 0, points: 1 } },
        'attributes',
        'agility',
        1,
        { attributes: { agility: 1, points: 0 }, weaponProficiencies: { points: 1 } },
      ],
      [
        'cannot increase Weapon Master skill without required Agility',
        { skills: { points: 1, weaponMaster: 0 } },
        'skills',
        'weaponMaster',
        1,
        { skills: { points: 1, weaponMaster: 0 } },
      ],
      [
        'increases Weapon Master when Agility requirement (3) is satisfied',
        { attributes: { agility: 3 }, skills: { points: 1 } },
        'skills',
        'weaponMaster',
        1,
        { skills: { points: 0, weaponMaster: 1 } },
      ],
      [
        'increases Shield when Agility requirement (6) is satisfied',
        { attributes: { agility: 6 }, skills: { points: 1 } },
        'skills',
        'shield',
        1,
        { skills: { points: 0, shield: 1 } },
      ],
      [
        'does not increase Shield when Agility requirement (5) is not satisfied',
        { attributes: { agility: 5 }, skills: { points: 1 } },
        'skills',
        'shield',
        1,
        { skills: { points: 1, shield: 0 } },
      ],
      [
        'does not allow Athletics to increase beyond requirements',
        { skills: { points: 1, athletics: 0 } },
        'skills',
        'athletics',
        2,
        { skills: { points: 1, athletics: 0 } },
      ],
      [
        'increases Bow proficiency and reduces WPP points accordingly',
        { weaponProficiencies: { points: 22 } },
        'weaponProficiencies',
        'bow',
        1,
        { weaponProficiencies: { points: 12, bow: 1 } },
      ],
      [
        'does not allow Bow proficiency increase if not enough WPP points',
        { weaponProficiencies: { points: 3 } },
        'weaponProficiencies',
        'bow',
        2,
        { weaponProficiencies: { bow: 0 } },
      ],
    ])(
      '%s',
      (_description, initialCharacteristics, characteristicSectionKey, characteristicKey, newValue, expectation) => {
        const { characteristics, onInput } = useCharacterCharacteristicBuilder(createCharacteristics(initialCharacteristics))

        onInput(characteristicSectionKey, characteristicKey, newValue)

        Object.entries(expectation).forEach(([key, values]) => {
          expect(characteristics.value).toEqual(
            expect.objectContaining({
              [key]: expect.objectContaining(values),
            }),
          )
        })
      },
    )
  })

  it('reset', () => {
    const { characteristics, onInput, reset } = useCharacterCharacteristicBuilder(
      createCharacteristics({ attributes: { points: 5 }, skills: { points: 10 } }),
    )

    expect(characteristics.value.attributes.agility).toEqual(0)
    expect(characteristics.value.attributes.points).toEqual(5)

    onInput('attributes', 'agility', 2)

    expect(characteristics.value.attributes.agility).toEqual(2)
    expect(characteristics.value.attributes.points).toEqual(3)

    reset()

    expect(characteristics.value.attributes.agility).toEqual(0)
    expect(characteristics.value.attributes.points).toEqual(5)
  })

  it('free 1 attribute point scenario', () => {
    const {
      canConvertAttributesToSkills,
      characteristics,
      getInputProps,
      isChangeValid,
      onInput,
      isDirty,
    } = useCharacterCharacteristicBuilder(createCharacteristics({ attributes: { points: 1 } }))

    //
    expect(characteristics.value.attributes.points).toEqual(1)
    expect(characteristics.value.attributes.strength).toEqual(0)

    expect(getInputProps('attributes', 'strength').max).toEqual(1)
    expect(getInputProps('attributes', 'agility').max).toEqual(1)

    expect(canConvertAttributesToSkills.value).toBeTruthy()
    expect(isDirty.value).toBeFalsy()

    // +1 strength
    onInput('attributes', 'strength', 1)

    expect(characteristics.value.attributes.points).toEqual(0)
    expect(characteristics.value.attributes.strength).toEqual(1)

    expect(getInputProps('attributes', 'strength')).toEqual({
      max: 1,
      min: 0,
      modelValue: 1,
    })
    expect(getInputProps('attributes', 'agility').max).toEqual(0)

    expect(isDirty.value).toBeTruthy()
    expect(isChangeValid.value).toBeTruthy()
    expect(canConvertAttributesToSkills.value).toBeFalsy()
  })

  it('scenario - 33 lvl, tin can', () => {
    const initialCharacteristics = ref(createCharacteristics({
      attributes: { agility: 3, points: 32, strength: 3 },
      skills: { points: 34 },
      weaponProficiencies: { points: 322 },
    }))

    const {
      canConvertAttributesToSkills,
      canConvertSkillsToAttributes,
      characteristics,
      isChangeValid,
      onInput,
      isDirty,
    } = useCharacterCharacteristicBuilder(initialCharacteristics)

    onInput('attributes', 'strength', 30)
    onInput('attributes', 'agility', 8)

    expect(characteristics.value.attributes.points).toEqual(0)
    expect(characteristics.value.attributes.agility).toEqual(8)
    expect(characteristics.value.attributes.strength).toEqual(30)

    // convert skills to attribute
    for (let i = 0; i < 4; i++) {
      initialCharacteristics.value.attributes.points += ATTRIBUTES_TO_SKILLS_RATE
      initialCharacteristics.value.skills.points -= SKILLS_TO_ATTRIBUTES_RATE
    }

    expect(characteristics.value.attributes.points).toEqual(4)

    onInput('attributes', 'agility', 12)

    expect(characteristics.value.attributes.points).toEqual(0)
    expect(characteristics.value.attributes.agility).toEqual(12)
    expect(characteristics.value.attributes.strength).toEqual(30)

    expect(characteristics.value.weaponProficiencies.points).toEqual(331) // of agi

    onInput('skills', 'ironFlesh', 10)
    onInput('skills', 'powerStrike', 10)
    onInput('skills', 'athletics', 4)
    onInput('skills', 'weaponMaster', 2)

    expect(characteristics.value.weaponProficiencies.points).toEqual(351) // of wm

    onInput('weaponProficiencies', 'twoHanded', 35)

    expect(characteristics.value.weaponProficiencies.points).toEqual(1)

    expect(characteristics.value).toEqual({
      attributes: {
        agility: 12,
        points: 0,
        strength: 30,
      },
      skills: {
        athletics: 4,
        ironFlesh: 10,
        mountedArchery: 0,
        points: 0,
        powerDraw: 0,
        powerStrike: 10,
        powerThrow: 0,
        riding: 0,
        shield: 0,
        weaponMaster: 2,
      },
      weaponProficiencies: {
        bow: 0,
        crossbow: 0,
        oneHanded: 0,
        points: 1,
        polearm: 0,
        throwing: 0,
        twoHanded: 35,
      },
    })

    expect(isDirty.value).toBeTruthy()
    expect(isChangeValid.value).toBeTruthy()

    expect(canConvertSkillsToAttributes.value).toBeFalsy()
    expect(canConvertAttributesToSkills.value).toBeFalsy()
  })
})

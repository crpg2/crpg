import type { PartialDeep } from 'type-fest'

import { defu } from 'defu'
import { describe, expect, it, vi } from 'vitest'
import { ref } from 'vue'

import type {
  CharacterCharacteristics,
  CharacteristicKey,
  CharacteristicSectionKey,
} from '~/models/character'

import { useCharacterCharacteristicBuilder } from '../use-character-characteristic-builder'

const {
  mockedComputeHealthPoints,
  mockedCreateDefaultCharacteristic,
  mockedCreateEmptyCharacteristic,
  mockedWppForAgility,
  mockedWppForWeaponMaster,
  mockedGetCharacteristicCost,
  mockedSkillRequirementsSatisfied,
  mockedCharacteristicRequirementsSatisfied,
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
    mockedGetCharacteristicCost: vi.fn((_, __, value: number) => value),
    mockedSkillRequirementsSatisfied: vi.fn().mockReturnValue(true),
    mockedCharacteristicRequirementsSatisfied: vi.fn().mockReturnValue(true),
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
    getCharacteristicCost: mockedGetCharacteristicCost,
    skillRequirementsSatisfied: mockedSkillRequirementsSatisfied,
    characteristicRequirementsSatisfied: mockedCharacteristicRequirementsSatisfied,
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
    it('is valid when all sections have non-negative points and no unmet requirements', () => {
      const { isChangeValid } = useCharacterCharacteristicBuilder(createCharacteristics({ attributes: { points: 0 }, skills: { points: 0 }, weaponProficiencies: { points: 0 } }))
      expect(isChangeValid.value).toStrictEqual(true)
    })

    it('is invalid when weapon proficiencies have negative points', () => {
      const { isChangeValid } = useCharacterCharacteristicBuilder(createCharacteristics({ attributes: { points: 0 }, skills: { points: 0 }, weaponProficiencies: { points: -10 } }))
      expect(isChangeValid.value).toStrictEqual(false)
    })

    it('is invalid when some skill does not meet its Strength requirement', () => {
      mockedSkillRequirementsSatisfied.mockReturnValueOnce(false)
      const { isChangeValid } = useCharacterCharacteristicBuilder(createCharacteristics({ attributes: { strength: 2 }, skills: { ironFlesh: 1 } }))
      expect(isChangeValid.value).toStrictEqual(false)
    })
  })

  describe('getInputProps', () => {
    it('attribute (Strength) has max 0 when no free points', () => {
      const { getInputProps } = useCharacterCharacteristicBuilder(createCharacteristics({ attributes: { points: 0, strength: 0 } }))
      expect(getInputProps('attributes', 'strength')).toMatchObject({ max: 0 })
    })

    it('attribute (Strength) has max 1 when 1 free point available', () => {
      const { getInputProps } = useCharacterCharacteristicBuilder(createCharacteristics({ attributes: { points: 1, strength: 0 } }))
      expect(getInputProps('attributes', 'strength')).toMatchObject({ max: 1, modelValue: 0 })
    })

    it('skill (Iron Flesh) locked when Strength too low', () => {
      mockedCharacteristicRequirementsSatisfied.mockReturnValueOnce(false)
      const characteristics = createCharacteristics({ attributes: { points: 1, strength: 0 } })
      const { getInputProps } = useCharacterCharacteristicBuilder(characteristics)
      expect(getInputProps('attributes', 'strength')).toMatchObject({ max: 0, min: 0, modelValue: 0 })
      expect(mockedCharacteristicRequirementsSatisfied).toHaveBeenCalledWith('attributes', 'strength', 1, characteristics)
    })
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

    it('does not increase Shield when Agility requirement is not satisfied', () => {
      mockedCharacteristicRequirementsSatisfied.mockReturnValueOnce(false)
      const { characteristics, onInput } = useCharacterCharacteristicBuilder(createCharacteristics({ attributes: { agility: 5 }, skills: { points: 1, shield: 0 } }))
      onInput('skills', 'shield', 1)
      expect(characteristics.value).toEqual(expect.objectContaining({
        skills: expect.objectContaining({
          points: 1,
          shield: 0,
        }),
      }))
    })
  })

  describe('bulk field actions', () => {
    it('onResetField restores changed field to initial value', () => {
      const { characteristics, onInput, onResetField } = useCharacterCharacteristicBuilder(
        createCharacteristics({ attributes: { points: 5, strength: 2 } }),
      )

      onInput('attributes', 'strength', 4)
      expect(characteristics.value.attributes.strength).toEqual(4)
      expect(characteristics.value.attributes.points).toEqual(3)

      onResetField('attributes', 'strength')

      expect(characteristics.value.attributes.strength).toEqual(2)
      expect(characteristics.value.attributes.points).toEqual(5)
    })

    it('onFillField spends all available points for the selected field', () => {
      const { characteristics, onFillField } = useCharacterCharacteristicBuilder(
        createCharacteristics({ attributes: { points: 3, strength: 0 } }),
      )

      onFillField('attributes', 'strength')

      expect(characteristics.value.attributes.strength).toEqual(3)
      expect(characteristics.value.attributes.points).toEqual(0)
    })

    it('onFillField stops when next step violates requirements', () => {
      mockedCharacteristicRequirementsSatisfied.mockImplementation(
        (_section, _key, value: number) => value <= 2,
      )

      const { characteristics, onFillField } = useCharacterCharacteristicBuilder(
        createCharacteristics({ skills: { points: 5, shield: 0 } }),
      )

      onFillField('skills', 'shield')

      expect(characteristics.value.skills.shield).toEqual(2)
      expect(characteristics.value.skills.points).toEqual(3)

      mockedCharacteristicRequirementsSatisfied.mockReturnValue(true)
    })

    it('onInputWithAutoClamp sets the value when enough points available', () => {
      const { characteristics, onInputWithAutoClamp } = useCharacterCharacteristicBuilder(
        createCharacteristics({ attributes: { points: 5, strength: 0 } }),
      )

      onInputWithAutoClamp('attributes', 'strength', 3)

      expect(characteristics.value.attributes.strength).toEqual(3)
      expect(characteristics.value.attributes.points).toEqual(2)
    })

    it('onInputWithAutoClamp clamps to max available when not enough points', () => {
      const { characteristics, onInputWithAutoClamp } = useCharacterCharacteristicBuilder(
        createCharacteristics({ attributes: { points: 1, strength: 0 } }),
      )

      onInputWithAutoClamp('attributes', 'strength', 5)

      expect(characteristics.value.attributes.strength).toEqual(1)
      expect(characteristics.value.attributes.points).toEqual(0)
    })

    it('onInputWithAutoClamp clamps to initial value when no points available', () => {
      const { characteristics, onInputWithAutoClamp } = useCharacterCharacteristicBuilder(
        createCharacteristics({ attributes: { points: 0, strength: 2 } }),
      )

      onInputWithAutoClamp('attributes', 'strength', 5)

      expect(characteristics.value.attributes.strength).toEqual(2)
      expect(characteristics.value.attributes.points).toEqual(0)
    })

    it('onInputWithAutoClamp respects requirements when clamping', () => {
      mockedCharacteristicRequirementsSatisfied.mockImplementation(
        (_section, _key, value: number) => value <= 2,
      )

      const { characteristics, onInputWithAutoClamp } = useCharacterCharacteristicBuilder(
        createCharacteristics({ skills: { points: 10, shield: 0 } }),
      )

      onInputWithAutoClamp('skills', 'shield', 5)

      expect(characteristics.value.skills.shield).toEqual(2)
      expect(characteristics.value.skills.points).toEqual(8)

      mockedCharacteristicRequirementsSatisfied.mockReturnValue(true)
    })

    it('onInputWithAutoClamp with agility increases WPP bonus', () => {
      const { characteristics, onInputWithAutoClamp } = useCharacterCharacteristicBuilder(
        createCharacteristics({ attributes: { agility: 0, points: 5 } }),
      )

      onInputWithAutoClamp('attributes', 'agility', 3)

      expect(characteristics.value.attributes.agility).toEqual(3)
      expect(characteristics.value.weaponProficiencies.points).toEqual(3)
      expect(characteristics.value.attributes.points).toEqual(2)
    })

    it('onInputWithAutoClamp with multiple increments (simulating loop)', () => {
      const { characteristics, onInputWithAutoClamp } = useCharacterCharacteristicBuilder(
        createCharacteristics({ attributes: { points: 3, strength: 0 } }),
      )

      onInputWithAutoClamp('attributes', 'strength', 10)

      expect(characteristics.value.attributes.strength).toEqual(3)
      expect(characteristics.value.attributes.points).toEqual(0)
    })
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
      costToIncrease: 1,
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

    expect(characteristics.value.weaponProficiencies.points).toEqual(316)

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
        points: 316,
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

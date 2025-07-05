import type { Ref } from 'vue'

import { weaponProficiencyCostCoefs } from '~root/data/constants.json'

import type {
  CharacterCharacteristics,
  CharacteristicKey,
  CharacteristicSectionKey,
  SkillKey,
} from '~/models/character'

import {
  computeHealthPoints,
  createDefaultCharacteristic,
  createEmptyCharacteristic,
  wppForAgility,
  wppForWeaponMaster,
} from '~/services/character-service'
import { applyPolynomialFunction } from '~/utils/math'
import { mergeObjectWithSum } from '~/utils/object'

const characteristicCost = (
  characteristicSectionKey: CharacteristicSectionKey,
  characteristicKey: CharacteristicKey, // TODO:
  characteristic: number,
): number => {
  if (characteristicSectionKey === 'weaponProficiencies') {
    return Math.floor(applyPolynomialFunction(characteristic, weaponProficiencyCostCoefs))
  }

  return characteristic
}

const skillRequirementsSatisfied = (
  skillKey: SkillKey,
  skill: number,
  characteristics: CharacterCharacteristics,
): boolean => {
  switch (skillKey) {
    case 'ironFlesh':
    case 'powerStrike':
    case 'powerDraw':
    case 'powerThrow':
      return skill <= Math.floor(characteristics.attributes.strength / 3) // TODO: move "3" to constants.json

    case 'athletics':
    case 'riding':
    case 'weaponMaster':
      return skill <= Math.floor(characteristics.attributes.agility / 3)

    case 'mountedArchery':
    case 'shield':
      return skill <= Math.floor(characteristics.attributes.agility / 6)

    default:
      return false
  }
}

const characteristicRequirementsSatisfied = (
  characteristicSectionKey: CharacteristicSectionKey,
  characteristicKey: CharacteristicKey,
  characteristicValue: number,
  characteristics: CharacterCharacteristics,
): boolean => {
  switch (characteristicSectionKey) {
    case 'skills':
      return skillRequirementsSatisfied(
        characteristicKey as SkillKey,
        characteristicValue,
        characteristics,
      )
    default:
      return true
  }
}

interface CharacterCharacteristicsContext {
  characterCharacteristics: Ref<CharacterCharacteristics>
  // setCharacterCharacteristics: (payload: CharacterCharacteristics) => void
  loadCharacterCharacteristics: (delay: number, characterId: number) => void
}

const characterCharacteristicsKey: InjectionKey<CharacterCharacteristicsContext> = Symbol('CharacterCharacteristics')

export const useCharacterCharacteristicProvider = (ctx: CharacterCharacteristicsContext) => {
  provide(characterCharacteristicsKey, ctx)
}

export const useCharacterCharacteristic = () => {
  const { characterCharacteristics, loadCharacterCharacteristics } = injectStrict(characterCharacteristicsKey)

  const healthPoints = computed(() => computeHealthPoints(characterCharacteristics.value.skills.ironFlesh, characterCharacteristics.value.attributes.strength))

  return {
    characterCharacteristics,
    loadCharacterCharacteristics,
    healthPoints,
  }
}

export const useCharacterCharacteristicBuilder = (
  characteristicsInitial: MaybeRefOrGetter<CharacterCharacteristics>,
) => {
  const characteristicsDelta = ref<CharacterCharacteristics>(createEmptyCharacteristic())
  const characteristicDefault = ref<CharacterCharacteristics>(createDefaultCharacteristic())

  const characteristics = computed<CharacterCharacteristics>(() => Object.entries(toValue(characteristicsInitial)).reduce(
    (obj, [key, values]: [string | CharacteristicSectionKey, Partial<CharacterCharacteristics>],
    ) => ({
      ...obj,
      [key]: mergeObjectWithSum(obj[key as CharacteristicSectionKey] as any, values as any),
    }),
    { ...characteristicsDelta.value },
  ))

  const currentSkillRequirementsSatisfied = (skillKey: SkillKey): boolean =>
    skillRequirementsSatisfied(
      skillKey,
      characteristics.value.skills[skillKey],
      characteristics.value,
    )

  const allSkillsRequirementsSatisfied = computed(() =>
    Object.keys(characteristics.value.skills)
      .filter(skillKey => skillKey !== 'points')
      .every(skillKey => currentSkillRequirementsSatisfied(skillKey as SkillKey)),
  )

  const wasChangeMade = computed(() =>
    characteristicsDelta.value.attributes.points !== 0
    || characteristicsDelta.value.skills.points !== 0
    || characteristicsDelta.value.weaponProficiencies.points !== 0,
  )

  const isChangeValid = computed(() =>
    characteristics.value.attributes.points >= 0
    && characteristics.value.skills.points >= 0
    && characteristics.value.weaponProficiencies.points >= 0
    && allSkillsRequirementsSatisfied.value,
  )

  const canConvertAttributesToSkills = computed(() => characteristics.value.attributes.points >= 1)

  const canConvertSkillsToAttributes = computed(() => characteristics.value.skills.points >= 2)

  const onInput = (
    characteristicSectionKey: CharacteristicSectionKey,
    characteristicKey: CharacteristicKey,
    newCharacteristicValue: number,
  ): void => {
    const characteristicInitialSection = toValue(characteristicsInitial)[characteristicSectionKey]
    const characteristicDeltaSection = characteristicsDelta.value[characteristicSectionKey]
    const characteristicSection = characteristics.value[characteristicSectionKey]

    // @ts-expect-error FIXME: typeguard
    const oldCharacteristicValue = characteristicSection[characteristicKey]

    const costToIncrease = characteristicCost(characteristicSectionKey, characteristicKey, oldCharacteristicValue) - characteristicCost(characteristicSectionKey, characteristicKey, newCharacteristicValue)

    characteristicDeltaSection.points += costToIncrease
    // @ts-expect-error FIXME: typeguard
    characteristicDeltaSection[characteristicKey] = newCharacteristicValue - characteristicInitialSection[characteristicKey]

    if (characteristicKey === 'agility') {
      characteristicsDelta.value.weaponProficiencies.points += wppForAgility(newCharacteristicValue) - wppForAgility(oldCharacteristicValue)
    }
    else if (characteristicKey === 'weaponMaster') {
      characteristicsDelta.value.weaponProficiencies.points += wppForWeaponMaster(newCharacteristicValue) - wppForWeaponMaster(oldCharacteristicValue)
    }
  }

  // TODO: iter count, unit
  const getInputProps = (
    characteristicSectionKey: CharacteristicSectionKey,
    characteristicKey: CharacteristicKey,
    noLimit = false, // TODO: FIXME: need a name, for builder
  ): { modelValue: number, min: number, max: number } => {
    //
    const initialValue = noLimit
      ? (characteristicDefault.value[characteristicSectionKey] as any)[characteristicKey]
      : (toValue(characteristicsInitial)[characteristicSectionKey] as any)[characteristicKey]

    const value = (characteristics.value[characteristicSectionKey] as any)[characteristicKey]
    const points = characteristics.value[characteristicSectionKey].points

    const costToIncrease = characteristicCost(characteristicSectionKey, characteristicKey, value + 1) - characteristicCost(characteristicSectionKey, characteristicKey, value)

    const requirementsSatisfied = characteristicRequirementsSatisfied(
      characteristicSectionKey,
      characteristicKey,
      value + 1,
      characteristics.value,
    )

    return {
      max: value + (costToIncrease <= points && requirementsSatisfied ? 1 : 0),
      min: initialValue, // TODO: can to default for builder
      modelValue: value,
    }
  }

  // TODO: FIXME iter count, unit
  const onResetField = (
    characteristicSectionKey: CharacteristicSectionKey,
    characteristicKey: CharacteristicKey,
  ) => {
    for (let i = 1; i <= 300; i++) {
      const inputProps = getInputProps(characteristicSectionKey, characteristicKey)
      onInput(characteristicSectionKey, characteristicKey, inputProps.min!)
    }
  }

  const onFullFillField = (
    characteristicSectionKey: CharacteristicSectionKey,
    characteristicKey: CharacteristicKey,
  ) => {
    for (let i = 1; i <= 300; i++) {
      const inputProps = getInputProps(characteristicSectionKey, characteristicKey)
      onInput(characteristicSectionKey, characteristicKey, inputProps.max)
    }
  }

  const convertAttributeToSkills = () => {
    toValue(characteristicsInitial).attributes.points -= 1
    toValue(characteristicsInitial).skills.points += 2
  }

  const convertSkillsToAttribute = () => {
    toValue(characteristicsInitial).attributes.points += 1
    toValue(characteristicsInitial).skills.points -= 2
  }

  const reset = () => {
    characteristicsDelta.value = createEmptyCharacteristic()
  }

  return {
    canConvertAttributesToSkills,
    canConvertSkillsToAttributes,
    characteristics,
    convertAttributeToSkills,
    convertSkillsToAttribute,
    currentSkillRequirementsSatisfied,
    getInputProps,
    isChangeValid,
    onFullFillField,
    onInput,
    onResetField,
    reset,
    wasChangeMade,
  }
}

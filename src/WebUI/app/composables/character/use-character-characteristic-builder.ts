import type { MaybeRefOrGetter } from 'vue'

import { computed, ref, toValue } from 'vue'

import type {
  CharacterCharacteristics,
  CharacteristicKey,
  CharacteristicSectionKey,
} from '~/models/character'
import type { CharacteristicState } from '~/services/character-service'

import {
  allCharacteristicRequirementSatisfied,
  ATTRIBUTES_TO_SKILLS_RATE,
  characteristicRequirementSatisfied,
  computeHealthPoints,
  createDefaultCharacteristic,
  createEmptyCharacteristic,
  getCharacteristicCost,
  SKILLS_TO_ATTRIBUTES_RATE,
  wppForAgility,
  wppForWeaponMaster,
} from '~/services/character-service'
import { mergeObjectWithSum, objectEntries } from '~/utils/object'

export const useCharacterCharacteristicBuilder = (
  characteristicsInitial: MaybeRefOrGetter<CharacterCharacteristics>,
) => {
  const delta = ref<CharacterCharacteristics>(createEmptyCharacteristic())

  function reset() {
    delta.value = createEmptyCharacteristic()
  }

  const isDirty = computed(() => Object.values(delta.value).some(section => section.points !== 0))

  const characteristics = computed<CharacterCharacteristics>(() => {
    return objectEntries(toValue(characteristicsInitial)).reduce(
      (obj, [key, values]) => {
        return {
          ...obj,
          [key]: mergeObjectWithSum(
            obj[key],
            values as unknown as Record<string, number>,
          ),
        }
      },
      { ...delta.value },
    )
  })

  const isChangeValid = computed(() =>
    Object.values(characteristics.value).every(section => section.points >= 0)
    && allCharacteristicRequirementSatisfied(characteristics.value),
  )

  const canConvertAttributesToSkills = computed(() => characteristics.value.attributes.points >= ATTRIBUTES_TO_SKILLS_RATE)

  const canConvertSkillsToAttributes = computed(() => characteristics.value.skills.points >= SKILLS_TO_ATTRIBUTES_RATE)

  const defaults = createDefaultCharacteristic()

  const getCharacteristicState = (
    section: CharacteristicSectionKey,
    key: CharacteristicKey,
    noMinLimit = false,
  ): CharacteristicState => {
    const initialValue = noMinLimit
      ? (defaults[section] as any)[key] as number
      : (toValue(characteristicsInitial)[section] as any)[key] as number

    const value = (characteristics.value[section] as any)[key] as number

    const costToIncrease = getCharacteristicCost(section, key, value + 1) - getCharacteristicCost(section, key, value)

    const requirement = characteristicRequirementSatisfied(section, key, value, characteristics.value)
    const nextRequirement = characteristicRequirementSatisfied(section, key, value + 1, characteristics.value)

    return {
      value,
      min: initialValue,
      max: value + ((costToIncrease <= characteristics.value[section].points && (nextRequirement !== null ? nextRequirement.satisfied : true)) ? 1 : 0),
      requirement,
      costToIncrease,
    }
  }

  const onInput = (
    section: CharacteristicSectionKey,
    key: CharacteristicKey,
    newValue: number,
  ): void => {
    const deltaSection = delta.value[section]
    const oldValue = characteristics.value[section][key as keyof typeof deltaSection]

    const oldCost = getCharacteristicCost(section, key, oldValue)
    const newCost = getCharacteristicCost(section, key, newValue)
    const costToIncrease = newCost - oldCost

    if (costToIncrease > characteristics.value[section].points) {
      return
    }

    const requirement = characteristicRequirementSatisfied(section, key, newValue, characteristics.value)

    if (requirement !== null && requirement.satisfied === false) {
      return
    }

    deltaSection.points += (oldCost - newCost)
    deltaSection[key as keyof typeof deltaSection] = newValue - toValue(characteristicsInitial)[section][key as keyof typeof deltaSection]

    if (key === 'agility') {
      delta.value.weaponProficiencies.points += wppForAgility(newValue) - wppForAgility(oldValue)
    }
    else if (key === 'weaponMaster') {
      delta.value.weaponProficiencies.points += wppForWeaponMaster(newValue) - wppForWeaponMaster(oldValue)
    }
  }

  const onInputWithAutoClamp = (section: CharacteristicSectionKey, key: CharacteristicKey, targetValue: number): void => {
    const initialValue = (toValue(characteristicsInitial)[section] as any)[key]
    let valueToTry = Math.min(targetValue, 250) // avoid long loop

    while (valueToTry >= initialValue) {
      onInput(section, key, valueToTry)
      const afterValue = (characteristics.value[section] as any)[key]

      if (afterValue === valueToTry) {
        return
      }

      valueToTry--
    }
  }

  const onResetField = (section: CharacteristicSectionKey, key: CharacteristicKey): void => {
    const { value, min } = getCharacteristicState(section, key)
    if (value > min) {
      onInput(section, key, min)
    }
  }

  const onFillField = (section: CharacteristicSectionKey, key: CharacteristicKey): void => {
    let { value, max } = getCharacteristicState(section, key)
    while (max > value) {
      onInput(section, key, max)
      const { value: newValue, max: newMax } = getCharacteristicState(section, key)
      max = newMax
      value = newValue
    }
  }

  const healthPoints = computed(() => computeHealthPoints(
    characteristics.value.skills.ironFlesh,
    characteristics.value.attributes.strength,
  ))

  return {
    canConvertAttributesToSkills,
    canConvertSkillsToAttributes,
    characteristics,
    getCharacteristicState,
    isChangeValid,
    onInput,
    onInputWithAutoClamp,
    onFillField,
    onResetField,
    reset,
    isDirty,
    healthPoints,
  }
}

import type { MaybeRefOrGetter } from 'vue'

import { computed, ref, toValue } from 'vue'

import type {
  CharacterCharacteristics,
  CharacteristicKey,
  CharacteristicSectionKey,
  SkillKey,
} from '~/models/character'

import {
  ATTRIBUTES_TO_SKILLS_RATE,
  characteristicRequirementsSatisfied,
  computeHealthPoints,
  createDefaultCharacteristic,
  createEmptyCharacteristic,
  getCharacteristicCost,
  skillRequirementsSatisfied,
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

  const isChangeValid = computed(() =>
    Object.values(characteristics.value).every(section => section.points >= 0) && allSkillsRequirementsSatisfied.value,
  )

  const canConvertAttributesToSkills = computed(() => characteristics.value.attributes.points >= ATTRIBUTES_TO_SKILLS_RATE)

  const canConvertSkillsToAttributes = computed(() => characteristics.value.skills.points >= SKILLS_TO_ATTRIBUTES_RATE)

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

    const requirementsSatisfied = characteristicRequirementsSatisfied(section, key, newValue, characteristics.value)

    if (!requirementsSatisfied) {
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

  const defaults = createDefaultCharacteristic()

  const getInputProps = (
    section: CharacteristicSectionKey,
    key: CharacteristicKey,
    noLimit = false, // TODO: FIXME: need a name, for builder page
  ): { modelValue: number, min: number, max: number } => {
    const initialValue = noLimit
      ? (defaults[section] as any)[key]
      : (toValue(characteristicsInitial)[section] as any)[key]

    const value = (characteristics.value[section] as any)[key]

    const costToIncrease = getCharacteristicCost(section, key, value + 1) - getCharacteristicCost(section, key, value)

    const requirementsSatisfied = characteristicRequirementsSatisfied(section, key, value + 1, characteristics.value)

    return {
      max: value + ((costToIncrease <= characteristics.value[section].points && requirementsSatisfied) ? 1 : 0),
      min: initialValue, // TODO: can to default for builder
      modelValue: value,
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
    currentSkillRequirementsSatisfied,
    getInputProps,
    isChangeValid,
    onInput,
    reset,
    isDirty,
    healthPoints,
  }
}

// TODO: FIXME iter count, unit
// const onResetField = (
//   characteristicSectionKey: CharacteristicSectionKey,
//   characteristicKey: CharacteristicKey,
// ) => {
//   for (let i = 1; i <= 300; i++) {
//     const inputProps = getInputProps(characteristicSectionKey, characteristicKey)
//     onInput(characteristicSectionKey, characteristicKey, inputProps.min!)
//   }
// }

// const onFullFillField = (
//   characteristicSectionKey: CharacteristicSectionKey,
//   characteristicKey: CharacteristicKey,
// ) => {
//   for (let i = 1; i <= 300; i++) {
//     const inputProps = getInputProps(characteristicSectionKey, characteristicKey)
//     onInput(characteristicSectionKey, characteristicKey, inputProps.max)
//   }
// }

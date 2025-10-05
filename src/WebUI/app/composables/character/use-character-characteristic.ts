import { weaponProficiencyCostCoefs } from '~root/data/constants.json'
import { computed, ref, toValue } from 'vue'

import type {
  CharacterCharacteristics,
  CharacteristicConversion,
  CharacteristicKey,
  CharacteristicSectionKey,
  SkillKey,
} from '~/models/character'

import { useAsyncCallback } from '~/composables/utils/use-async-callback'
import { CHARACTER_QUERY_KEYS } from '~/queries'
import {
  computeHealthPoints,
  convertCharacterCharacteristics,
  createDefaultCharacteristic,
  createEmptyCharacteristic,
  getCharacterCharacteristics,
  updateCharacterCharacteristics,
  wppForAgility,
  wppForWeaponMaster,
} from '~/services/character-service'

import { useCharacter } from './use-character'

export const useCharacterCharacteristic = () => {
  const { t } = useI18n()
  const { characterId } = useCharacter()

  const {
    data: characterCharacteristics,
    refresh: loadCharacterCharacteristics,
  } = useAsyncDataCustom(
    CHARACTER_QUERY_KEYS.characteristics(toValue(characterId)),
    () => getCharacterCharacteristics(toValue(characterId)),
    {
      default: createEmptyCharacteristic,
    },
  )

  function setCharacterCharacteristicsSync(characteristic: CharacterCharacteristics) {
    characterCharacteristics.value = characteristic
  }

  const {
    execute: onConvertCharacterCharacteristics,
    isLoading: convertingCharacterCharacteristics,
  } = useAsyncCallback(async (conversion: CharacteristicConversion) => {
    setCharacterCharacteristicsSync(
      await convertCharacterCharacteristics(toValue(characterId), conversion),
    )
  }, {
    pageLoading: true,
    delay: 500,
  })

  const {
    execute: onCommitCharacterCharacteristics,
  } = useAsyncCallback(async (characteristics: CharacterCharacteristics) => {
    setCharacterCharacteristicsSync(
      await updateCharacterCharacteristics(toValue(characterId), characteristics),
    )
  }, {
    successMessage: t('character.characteristic.commit.notify'),
    pageLoading: true,
  })

  const healthPoints = computed(() => computeHealthPoints(characterCharacteristics.value.skills.ironFlesh, characterCharacteristics.value.attributes.strength))

  return {
    characterCharacteristics,
    loadCharacterCharacteristics,
    healthPoints,

    onConvertCharacterCharacteristics,
    onCommitCharacterCharacteristics,
    convertingCharacterCharacteristics,
  }
}

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
      return skill <= Math.floor(characteristics.attributes.agility / 3) // TODO: move "3" to constants.json

    case 'mountedArchery':
    case 'shield':
      return skill <= Math.floor(characteristics.attributes.agility / 6) // TODO: move "6" to constants.json

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
    noLimit = false, // TODO: FIXME: need a name, for builder page
  ): { modelValue: number, min: number, max: number } => {
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

  const healthPoints = computed(() => computeHealthPoints(
    characteristics.value.skills.ironFlesh,
    characteristics.value.attributes.strength,
  ))

  return {
    canConvertAttributesToSkills,
    canConvertSkillsToAttributes,
    characteristics,
    convertAttributeToSkills,
    convertSkillsToAttribute,
    currentSkillRequirementsSatisfied,
    getInputProps,
    isChangeValid,
    // onFullFillField,
    onInput,
    // onResetField,
    reset,
    wasChangeMade,
    healthPoints,
  }
}

import { useI18n } from '#imports'
import { computed, toValue } from 'vue'

import type {
  CharacterCharacteristics,
  CharacteristicConversion,
} from '~/models/character'

import { useAsyncCallback } from '~/composables/utils/use-async-callback'
import { CHARACTER_QUERY_KEYS } from '~/queries'
import {
  computeHealthPoints,
  convertCharacterCharacteristics,
  createEmptyCharacteristic,
  getCharacterCharacteristics,
  updateCharacterCharacteristics,
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

  const [onConvertCharacterCharacteristics, convertingCharacterCharacteristics] = useAsyncCallback(
    async (conversion: CharacteristicConversion) => {
      setCharacterCharacteristicsSync(
        await convertCharacterCharacteristics(toValue(characterId), conversion),
      )
    },
    {
      pageLoading: true,
      delay: 500,
    },
  )

  const [onCommitCharacterCharacteristics] = useAsyncCallback(
    async (characteristics: CharacterCharacteristics) => {
      setCharacterCharacteristicsSync(
        await updateCharacterCharacteristics(toValue(characterId), characteristics),
      )
    },
    {
      successMessage: t('character.characteristic.commit.notify'),
      pageLoading: true,
    },
  )

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

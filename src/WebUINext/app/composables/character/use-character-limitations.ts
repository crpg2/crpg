import { getCharacterLimitations } from '~/services/character-service'

import { useCharacter } from './use-character'

export const useCharacterLimitations = () => {
  const { characterId } = useCharacter()

  const {
    state: characterLimitations,
    execute: loadCharacterLimitations,
    isLoading: loadingCharacterLimitations,
  } = useAsyncState(
    () => getCharacterLimitations(toValue(characterId)),
    { lastRespecializeAt: new Date() },
  )

  return {
    characterLimitations,
    loadCharacterLimitations,
    loadingCharacterLimitations,
  }
}

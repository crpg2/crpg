import { getCharacterLimitations } from '~/services/character-service'

import { useAsyncStateWithPoll } from '../utils/use-async-state'

export const useCharacterLimitations = (characterId: MaybeRefOrGetter<number>) => {
  const {
    state: characterLimitations,
    execute: loadCharacterLimitations,
    isLoading: loadingCharacterLimitations,
  } = useAsyncStateWithPoll(
    () => getCharacterLimitations(toValue(characterId)),
    { lastRespecializeAt: new Date() },
  )

  return {
    characterLimitations,
    loadCharacterLimitations,
    loadingCharacterLimitations,
  }
}

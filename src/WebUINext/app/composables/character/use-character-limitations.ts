import type { CharacterLimitations } from '~/models/character'

import { usePageLoading } from '~/composables/app/use-page-loading'
import { usePollInterval } from '~/composables/utils/use-poll-interval'
import { getCharacterLimitations } from '~/services/character-service'
import { pollCharacterLimitationsSymbol } from '~/symbols'

interface CharacterLimitationsContext {
  characterLimitations: Ref<CharacterLimitations>
  loadCharacterLimitations: () => Promise<CharacterLimitations>
  loadingCharacterLimitations: Ref<boolean>
}

const characterLimitationsKey: InjectionKey<CharacterLimitationsContext> = Symbol('CharacterLimitations')

export const useCharacterLimitationsProvider = (characterId: MaybeRefOrGetter<number>) => {
  const {
    state: characterLimitations,
    execute: loadCharacterLimitations,
    isLoading: loadingCharacterLimitations,
  } = useAsyncState(
    () => getCharacterLimitations(toValue(characterId)),
    { lastRespecializeAt: new Date() },
    {
      immediate: true,
    },
  )

  usePollInterval(
    {
      key: pollCharacterLimitationsSymbol,
      fn: loadCharacterLimitations,
    },
  )

  usePageLoading({
    watch: [loadingCharacterLimitations],
  })

  provide(characterLimitationsKey, {
    characterLimitations,
    loadCharacterLimitations,
    loadingCharacterLimitations,
  })
}

export const useCharacterLimitations = () => {
  const {
    characterLimitations,
    loadCharacterLimitations,
  } = injectStrict(characterLimitationsKey)

  return {
    characterLimitations,
    loadCharacterLimitations,
  }
}

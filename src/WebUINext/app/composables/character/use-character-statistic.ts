import type { CharacterStatistics } from '~/models/character'
import type { GameMode } from '~/models/game-mode'

import { usePageLoading } from '~/composables/app/use-page-loading'
import { usePollInterval } from '~/composables/utils/use-poll-interval'
import { getCharacterStatistics } from '~/services/character-service'
import { pollCharacterStatisticsSymbol } from '~/symbols'

interface CharacterStatisticsContext {
  characterStatistics: Ref<Partial<Record<GameMode, CharacterStatistics>>>
  loadCharacterStatistics: () => Promise<Partial<Record<GameMode, CharacterStatistics>>>
  loadingCharacterStatistics: Ref<boolean>
}

const characterStatisticsKey: InjectionKey<CharacterStatisticsContext> = Symbol('CharacterItems')

export const useCharacterStatisticsProvider = (characterId: MaybeRefOrGetter<number>) => {
  const {
    state: characterStatistics,
    execute: loadCharacterStatistics,
    isLoading: loadingCharacterStatistics,
  } = useAsyncState(
    () => getCharacterStatistics(toValue(characterId)),
    {},
    { immediate: true },
  )

  usePollInterval(
    {
      key: pollCharacterStatisticsSymbol,
      fn: loadCharacterStatistics,
    },
  )

  usePageLoading({
    watch: [loadingCharacterStatistics],
  })

  provide(characterStatisticsKey, {
    characterStatistics,
    loadCharacterStatistics,
    loadingCharacterStatistics,
  })

  return {
    characterStatistics,
    loadCharacterStatistics,
    loadingCharacterStatistics,
  }
}

export const useCharacterStatistic = () => {
  const {
    characterStatistics,
    loadCharacterStatistics,
    loadingCharacterStatistics,
  } = injectStrict(characterStatisticsKey)

  return {
    characterStatistics,
    loadCharacterStatistics,
    loadingCharacterStatistics,
  }
}

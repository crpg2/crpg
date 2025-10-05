import type { Character } from '~/models/character'

import { useUser } from '~/composables/user/use-user'
import { CHARACTER_QUERY_KEYS } from '~/queries'
import { getCharacters } from '~/services/character-service'

const characterKey: InjectionKey<{
  characterId: ComputedRef<number>
  character: ComputedRef<Character>
}> = Symbol('Character')

export const useCharactersProvider = () => {
  return useAsyncDataCustom(
    CHARACTER_QUERY_KEYS.root,
    getCharacters,
    {
      default: () => [],
      immediate: false, // you need to manually request data using await execute()
    },
  )
}

export const useCharacters = () => {
  const { user } = useUser()

  const characters = getAsyncData<Character[]>(CHARACTER_QUERY_KEYS.root)
  const refreshCharacters = refreshAsyncData(CHARACTER_QUERY_KEYS.root)

  const activeCharacterId = computed(() => user.value!.activeCharacterId)

  const fallbackCharacterId = computed(() => user.value!.activeCharacterId || characters.value?.[0]?.id || null)

  const validateCharacter = (id: number) => characters.value.some(c => c.id === id)

  return {
    characters,
    refreshCharacters,
    activeCharacterId,
    fallbackCharacterId,
    validateCharacter,
  }
}

export const useCharacterProvider = (character: MaybeRefOrGetter<Character>) => {
  provide(characterKey, {
    character: computed(() => toValue(character)),
    characterId: computed(() => toValue(character)?.id),
  })
}

export const useCharacter = () => {
  const { character, characterId } = injectStrict(characterKey)

  return {
    character,
    characterId,
  }
}

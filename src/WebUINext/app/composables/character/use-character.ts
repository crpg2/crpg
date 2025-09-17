import type { Character } from '~/models/character'

const characterKey: InjectionKey<{
  characterId: ComputedRef<number>
  character: ComputedRef<Character>
}> = Symbol('Character')

export const useCharacterProvider = (character: MaybeRefOrGetter<Character>) => {
  provide(characterKey, {
    character: computed(() => toValue(character)),
    characterId: computed(() => toValue(character).id),
  })
}

export const useCharacter = () => {
  const { character, characterId } = injectStrict(characterKey)

  return {
    character,
    characterId,
  }
}

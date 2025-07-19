import type { Character } from '~/models/character'

const characterKey: InjectionKey<Ref<Character>> = Symbol('Character')

export const useCharacterProvider = (character: Ref<Character>) => {
  provide(characterKey, character)
}

export const useCharacter = () => {
  const character = injectStrict(characterKey)

  return {
    character,
  }
}

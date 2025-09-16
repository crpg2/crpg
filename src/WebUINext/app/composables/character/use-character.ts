import type { Character } from '~/models/character'

const characterKey: InjectionKey<Ref<Character>> = Symbol('Character')

export const useCharacterProvider = (characterId: MaybeRefOrGetter<number>) => {
  const userStore = useUserStore()
  const character = computed(() => userStore.characters.find(c => c.id === toValue(characterId))!)
  const _characterId = computed(() => character.value.id)

  provide(characterKey, character)

  return {
    character,
    characterId: _characterId,
  }
}

export const useCharacter = () => {
  const character = injectStrict(characterKey)

  return {
    character,
  }
}

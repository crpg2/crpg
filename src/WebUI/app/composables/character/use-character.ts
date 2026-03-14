import type { CampaignUpdate } from '~/models/campaign/party'
import type { Character } from '~/models/character'
import type { User } from '~/models/user'

import { useUser } from '~/composables/user/use-user'
import { CHARACTER_QUERY_KEYS } from '~/queries'
import { getCharacters } from '~/services/character-service'

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

export const useCharacterState = (strict: boolean = true) => {
  const state = useState<Character | null>('character')

  if (strict && state.value === null) {
    throw createError({ statusMessage: 'Character not provided' })
  }

  const characterId = computed(() => state.value!.id)

  const setCharacterState = (data: Character) => {
    state.value = data
  }

  return {
    characterState: state as Ref<Character>,
    characterId,
    setCharacterState,
  }
}

export const useCharacter = () => {
  const { characterState, characterId, setCharacterState } = useCharacterState()

  return {
    character: characterState,
    characterId,
    setCharacterState,
  }
}

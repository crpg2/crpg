import { usePageLoading } from '~/composables/app/use-page-loading'
import { useAsyncCallback } from '~/composables/utils/use-async-callback'
import { setCharacterForTournament } from '~/services/character-service'
import { useUserStore } from '~/stores/user'

import { useCharacter } from './use-character'

export const useCharacterTournament = (onSuccess?: () => void) => {
  const { t } = useI18n()

  const userStore = useUserStore()
  const { characterId } = useCharacter()

  const {
    execute: onSetCharacterForTournament,
    isLoading: settingCharacterForTournament,
  } = useAsyncCallback(async () => {
    await setCharacterForTournament(characterId.value)
    await userStore.fetchCharacters()
  }, {
    onSuccess,
    successMessage: t('character.settings.tournament.notify.success'),
  })

  usePageLoading([settingCharacterForTournament])

  return {
    onSetCharacterForTournament,
    settingCharacterForTournament,
  }
}

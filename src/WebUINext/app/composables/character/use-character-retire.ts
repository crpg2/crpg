import { useAsyncCallback } from '~/composables/utils/use-async-callback'
import { retireCharacter } from '~/services/character-service'
import { useUserStore } from '~/stores/user'

import { useCharacter } from './use-character'

export const useCharacterRetire = (onSuccess?: () => void) => {
  const { t } = useI18n()

  const userStore = useUserStore()
  const { characterId } = useCharacter()

  const {
    execute: onRetireCharacter,
    isLoading: retiringCharacter,
  } = useAsyncCallback(async () => {
    await retireCharacter(characterId.value)

    await Promise.all([
      userStore.fetchUser(), // update experienceMultiplier
      userStore.fetchCharacters(),
    ])
  }, {
    successMessage: t('character.settings.retire.notify.success'),
    pageLoading: true,
    onSuccess,
  })

  return {
    onRetireCharacter,
    retiringCharacter,
  }
}

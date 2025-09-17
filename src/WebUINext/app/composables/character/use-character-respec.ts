import { useAsyncCallback } from '~/composables/utils/use-async-callback'
import { getRespecCapability, respecializeCharacter } from '~/services/character-service'
import { useUserStore } from '~/stores/user'

import { useCharacter } from './use-character'
import { useCharacterLimitations } from './use-character-limitations'

export const useCharacterRespec = (onSuccess?: () => void) => {
  const { t } = useI18n()

  const userStore = useUserStore()
  const { character, characterId } = useCharacter()
  const { characterLimitations, loadCharacterLimitations } = useCharacterLimitations(characterId)

  const respecCapability = computed(() => getRespecCapability(
    character.value,
    characterLimitations.value,
    userStore.user!.gold,
    userStore.isRecentUser,
  ))

  const {
    execute: onRespecializeCharacter,
    isLoading: respecializingCharacter,
  } = useAsyncCallback(
    async () => {
      await respecializeCharacter(characterId.value)

      await Promise.all([
        userStore.fetchUser(), // update gold
        userStore.fetchCharacters(),
        loadCharacterLimitations(),
      ])
    },
    {
      pageLoading: true,
      successMessage: t('character.settings.respecialize.notify.success'),
      onSuccess,
    },
  )

  return {
    characterLimitations,
    loadCharacterLimitations,
    respecCapability,
    onRespecializeCharacter,
    respecializingCharacter,
  }
}

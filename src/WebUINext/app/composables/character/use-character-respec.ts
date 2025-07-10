import { useAsyncCallback } from '~/composables/utils/use-async-callback'
import { getCharacterLimitations, getRespecCapability, respecializeCharacter } from '~/services/character-service'
import { useUserStore } from '~/stores/user'

import { useCharacter } from './use-character'

export const useCharacterRespec = () => {
  const toast = useToast()
  const { t } = useI18n()

  const userStore = useUserStore()
  const { character } = useCharacter()

  const {
    state: characterLimitations,
    execute: loadCharacterLimitations,
  } = useAsyncState(
    (id: number) => getCharacterLimitations(id),
    { lastRespecializeAt: new Date() },
    {
      immediate: false,
      resetOnExecute: false,
    },
  )

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
    async (characterId: number) => {
      await respecializeCharacter(characterId)

      await Promise.all([
        userStore.fetchUser(), // update gold
        userStore.fetchCharacters(), // update characters
        loadCharacterLimitations(0, characterId),
      ])

      toast.add({
        title: t('character.settings.respecialize.notify.success'),
        close: false,
        color: 'success',
      })
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

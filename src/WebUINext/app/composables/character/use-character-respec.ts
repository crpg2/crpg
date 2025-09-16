import { usePageLoading } from '~/composables/app/use-page-loading'
import { useAsyncCallback } from '~/composables/utils/use-async-callback'
import { getRespecCapability, respecializeCharacter } from '~/services/character-service'
import { useUserStore } from '~/stores/user'

import { useCharacter } from './use-character'
import { useCharacterLimitations } from './use-character-limitations'

export const useCharacterRespec = () => {
  const toast = useToast()
  const { t } = useI18n()

  const userStore = useUserStore()
  const { character } = useCharacter()
  const { characterLimitations, loadCharacterLimitations } = useCharacterLimitations()

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
      await respecializeCharacter(character.value.id)

      await Promise.all([
        userStore.fetchUser(), // update gold
        userStore.fetchCharacters(), // update characters
        loadCharacterLimitations(),
      ])

      toast.add({
        title: t('character.settings.respecialize.notify.success'),
        close: false,
        color: 'success',
      })
    },
  )

  usePageLoading({
    watch: [respecializingCharacter],
  })

  return {
    characterLimitations,
    loadCharacterLimitations,
    respecCapability,
    onRespecializeCharacter,
    respecializingCharacter,
  }
}

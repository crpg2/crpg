import { usePageLoading } from '~/composables/app/use-page-loading'
import { useAsyncCallback } from '~/composables/utils/use-async-callback'
import { setCharacterForTournament } from '~/services/character-service'
import { useUserStore } from '~/stores/user'

import { useCharacter } from './use-character'
import { useCharacterCharacteristic } from './use-character-characteristic'

export const useCharacterTournament = () => {
  const toast = useToast()
  const { t } = useI18n()

  const userStore = useUserStore()
  const { character } = useCharacter()
  const { loadCharacterCharacteristics } = useCharacterCharacteristic()

  const {
    execute: onSetCharacterForTournament,
    isLoading: settingCharacterForTournament,
  } = useAsyncCallback(async () => {
    await setCharacterForTournament(character.value.id)

    await Promise.all([
      userStore.fetchCharacters(),
      loadCharacterCharacteristics(),
    ])

    toast.add({
      title: t('character.settings.tournament.notify.success'),
      close: false,
      color: 'success',
    })
  })

  usePageLoading({
    watch: [settingCharacterForTournament],
  })

  return {
    onSetCharacterForTournament,
    settingCharacterForTournament,
  }
}

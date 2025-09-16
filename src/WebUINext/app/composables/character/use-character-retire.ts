import { usePageLoading } from '~/composables/app/use-page-loading'
import { useAsyncCallback } from '~/composables/utils/use-async-callback'
import { retireCharacter } from '~/services/character-service'
import { useUserStore } from '~/stores/user'

import { useCharacter } from './use-character'
import { useCharacterCharacteristic } from './use-character-characteristic'

export const useCharacterRetire = () => {
  const toast = useToast()
  const { t } = useI18n()

  const userStore = useUserStore()
  const { character } = useCharacter()
  const { loadCharacterCharacteristics } = useCharacterCharacteristic()

  const {
    execute: onRetireCharacter,
    isLoading: retiringCharacter,
  } = useAsyncCallback(async () => {
    await retireCharacter(character.value.id)

    await Promise.all([
      userStore.fetchUser(), // update user
      userStore.fetchCharacters(), // update char
      loadCharacterCharacteristics(),
    ])

    toast.add({
      title: t('character.settings.retire.notify.success'),
      close: false,
      color: 'success',
    })
  })

  usePageLoading({
    watch: [retiringCharacter],
  })

  return {
    onRetireCharacter,
    retiringCharacter,
  }
}

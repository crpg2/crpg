import { useStorage } from '@vueuse/core'
import { LazyAppWelcomeModal } from '#components'

import { useUser } from './user/use-user'

export const useWelcome = () => {
  const showedWelcomeMessage = useStorage<boolean>('user-welcome-message-showed', false)

  const { user } = useUser()

  const isNewUser = computed(() => user.value?.isRecent)

  const overlay = useOverlay()

  const modal = overlay.create(LazyAppWelcomeModal, {
    props: {
      onClose: () => {
        showedWelcomeMessage.value = true
      },
    },
  })

  if (isNewUser.value && !showedWelcomeMessage.value) {
    modal.open()
  }

  return {
    showWelcomeMessage: modal.open,
  }
}

import { useStorage } from '@vueuse/core'
import { LazyAppWelcomeModal } from '#components'
import { useOverlay } from '#imports'

import { useUser } from '~/composables/user/use-user'

export const useWelcome = () => {
  const showedWelcomeMessage = useStorage<boolean>('user-welcome-message-showed', false)

  const { user } = useUser()

  const overlay = useOverlay()

  const modal = overlay.create(LazyAppWelcomeModal, {
    props: {
      onClose: () => {
        showedWelcomeMessage.value = true
      },
    },
  })

  if (Boolean(user.value?.isRecent) && !showedWelcomeMessage.value) {
    modal.open()
  }

  return {
    showWelcomeMessage: modal.open,
  }
}

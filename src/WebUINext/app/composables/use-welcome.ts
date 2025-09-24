import { useStorage } from '@vueuse/core'
import { LazyAppWelcomeModal } from '#components'

import { useUserStore } from '~/stores/user'

export const useWelcome = () => {
  const showedWelcomeMessage = useStorage<boolean>('user-welcome-message-showed', false)

  const userStore = useUserStore()

  const isNewUser = computed(() => userStore.user !== null,
  // TODO:
  // && userStore.isRecentUser
  )

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

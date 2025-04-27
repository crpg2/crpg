import { useStorage } from '@vueuse/core'
import { AppWelcome } from '#components'

import { useUserStore } from '~/stores/user'

const showedWelcomeMessage = useStorage<boolean>('user-welcome-message-showed', false)

// TODO: FIXME: notification center https://github.com/namidaka/crpg/issues/186
export const useWelcome = () => {
  // TOOD: FIXME:
  const userStore = useUserStore()

  const shownWelcomeMessage = computed(() => userStore.user !== null && userStore.isRecentUser)

  const onCloseWelcomeMessage = () => {
    showedWelcomeMessage.value = true
  }

  const showWelcomeMessage = () => {
    showedWelcomeMessage.value = false
  }

  const overlay = useOverlay()

  const modal = overlay.create(AppWelcome, {
    props: {
      onClose: () => onCloseWelcomeMessage(),
    },
  })

  // TODO: FIXME:
  // modal.open()

  // watch(shownWelcomeMessage, () => {
  //   shownWelcomeMessage.value ? modal.open() : modal.close()
  // }, { immediate: true })

  return {
    showWelcomeMessage,
  }
}

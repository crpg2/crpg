import { computedWithControl, useLocalStorage } from '@vueuse/core'

import { REGION } from '~/models/region'
import { getHHEventByRegion, getHHEventRemaining } from '~/services/hh-service'
import { useUserStore } from '~/stores/user'

export const useHappyHours = () => {
  const { $config } = useNuxtApp()
  const userStore = useUserStore()
  const { t } = useI18n()
  const toast = useToast()

  const source = ref()

  const hHEvent = computedWithControl(
    () => source.value,
    () => getHHEventByRegion($config.public.HH, userStore.user?.region || REGION.Eu),
  )

  const HHEventRemaining = computed(() => getHHEventRemaining(hHEvent.value))

  const isHHCountdownEnded = ref<boolean>(false)
  const alreadyShownHHStartedNotification = useLocalStorage<boolean>('hh-start-notification-shown', false)

  const onStartHHCountdown = () => {
    if (!alreadyShownHHStartedNotification.value) {
      toast.add({
        title: t('hh.notify.started'),
        close: false,
        color: 'success',
      })
      alreadyShownHHStartedNotification.value = true
    }
  }

  const onEndHHCountdown = () => {
    isHHCountdownEnded.value = true
    alreadyShownHHStartedNotification.value = false
    toast.add({
      title: t('hh.notify.ended'),
      close: false,
      color: 'success',
    })
  }

  return {
    hHEvent,
    HHEventRemaining,
    isHHCountdownEnded,
    onEndHHCountdown,
    onStartHHCountdown,
  }
}

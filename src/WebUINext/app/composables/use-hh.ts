import { computedWithControl, useLocalStorage } from '@vueuse/core'

import { REGION } from '~/models/region'
import { getHHEventByRegion, getHHEventRemaining } from '~/services/hh-service'
import { useUserStore } from '~/stores/user'

// TODO: FIXME:
export const useHappyHours = () => {
  const HHPollId = Symbol('hh')

  const userStore = useUserStore()
  // const { t } = useI18n()
  const { $config } = useNuxtApp()

  const source = ref()
  const HHEvent = computedWithControl(
    () => source.value,
    () => getHHEventByRegion($config.public.HH, userStore.user?.region || REGION.Eu),
  )

  const HHEventRemaining = computed(() => getHHEventRemaining(HHEvent.value))

  const isHHCountdownEnded = ref<boolean>(false)
  const alreadyShownHHStartedNotification = useLocalStorage('hh-start-notification-shown', false)

  const onStartHHCountdown = () => {
    if (!alreadyShownHHStartedNotification.value) {
      // $notify(t('hh.notify.started'))
      alreadyShownHHStartedNotification.value = true
    }
  }

  const onEndHHCountdown = () => {
    isHHCountdownEnded.value = true
    alreadyShownHHStartedNotification.value = false
    // $notify(t('hh.notify.ended'))
  }

  return {
    HHEvent,
    HHEventRemaining,
    HHPollId,
    isHHCountdownEnded,
    onEndHHCountdown,
    onStartHHCountdown,
  }
}

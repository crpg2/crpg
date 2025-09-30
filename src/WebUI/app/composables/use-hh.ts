import { computedWithControl, useIntervalFn, useLocalStorage } from '@vueuse/core'

import { REGION } from '~/models/region'
import { getHHEventByRegion, getHHEventRemainingSeconds } from '~/services/hh-service'
import { useUserStore } from '~/stores/user'

export const useHappyHours = () => {
  const { settings } = useAppConfig()

  const userStore = useUserStore()
  const { t } = useI18n()
  const toast = useToast()

  const source = ref()

  const hHEvent = computedWithControl(
    () => source.value,
    () => getHHEventByRegion(settings.happyHours, userStore.user?.region || REGION.Eu),
  )

  const isHhEventActive = computed(() => getHHEventRemainingSeconds(hHEvent.value) !== 0)

  const showedHHStartedNotification = useLocalStorage<boolean>('hh-start-notification-shown', false)

  const onStartHH = () => {
    if (showedHHStartedNotification.value) {
      return
    }
    toast.add({
      title: t('hh.notify.started'),
      close: false,
      color: 'success',
      duration: 7000,
    })
    showedHHStartedNotification.value = true
  }

  const onEndHH = () => {
    toast.add({
      title: t('hh.notify.ended'),
      close: false,
      color: 'success',
      duration: 7000,
    })
  }

  const { pause } = useIntervalFn(() => {
    hHEvent.trigger()

    if (isHhEventActive.value) {
      onStartHH()
    }
  }, 5000)

  tryOnScopeDispose(pause)

  return {
    hHEvent,
    isHhEventActive,
    onEndHH,
  }
}

import { useAsyncState } from '@vueuse/core'

import { NotificationState } from '~/models/notificatios'
import {
  deleteAllUserNotifications,
  deleteUserNotification,
  getUserNotifications,
  readAllUserNotifications,
  readUserNotification,
} from '~/services/users-service'
import { useUserStore } from '~/stores/user'
import { useAsyncCallback } from '~/utils/useAsyncCallback'

export const useUsersNotifications = () => {
  const userStore = useUserStore()

  const {
    state: notifications,
    execute: loadNotifications,
    isLoading: loadingNotifications,
  } = useAsyncState(
    () => getUserNotifications(),
    {
      notifications: [],
      dict: {
        users: [],
        clans: [],
        characters: [],
      },
    },
    {
      resetOnExecute: false,
    },
  )

  const hasUnreadNotifications = computed(() =>
    notifications.value.notifications.some(n => n.state === NotificationState.Unread),
  )

  const { execute: readNotification, loading: readingNotification } = useAsyncCallback(
    async (id: number) => {
      await readUserNotification(id)

      await Promise.all([loadNotifications(), userStore.fetchUser()])
    },
  )

  const { execute: readAllNotifications, loading: readingAllNotification } = useAsyncCallback(
    async () => {
      await readAllUserNotifications()
      await Promise.all([loadNotifications(), userStore.fetchUser()])
    },
  )

  const { execute: deleteNotification, loading: deletingNotification } = useAsyncCallback(
    async (id: number) => {
      await deleteUserNotification(id)
      await Promise.all([loadNotifications(), userStore.fetchUser()])
    },
  )

  const { execute: deleteAllNotifications, loading: deletingAllNotification } = useAsyncCallback(
    async () => {
      await deleteAllUserNotifications()
      await Promise.all([loadNotifications(), userStore.fetchUser()])
    },
  )

  const isLoading = computed(
    () =>
      loadingNotifications.value
      || readingNotification.value
      || deletingNotification.value
      || readingAllNotification.value
      || deletingAllNotification.value,
  )

  const isEmpty = computed(() => !notifications.value.notifications.length)

  return {
    notifications,
    isEmpty,
    isLoading,
    loadNotifications,
    hasUnreadNotifications,
    readNotification,
    readAllNotifications,
    deleteNotification,
    deleteAllNotifications,
  }
}

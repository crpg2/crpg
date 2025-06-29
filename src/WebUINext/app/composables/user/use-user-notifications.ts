import { useAsyncState } from '@vueuse/core'

import { useAsyncCallback } from '~/composables/utils/use-async-callback'
import { NotificationState } from '~/models/notifications'
import {
  deleteAllUserNotifications,
  deleteUserNotification,
  getUserNotifications,
  readAllUserNotifications,
  readUserNotification,
} from '~/services/user-service'
import { useUserStore } from '~/stores/user'

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

  function updateState() {
    return Promise.all([loadNotifications(), userStore.fetchUser()])
  }

  const { execute: readNotification, isLoading: readingNotification } = useAsyncCallback(
    async (id: number) => {
      await readUserNotification(id)
      updateState()
    },
  )

  const { execute: readAllNotifications, isLoading: readingAllNotification } = useAsyncCallback(
    async () => {
      await readAllUserNotifications()
      updateState()
    },
  )

  const { execute: deleteNotification, isLoading: deletingNotification } = useAsyncCallback(
    async (id: number) => {
      await deleteUserNotification(id)
      updateState()
    },
  )

  const { execute: deleteAllNotifications, isLoading: deletingAllNotification } = useAsyncCallback(
    async () => {
      await deleteAllUserNotifications()
      updateState()
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

import { useAsyncState } from '@vueuse/core'
import { computed } from 'vue'

import { useUser } from '~/composables/user/use-user'
import { useAsyncCallback } from '~/composables/utils/use-async-callback'
import { NOTIFICATION_STATE } from '~/models/notifications'
import {
  deleteAllUserNotifications,
  deleteUserNotification,
  getUserNotifications,
  readAllUserNotifications,
  readUserNotification,
} from '~/services/user-service'

export const useUsersNotifications = () => {
  const { fetchUser } = useUser()

  const {
    state: notifications,
    executeImmediate: loadNotifications,
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
    notifications.value.notifications.some(n => n.state === NOTIFICATION_STATE.Unread),
  )

  function updateState() {
    return Promise.all([loadNotifications(), fetchUser()])
  }

  const [readNotification, readingNotification] = useAsyncCallback(
    async (id: number) => {
      await readUserNotification(id)
      updateState()
    },
  )

  const [readAllNotifications, readingAllNotification] = useAsyncCallback(
    async () => {
      await readAllUserNotifications()
      updateState()
    },
  )

  const [deleteNotification, deletingNotification] = useAsyncCallback(
    async (id: number) => {
      await deleteUserNotification(id)
      updateState()
    },
  )

  const [deleteAllNotifications, deletingAllNotification] = useAsyncCallback(
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

  return {
    notifications: toRef(() => notifications.value.notifications),
    notificationsDict: toRef(() => notifications.value.dict),
    isLoading,
    loadNotifications,
    hasUnreadNotifications,
    readNotification,
    readAllNotifications,
    deleteNotification,
    deleteAllNotifications,
  }
}

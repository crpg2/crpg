import { flushPromises } from '@vue/test-utils'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { NOTIFICATION_STATE } from '~/models/notifications'

import { useUsersNotifications } from '../use-user-notifications'

const {
  getUserNotificationsMock,
  readUserNotificationMock,
  readAllUserNotificationsMock,
  deleteUserNotificationMock,
  deleteAllUserNotificationsMock,
  useUserMock,
} = vi.hoisted(() => ({
  getUserNotificationsMock: vi.fn(),
  readUserNotificationMock: vi.fn(),
  readAllUserNotificationsMock: vi.fn(),
  deleteUserNotificationMock: vi.fn(),
  deleteAllUserNotificationsMock: vi.fn(),
  useUserMock: vi.fn(),
}))

vi.mock('~/services/user-service', () => ({
  getUserNotifications: getUserNotificationsMock,
  readUserNotification: readUserNotificationMock,
  readAllUserNotifications: readAllUserNotificationsMock,
  deleteUserNotification: deleteUserNotificationMock,
  deleteAllUserNotifications: deleteAllUserNotificationsMock,
}))

vi.mock('~/composables/user/use-user', () => ({
  useUser: useUserMock,
}))

vi.mock('~/composables/utils/use-async-callback', () => ({
  useAsyncCallback: vi.fn((callback: any) => [vi.fn((...args: any[]) => callback(...args))]),
}))

describe('useUsersNotifications', () => {
  beforeEach(() => {
    useUserMock.mockReturnValue({ fetchUser: vi.fn() })

    getUserNotificationsMock.mockResolvedValue({
      notifications: [],
      dict: { users: [], clans: [], characters: [] },
    })
    readUserNotificationMock.mockResolvedValue(undefined)
    readAllUserNotificationsMock.mockResolvedValue(undefined)
    deleteUserNotificationMock.mockResolvedValue(undefined)
    deleteAllUserNotificationsMock.mockResolvedValue(undefined)
  })

  it('initializes with empty notifications and computed values', () => {
    const composable = useUsersNotifications()

    expect(composable.notifications.value.notifications).toEqual([])
    expect(composable.isEmpty.value).toBe(true)
    expect(composable.hasUnreadNotifications.value).toBe(false)
  })

  it('loadNotifications fetches data from service', async () => {
    const fakeResponse = {
      notifications: [{ id: 1, state: NOTIFICATION_STATE.Read }],
      dict: { users: [], clans: [], characters: [] },
    }
    getUserNotificationsMock.mockResolvedValue(fakeResponse)

    const { notifications } = useUsersNotifications() // immediate load
    await flushPromises()

    expect(getUserNotificationsMock).toHaveBeenCalledTimes(1)
    expect(notifications.value.notifications).toEqual(fakeResponse.notifications)
  })

  it('hasUnreadNotifications updates correctly when data changes', async () => {
    getUserNotificationsMock.mockResolvedValue({
      notifications: [
        { id: 1, state: NOTIFICATION_STATE.Read },
        { id: 2, state: NOTIFICATION_STATE.Unread },
      ],
      dict: { users: [], clans: [], characters: [] },
    })

    const { hasUnreadNotifications } = useUsersNotifications()
    await flushPromises()

    expect(hasUnreadNotifications.value).toBe(true)
  })

  it('readNotification calls service and refreshes user + notifications', async () => {
    const fetchUser = vi.fn()
    useUserMock.mockReturnValue({ fetchUser })

    const { readNotification, notifications } = useUsersNotifications()
    await flushPromises()

    await readNotification(123)

    expect(readUserNotificationMock).toHaveBeenCalledWith(123)
    expect(getUserNotificationsMock).toHaveBeenCalled()
    expect(fetchUser).toHaveBeenCalled()
    expect(notifications.value).toBeDefined()
  })

  it('deleteAllNotifications calls service and refreshes state', async () => {
    const fetchUser = vi.fn()
    useUserMock.mockReturnValue({ fetchUser })

    const { deleteAllNotifications } = useUsersNotifications()
    await flushPromises()
    await deleteAllNotifications()

    expect(deleteAllUserNotificationsMock).toHaveBeenCalledTimes(1)
    expect(getUserNotificationsMock).toHaveBeenCalled()
    expect(fetchUser).toHaveBeenCalled()
  })
})

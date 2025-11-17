import { beforeEach, describe, expect, it, vi } from 'vitest'
import { ref } from 'vue'

import { useUser } from '../use-user'

const { getUserMock, useStateMock } = vi.hoisted(() => ({
  getUserMock: vi.fn(),
  useStateMock: vi.fn(),
}))

vi.mock('#imports', () => ({
  useState: useStateMock,
}))

vi.mock('~/services/user-service', () => ({
  getUser: getUserMock,
}))

describe('useUser', () => {
  const userRef = ref(null)

  beforeEach(() => {
    useStateMock.mockReturnValue(userRef)
  })

  it('initializes with user = null', () => {
    const { user } = useUser()
    expect(user.value).toBeNull()
  })

  it('fetchUser calls getUser and updates user', async () => {
    const fakeUser = { id: 1, name: 'John' }
    getUserMock.mockResolvedValue(fakeUser)

    const { user, fetchUser } = useUser()
    await fetchUser()

    expect(getUserMock).toHaveBeenCalledTimes(1)
    expect(user.value).toEqual(fakeUser)
  })

  it('computes clan and clanMemberRole correctly', async () => {
    const fakeUser = {
      clanMembership: {
        clan: { id: 10, name: 'Wolves' },
        role: 'Leader',
      },
    }
    getUserMock.mockResolvedValue(fakeUser)

    const { fetchUser, clan, clanMemberRole } = useUser()
    await fetchUser()

    expect(clan.value).toEqual({ id: 10, name: 'Wolves' })
    expect(clanMemberRole.value).toBe('Leader')
  })

  it('computes hasUnreadNotifications correctly', async () => {
    const fakeUser = { unreadNotificationsCount: 3 }
    getUserMock.mockResolvedValue(fakeUser)

    const { fetchUser, hasUnreadNotifications } = useUser()
    await fetchUser()

    expect(hasUnreadNotifications.value).toBe(true)
  })
})

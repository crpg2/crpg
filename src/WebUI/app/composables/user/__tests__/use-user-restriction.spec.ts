import { beforeEach, describe, expect, it, vi } from 'vitest'
import { ref } from 'vue'

import { useUserRestriction } from '../use-user-restriction'

const { useStateMock, getUserRestrictionMock } = vi.hoisted(() => ({
  getUserRestrictionMock: vi.fn(),
  useStateMock: vi.fn(),
}))

vi.mock('#imports', () => ({
  useState: useStateMock,
}))

vi.mock('~/services/user-service', () => ({
  getUserRestriction: getUserRestrictionMock,
}))

describe('useUserRestriction', () => {
  const restrictionRef = ref(null)

  beforeEach(() => {
    useStateMock.mockReturnValue(restrictionRef)
  })

  it('initializes with restriction = null', () => {
    const { restriction } = useUserRestriction()
    expect(restriction.value).toBeNull()
  })

  it('fetchUserRestriction calls getUserRestriction and updates restriction', async () => {
    const fakeRestriction = { type: 'ban', reason: 'spam', expiresAt: '2025-12-01' }
    getUserRestrictionMock.mockResolvedValue(fakeRestriction)

    const { restriction, fetchUserRestriction } = useUserRestriction()
    await fetchUserRestriction()

    expect(getUserRestrictionMock).toHaveBeenCalledTimes(1)
    expect(restriction.value).toEqual(fakeRestriction)
  })

  it('handles null return from getUserRestriction', async () => {
    getUserRestrictionMock.mockResolvedValue(null)

    const { restriction, fetchUserRestriction } = useUserRestriction()
    await fetchUserRestriction()

    expect(restriction.value).toBeNull()
  })
})

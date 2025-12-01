import { flushPromises } from '@vue/test-utils'
import { describe, expect, it, vi } from 'vitest'

import { useClanApplications } from '../use-clan-applications'

const {
  CLAN_INVITATIONS,
  mockedUseClan,
  mockedGetClanInvitations,
  mockedRespondToClanInvitation,
  mockedInviteToClan,
} = vi.hoisted(() => {
  const CLAN_INVITATIONS = [{ id: 2 }, { id: 3 }]
  return {
    CLAN_INVITATIONS,
    mockedUseClan: vi.fn().mockReturnValue({ clan: { value: { id: 1 } } }),
    mockedGetClanInvitations: vi.fn().mockResolvedValue(CLAN_INVITATIONS),
    mockedRespondToClanInvitation: vi.fn(),
    mockedInviteToClan: vi.fn(),
  }
})

vi.mock('~/services/clan-service', () => ({
  getClanInvitations: mockedGetClanInvitations,
  respondToClanInvitation: mockedRespondToClanInvitation,
  inviteToClan: mockedInviteToClan,
}))

vi.mock('~/composables/clan/use-clan', () => ({
  useClan: mockedUseClan,
}))

describe('useClanApplications', () => {
  it('initial state', async () => {
    const { applications, applicationsCount } = useClanApplications()

    expect(applications.value).toEqual([])
    expect(applicationsCount.value).toBe(0)

    await flushPromises()

    expect(applications.value).toEqual(CLAN_INVITATIONS)
    expect(applicationsCount.value).toBe(2)
    expect(mockedGetClanInvitations).toHaveBeenCalledWith(1)
  })

  it('responds to clan invitation', async () => {
    const { respondToClanInvitation } = useClanApplications(false)

    await respondToClanInvitation(2, true)

    expect(mockedRespondToClanInvitation).toHaveBeenCalledWith(1, 2, true)
  })

  it('invites a user to clan', async () => {
    const { inviteToClan } = useClanApplications(false)

    await inviteToClan(5)

    expect(mockedInviteToClan).toHaveBeenCalledWith(1, 5)
  })
})

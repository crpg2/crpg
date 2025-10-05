import { flushPromises } from '@vue/test-utils'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { useClanMembers } from '../use-clan-members'

const {
  mockedUseClan,
  mockedGetClanMembers,
  mockedKickClanMember,
  mockedUpdateClanMember,
} = vi.hoisted(() => ({
  mockedUseClan: vi.fn().mockReturnValue({ clan: { value: { id: 1 } } }),
  mockedGetClanMembers: vi.fn().mockResolvedValue([
    { user: { id: 1 } },
    { user: { id: 11 } },
  ]),
  mockedKickClanMember: vi.fn(),
  mockedUpdateClanMember: vi.fn(),
}))

vi.mock('~/services/clan-service', () => ({
  getClanMembers: mockedGetClanMembers,
  kickClanMember: mockedKickClanMember,
  updateClanMember: mockedUpdateClanMember,
}))

vi.mock('~/composables/clan/use-clan', () => ({
  useClan: mockedUseClan,
}))

describe('useClanMembers', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('initial state and loads members', async () => {
    const { clanMembers, clanMembersCount, isLastMember } = useClanMembers()

    expect(clanMembers.value).toEqual([])
    expect(clanMembersCount.value).toBe(0)
    expect(isLastMember.value).toBe(true)

    await flushPromises()

    expect(clanMembers.value).toEqual([{ user: { id: 1 } }, { user: { id: 11 } }])
    expect(clanMembersCount.value).toBe(2)
    expect(isLastMember.value).toBe(false)
    expect(mockedGetClanMembers).toHaveBeenCalledWith(1)
  })

  it('getClanMember returns correct member or null', async () => {
    const { getClanMember } = useClanMembers()

    await flushPromises()

    expect(getClanMember(1)).toEqual({ user: { id: 1 } })
    expect(getClanMember(11)).toEqual({ user: { id: 11 } })
    expect(getClanMember(99)).toBeNull()
  })

  it('updateClanMember calls service with correct args', () => {
    const { updateClanMember } = useClanMembers()

    updateClanMember(11, 'Leader')
    expect(mockedUpdateClanMember).toHaveBeenCalledWith(1, 11, 'Leader')
  })

  it('kickClanMember calls service with correct args', () => {
    const { kickClanMember } = useClanMembers()

    kickClanMember(1)
    expect(mockedKickClanMember).toHaveBeenCalledWith(1, 1)
  })

  it('isLastMember works correctly with one member', async () => {
    mockedGetClanMembers.mockResolvedValueOnce([{ user: { id: 1 } }])

    const { isLastMember } = useClanMembers()

    await flushPromises()

    expect(isLastMember.value).toBe(true)
  })
})

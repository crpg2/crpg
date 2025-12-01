import { describe, expect, it, vi } from 'vitest'

import type { Clan, ClanUpdate } from '~/models/clan'

import { useClan } from '../use-clan'

const {
  mockedGetAsyncData,
  mockedRefreshAsyncData,
  mockedUpdateClanService,
  mockedUseRoute,
  mockClanData,
} = vi.hoisted(() => {
  const mockRoute = { params: { id: '42' } }
  const mockClanData = { value: ({ id: 42, name: 'Test Clan' } as Clan) }
  return {
    mockedGetAsyncData: vi.fn().mockReturnValue(mockClanData),
    mockedRefreshAsyncData: vi.fn().mockReturnValue(vi.fn()),
    mockedUpdateClanService: vi.fn(),
    mockedUseRoute: vi.fn().mockReturnValue(mockRoute),
    mockClanData,
  }
})

vi.mock('#imports', () => ({
  getAsyncData: mockedGetAsyncData,
  refreshAsyncData: mockedRefreshAsyncData,
  useRoute: mockedUseRoute,
}))

vi.mock('~/services/clan-service', () => ({
  updateClan: mockedUpdateClanService,
}))

vi.mock('~/queries', () => ({
  CLAN_QUERY_KEYS: {
    byId: (id: number) => `clan-${id}`,
  },
}))

describe('useClan', () => {
  it('returns clan and refresh function', async () => {
    const { clan, refreshClan } = useClan()

    expect(clan.value).toBe(mockClanData.value)

    refreshClan()

    expect(mockedGetAsyncData).toHaveBeenCalledWith('clan-42')
    expect(mockedRefreshAsyncData).toHaveBeenCalledWith('clan-42')
  })

  it('updateClan calls service with correct id and data', () => {
    const { updateClan } = useClan()
    const updateData = { name: 'New Name' } as ClanUpdate

    updateClan(updateData)

    expect(mockedUpdateClanService).toHaveBeenCalledWith(mockClanData.value.id, updateData)
  })
})

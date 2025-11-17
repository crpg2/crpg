import { describe, expect, it, vi } from 'vitest'

import { useUserItems, useUserItemsProvider } from '../use-user-items'

const {
  getAsyncDataMock,
  refreshAsyncDataMock,
  useAsyncDataCustomMock,
} = vi.hoisted(() => ({
  getAsyncDataMock: vi.fn(),
  refreshAsyncDataMock: vi.fn(),
  useAsyncDataCustomMock: vi.fn(),
}))

vi.mock('~/composables/utils/use-async-data-custom', () => ({
  getAsyncData: getAsyncDataMock,
  refreshAsyncData: refreshAsyncDataMock,
  useAsyncDataCustom: useAsyncDataCustomMock,
}))

const getUserItemsMock = vi.hoisted(() => vi.fn())
vi.mock('~/services/user-service', () => ({
  getUserItems: getUserItemsMock,
}))

describe('user items composables', () => {
  describe('useUserItemsProvider', () => {
    it('calls useAsyncDataCustom with expected parameters', async () => {
      const returnValue = { data: 'mocked' }
      useAsyncDataCustomMock.mockReturnValue(returnValue)

      const result = useUserItemsProvider()

      expect(useAsyncDataCustomMock).toHaveBeenCalledWith(
        expect.any(Function),
        expect.any(Function),
        { default: expect.any(Function) },
      )

      expect(result).toBe(returnValue)

      const [_, fetchFn, options] = useAsyncDataCustomMock.mock.calls[0]!

      getUserItemsMock.mockResolvedValue([{ id: 1 }])
      await fetchFn()
      expect(getUserItemsMock).toHaveBeenCalledTimes(1)
      expect(options.default()).toEqual([])
    })
  })

  describe('useUserItems', () => {
    it('returns reactive userItems and refreshUserItems', () => {
      const fakeData = { data: [{ id: 10 }] }
      const fakeRefresh = vi.fn()

      getAsyncDataMock.mockReturnValueOnce(fakeData)
      refreshAsyncDataMock.mockReturnValueOnce(fakeRefresh)

      const result = useUserItems()

      expect(result.userItems).toBe(fakeData)
      expect(result.refreshUserItems).toBe(fakeRefresh)
    })
  })
})

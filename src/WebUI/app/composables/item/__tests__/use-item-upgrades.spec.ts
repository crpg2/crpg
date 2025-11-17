import type { PartialDeep } from 'type-fest'

import { flushPromises } from '@vue/test-utils'
import { describe, expect, it, vi } from 'vitest'

import { useItemUpgrades } from '../use-item-upgrades'

const {
  mockedGetItemUpgrades,
  mockedGetRelativeEntries,
  mockedCreateItemIndex,
  mockedUseUser,
  mockedItemUpgrades,
} = vi.hoisted(() => {
  const mockedItemUpgrades = [
    {
      id: 't0',
      rank: 0,
    },
    {
      id: 't1',
      rank: 1,
    },
    {
      id: 't2',
      rank: 2,
    },
    {
      id: 't3',
      rank: 3,
    },
  ]
  return {
    mockedGetItemUpgrades: vi.fn().mockResolvedValue(mockedItemUpgrades),
    mockedGetRelativeEntries: vi.fn().mockReturnValue({}),
    mockedCreateItemIndex: vi.fn(val => val),
    mockedUseUser: vi.fn<() => PartialDeep<ReturnType<typeof import('~/composables/user/use-user')['useUser']>>>()
      .mockReturnValue({ user: { value: { heirloomPoints: 0 } } }),
    mockedItemUpgrades,
  }
})

vi.mock('~/services/item-service', () => ({
  getItemUpgrades: mockedGetItemUpgrades,
  getRelativeEntries: mockedGetRelativeEntries,
}))

vi.mock('~/services/item-search-service/indexator', () => ({
  createItemIndex: mockedCreateItemIndex,
}))

vi.mock('~/composables/user/use-user', () => ({
  useUser: mockedUseUser,
}))

describe('useItemUpgrades', () => {
  const item = { id: 't0', baseId: '1', rank: 0 }

  it('itemUpgrades', async () => {
    const { itemUpgrades } = useItemUpgrades({ item, aggregationConfig: {} })

    await flushPromises()

    expect(itemUpgrades.value).toEqual(mockedItemUpgrades)
  })

  it('baseItem', async () => {
    const { baseItem } = useItemUpgrades({ item, aggregationConfig: {} })

    await flushPromises()

    expect(baseItem.value).toEqual(mockedItemUpgrades[0])
  })

  it('nextItem', async () => {
    const { nextItem } = useItemUpgrades({ item, aggregationConfig: {} })

    await flushPromises()

    expect(nextItem.value).toEqual(mockedItemUpgrades[1])
  })

  it('relativeEntries', async () => {
    const { relativeEntries } = useItemUpgrades({ item, aggregationConfig: {} })

    await flushPromises()

    expect(relativeEntries.value).toEqual({})
  })

  describe('validation', () => {
    it('basic', async () => {
      mockedUseUser.mockReturnValueOnce({ user: { value: { heirloomPoints: 1 } } })

      const { canUpgrade, validation } = useItemUpgrades({ item, aggregationConfig: {} })

      await flushPromises()

      expect(validation.value.points).toEqual(true)
      expect(validation.value.maxRank).toEqual(true)
      expect(canUpgrade.value).toEqual(true)
    })

    it('points', async () => {
      const { canUpgrade, validation } = useItemUpgrades({ item, aggregationConfig: {} })

      await flushPromises()

      expect(validation.value.points).toEqual(false)
      expect(validation.value.maxRank).toEqual(true)
      expect(canUpgrade.value).toEqual(false)
    })

    it('maxRank', async () => {
      const item = {
        baseId: '1',
        id: 't3',
        rank: 3,
      }

      mockedUseUser.mockReturnValueOnce({ user: { value: { heirloomPoints: 1 } } })

      const { canUpgrade, validation } = useItemUpgrades({ item, aggregationConfig: {} })

      await flushPromises()

      expect(validation.value.maxRank).toEqual(false)
      expect(canUpgrade.value).toEqual(false)
    })
  })
})

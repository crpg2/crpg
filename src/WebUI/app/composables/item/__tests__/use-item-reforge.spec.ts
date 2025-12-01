import type { PartialDeep } from 'type-fest'

import { describe, expect, it, vi } from 'vitest'

import { useItemReforge } from '../use-item-reforge'

const {
  mockedReforgeCostByRank,
  mockedGetReforgeCostByRank,
  mockedUseUser,
} = vi.hoisted(() => {
  const mockedReforgeCostByRank = {
    0: 0,
    1: 40000,
    2: 90000,
    3: 150000,
  } as const

  return {
    mockedReforgeCostByRank,
    // @ts-expect-error ///
    mockedGetReforgeCostByRank: vi.fn(rank => mockedReforgeCostByRank[rank] as number),
    mockedUseUser: vi.fn<() => PartialDeep<ReturnType<typeof import('~/composables/user/use-user')['useUser']>>>()
      .mockReturnValue({ user: { value: { gold: 500000 } } }),
  }
})

vi.mock('~/services/item-service', () => ({
  reforgeCostByRank: mockedReforgeCostByRank,
  getReforgeCostByRank: mockedGetReforgeCostByRank,
}))

vi.mock('~/composables/user/use-user', () => ({
  useUser: mockedUseUser,
}))

const ITEM_RANK = 2

describe('useItemReforge', () => {
  it('reforgeCost', () => {
    const { reforgeCost } = useItemReforge(ITEM_RANK)

    expect(reforgeCost.value).toEqual(mockedReforgeCostByRank[ITEM_RANK])
  })

  it('reforgeCostTable - table without 0 rank item', () => {
    const { reforgeCostTable } = useItemReforge(ITEM_RANK)

    expect(reforgeCostTable.value.length).toEqual(3)
  })

  describe('validation', () => {
    it('not much gold', () => {
      mockedUseUser.mockReturnValueOnce({ user: { value: { gold: 1000 } } })

      const { validation, canReforge } = useItemReforge(ITEM_RANK)

      expect(validation.value.rank).toBeTruthy()
      expect(validation.value.gold).toBeFalsy()
      expect(canReforge.value).toBeFalsy()
    })

    it('lots of gold', () => {
      const { validation, canReforge } = useItemReforge(ITEM_RANK)

      expect(validation.value.gold).toBeTruthy()
      expect(canReforge.value).toBeTruthy()
    })

    it('you cannot reforge an base item', () => {
      const { validation, canReforge } = useItemReforge(0)

      expect(validation.value.rank).toBeFalsy()
      expect(canReforge.value).toBeFalsy()
    })
  })
})

import { flushPromises } from '@vue/test-utils'
import { describe, expect, it, vi } from 'vitest'

import { useClanArmory } from '../use-clan-armory'

const {
  clanId,
  mockedUseClan,
  mockedAddItemToClanArmory,
  mockedBorrowItemFromClanArmory,
  mockedGetClanArmory,
  mockedRemoveItemFromClanArmory,
  mockedReturnItemToClanArmory,
} = vi.hoisted(() => {
  const clanId = 1
  return {
    clanId,
    mockedUseClan: vi.fn().mockReturnValue({ clan: { value: { id: clanId } } }),
    mockedAddItemToClanArmory: vi.fn(),
    mockedBorrowItemFromClanArmory: vi.fn(),
    mockedGetClanArmory: vi.fn().mockResolvedValue([{ id: 1 }]),
    mockedRemoveItemFromClanArmory: vi.fn(),
    mockedReturnItemToClanArmory: vi.fn(),
  }
})

vi.mock('~/services/clan-service', () => ({
  addItemToClanArmory: mockedAddItemToClanArmory,
  borrowItemFromClanArmory: mockedBorrowItemFromClanArmory,
  getClanArmory: mockedGetClanArmory,
  removeItemFromClanArmory: mockedRemoveItemFromClanArmory,
  returnItemToClanArmory: mockedReturnItemToClanArmory,
}))

vi.mock('~/composables/clan/use-clan', () => ({
  useClan: mockedUseClan,
}))

describe('useClanArmory', () => {
  it('clanArmory', async () => {
    const { clanArmory } = useClanArmory()

    expect(clanArmory.value).toEqual([])

    await flushPromises()

    expect(mockedGetClanArmory).toBeCalled()
    expect(clanArmory.value).toEqual([{ id: 1 }])
  })

  it('addItem', async () => {
    const itemId = 1
    const { addItem } = useClanArmory()

    await addItem(itemId)

    expect(mockedAddItemToClanArmory).toBeCalledWith(clanId, itemId)
  })

  it('removeItem', async () => {
    const itemId = 1
    const { removeItem } = useClanArmory()

    await removeItem(itemId)

    expect(mockedRemoveItemFromClanArmory).toBeCalledWith(clanId, itemId)
  })

  it('borrowItem', async () => {
    const itemId = 1
    const { borrowItem } = useClanArmory()

    await borrowItem(itemId)

    expect(mockedBorrowItemFromClanArmory).toBeCalledWith(clanId, itemId)
  })

  it('returnItem', async () => {
    const itemId = 1
    const { returnItem } = useClanArmory()

    await returnItem(itemId)

    expect(mockedReturnItemToClanArmory).toBeCalledWith(clanId, itemId)
  })
})

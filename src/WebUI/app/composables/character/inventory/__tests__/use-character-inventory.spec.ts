import type { PartialDeep } from 'type-fest'

import { beforeEach, describe, expect, it, vi } from 'vitest'

import type { Clan } from '~/models/clan'
import type { UserItemsBySlot } from '~/models/user'

import { ITEM_SLOT, ITEM_TYPE } from '~/models/item'

import { useCharacterInventory } from '../use-character-inventory'

const {
  mockedUseUser,
  mockedFetchUser,
  mockedUserClan,
  mockedUseUserItems,
  mockedRefreshUserItems,
  mockedUseCharacterItems,
  mockedOnUpdateCharacterItems,
  mockedloadCharacterItems,
  mockedEquippedItemsBySlot,
  mockedGetLinkedSlots,
  mockedReforgeUserItem,
  mockedRepairUserItem,
  mockedSellUserItem,
  mockedUpgradeUserItem,
  mockedAddItemToClanArmory,
  mockedRemoveItemFromClanArmory,
  mockedReturnItemToClanArmory,
} = vi.hoisted(() => {
  const mockedFetchUser = vi.fn()
  const mockedRefreshUserItems = vi.fn()
  const mockedOnUpdateCharacterItems = vi.fn()
  const mockedloadCharacterItems = vi.fn()
  const mockedEquippedItemsBySlot = { value: {} }
  const mockedUserClan = { value: null } as { value: Partial<Clan> | null }

  return {
    mockedUseUser: vi.fn().mockReturnValue({
      clan: mockedUserClan,
      fetchUser: mockedFetchUser,
    }),
    mockedFetchUser,
    mockedUserClan,
    mockedUseUserItems: vi.fn().mockReturnValue({
      refreshUserItems: mockedRefreshUserItems,
    }),
    mockedRefreshUserItems,
    mockedUseCharacterItems: vi.fn().mockReturnValue({
      onUpdateCharacterItems: mockedOnUpdateCharacterItems,
      equippedItemsBySlot: mockedEquippedItemsBySlot,
      loadCharacterItems: mockedloadCharacterItems,
    }),
    mockedOnUpdateCharacterItems,
    mockedloadCharacterItems,
    mockedEquippedItemsBySlot,
    mockedGetLinkedSlots: vi.fn(),
    mockedReforgeUserItem: vi.fn(),
    mockedRepairUserItem: vi.fn(),
    mockedSellUserItem: vi.fn(),
    mockedUpgradeUserItem: vi.fn(),
    mockedAddItemToClanArmory: vi.fn(),
    mockedRemoveItemFromClanArmory: vi.fn(),
    mockedReturnItemToClanArmory: vi.fn(),
  }
})

vi.mock('~/composables/utils/use-async-callback', () => ({
  useAsyncCallback: vi.fn((callback: any) => [vi.fn((...args: any[]) => callback(...args))]),
}))

vi.mock('#imports', () => ({
  useI18n: vi.fn(() => ({ t: (key: string) => key })),
}))

vi.mock('~/composables/user/use-user', () => ({
  useUser: mockedUseUser,
}))

vi.mock('~/composables/user/use-user-items', () => ({
  useUserItems: mockedUseUserItems,
}))

vi.mock('~/composables/character/use-character-items', () => ({
  useCharacterItems: mockedUseCharacterItems,
}))

vi.mock('~/services/item-service', () => ({
  getLinkedSlots: mockedGetLinkedSlots,
}))

vi.mock('~/services/user-service', () => ({
  reforgeUserItem: mockedReforgeUserItem,
  repairUserItem: mockedRepairUserItem,
  sellUserItem: mockedSellUserItem,
  upgradeUserItem: mockedUpgradeUserItem,
}))

vi.mock('~/services/clan-service', () => ({
  addItemToClanArmory: mockedAddItemToClanArmory,
  removeItemFromClanArmory: mockedRemoveItemFromClanArmory,
  returnItemToClanArmory: mockedReturnItemToClanArmory,
}))

function expectRefreshData() {
  expect([mockedFetchUser, mockedRefreshUserItems, mockedloadCharacterItems].every(fn => fn.mock.calls.length > 0)).toBe(true)
}

describe('useCharacterInventory', () => {
  beforeEach(() => {
    vi.spyOn(mockedEquippedItemsBySlot, 'value', 'get').mockReturnValue({})
    vi.spyOn(mockedUserClan, 'value', 'get').mockReturnValue(null)
  })

  it('should sell user item and refresh data', async () => {
    const userItemsBySlot: PartialDeep<UserItemsBySlot> = {
      [ITEM_SLOT.Body]: { id: 1, item: { type: ITEM_TYPE.BodyArmor } },
    }

    vi.spyOn(mockedEquippedItemsBySlot, 'value', 'get').mockReturnValue(userItemsBySlot)
    mockedGetLinkedSlots.mockReturnValue([ITEM_SLOT.Head])

    await (useCharacterInventory().onSellUserItem(1))

    expect(mockedGetLinkedSlots).toHaveBeenCalledWith(ITEM_SLOT.Body, userItemsBySlot)
    expect(mockedOnUpdateCharacterItems).toHaveBeenCalledWith([{ slot: ITEM_SLOT.Head, userItemId: null }])
    expect(mockedSellUserItem).toHaveBeenCalledWith(1)
    expectRefreshData()
  })

  it('should repair user item and refresh data', async () => {
    await (useCharacterInventory().onRepairUserItem(2))

    expect(mockedRepairUserItem).toHaveBeenCalledWith(2)
    expectRefreshData()
  })

  it('should upgrade user item and refresh data', async () => {
    await (useCharacterInventory().onUpgradeUserItem(3))

    expect(mockedUpgradeUserItem).toHaveBeenCalledWith(3)
    expectRefreshData()
  })

  it('should reforge user item and refresh data', async () => {
    await (useCharacterInventory().onReforgeUserItem(4))

    expect(mockedReforgeUserItem).toHaveBeenCalledWith(4)
    expectRefreshData()
  })

  describe('onAddItemToClanArmory', () => {
    it('should add item to clan armory if clan exists', async () => {
      vi.spyOn(mockedUserClan, 'value', 'get').mockReturnValue({ id: 1 })

      await (useCharacterInventory().onAddItemToClanArmory(5))

      expect(mockedAddItemToClanArmory).toHaveBeenCalledWith(1, 5)
      expectRefreshData()
    })

    it('should not add item to clan armory if clan does not exist', async () => {
      vi.spyOn(mockedUserClan, 'value', 'get').mockReturnValue(null)
      await (useCharacterInventory().onAddItemToClanArmory(5))

      expect(mockedAddItemToClanArmory).not.toHaveBeenCalled()
    })
  })

  it('should return item to clan armory if clan exists', async () => {
    vi.spyOn(mockedUserClan, 'value', 'get').mockReturnValue({ id: 1 })
    await (useCharacterInventory().onReturnToClanArmory(6))

    expect(mockedReturnItemToClanArmory).toHaveBeenCalledWith(1, 6)
    expectRefreshData()
  })

  it('should remove item from clan armory if clan exists', async () => {
    vi.spyOn(mockedUserClan, 'value', 'get').mockReturnValue({ id: 1 })
    await (useCharacterInventory().onRemoveFromClanArmory(7))

    expect(mockedRemoveItemFromClanArmory).toHaveBeenCalledWith(1, 7)
    expectRefreshData()
  })
})

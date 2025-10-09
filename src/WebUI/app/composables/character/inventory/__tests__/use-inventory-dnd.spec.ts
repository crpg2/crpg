import type { PartialDeep } from 'type-fest'

import { afterEach, describe, expect, it, vi } from 'vitest'

import type { UserItem, UserItemsBySlot } from '~/models/user'

import { ITEM_SLOT, ITEM_TYPE } from '~/models/item'

import { _useInventoryDnD } from '../use-inventory-dnd'

const {
  mockedGetAvailableSlotsByItem,
  mockedGetUnEquipItems,
  mockedUseCharacterItems,
  mockedOnUpdateCharacterItems,
  mockedEquippedItemsBySlot,
  mockedUseToastAdd,
  mockedUseUser,
} = vi.hoisted(() => {
  const mockedOnUpdateCharacterItems = vi.fn()
  const mockedEquippedItemsBySlot = { value: {} }

  return {
    mockedGetAvailableSlotsByItem: vi.fn().mockReturnValue([]),
    mockedGetUnEquipItems: vi.fn().mockReturnValue([]),
    mockedUseCharacterItems: vi.fn().mockReturnValue({
      onUpdateCharacterItems: mockedOnUpdateCharacterItems,
      equippedItemsBySlot: mockedEquippedItemsBySlot,
    }),
    mockedOnUpdateCharacterItems,
    mockedEquippedItemsBySlot,
    mockedUseToastAdd: vi.fn(),
    mockedUseUser: vi.fn().mockReturnValue({ user: { value: { id: 1 } } }),
  }
})

vi.mock('#imports', () => ({
  useI18n: vi.fn(() => ({ t: (key: string) => key })),
  useToast: vi.fn(() => ({
    add: mockedUseToastAdd,
  })),
}))

vi.mock('~/services/item-service', () => ({
  getAvailableSlotsByItem: mockedGetAvailableSlotsByItem,
  getUnEquipItems: mockedGetUnEquipItems,
}))

vi.mock('~/composables/user/use-user', () => ({
  useUser: mockedUseUser,
}))

vi.mock('~/composables/character/use-character-items', () => ({
  useCharacterItems: mockedUseCharacterItems,
}))

describe('useInventoryDnD', () => {
  afterEach(() => {
    vi.spyOn(mockedEquippedItemsBySlot, 'value', 'get').mockReturnValue({})
  })

  describe('onDragStart', () => {
    const userItem: PartialDeep<UserItem> = {
      id: 1,
      item: { type: ITEM_TYPE.TwoHandedWeapon },
    }

    const AVAILABLE_SLOTS = [ITEM_SLOT.Weapon0, ITEM_SLOT.Weapon1, ITEM_SLOT.Weapon2, ITEM_SLOT.Weapon3]

    it('should do nothing if no item is provided', () => {
      const { availableSlots, focusedItemId, fromSlot, toSlot, onDragStart } = _useInventoryDnD()

      onDragStart(null, null)

      expect(focusedItemId.value).toEqual(null)
      expect(availableSlots.value).toEqual([])
      expect(fromSlot.value).toEqual(null)
      expect(toSlot.value).toEqual(null)
    })

    it('should focus and set available slots when dragging from inventory', () => {
      mockedGetAvailableSlotsByItem.mockReturnValueOnce({ slots: AVAILABLE_SLOTS, warning: null })

      const { availableSlots, focusedItemId, fromSlot, toSlot, onDragStart } = _useInventoryDnD()

      onDragStart(userItem as UserItem, null)

      expect(focusedItemId.value).toEqual(1)
      expect(availableSlots.value).toEqual(AVAILABLE_SLOTS)
      expect(fromSlot.value).toEqual(null)
      expect(toSlot.value).toEqual(null)
    })

    it('should remember fromSlot when dragging from equipped slot', () => {
      mockedGetAvailableSlotsByItem.mockReturnValueOnce({ slots: AVAILABLE_SLOTS, warning: null })

      const { availableSlots, focusedItemId, fromSlot, onDragStart } = _useInventoryDnD()

      onDragStart(userItem as UserItem, ITEM_SLOT.Weapon0)

      expect(focusedItemId.value).toEqual(1)
      expect(availableSlots.value).toEqual(AVAILABLE_SLOTS)
      expect(fromSlot.value).toEqual(ITEM_SLOT.Weapon0)
    })

    it('should show a warning toast if validation fails', () => {
      mockedGetAvailableSlotsByItem.mockReturnValueOnce({ slots: [], warning: 'some warn' })

      const { availableSlots, focusedItemId, fromSlot, onDragStart } = _useInventoryDnD()

      onDragStart(userItem as UserItem, null)

      expect(focusedItemId.value).toEqual(null)
      expect(availableSlots.value).toEqual([])
      expect(fromSlot.value).toEqual(null)
      expect(mockedUseToastAdd).toHaveBeenCalledWith(expect.objectContaining({ title: 'some warn' }))
    })
  })

  describe('hover behavior', () => {
    it('onDragEnter', () => {
      const { onDragEnter, toSlot } = _useInventoryDnD()

      expect(toSlot.value).toEqual(null)

      onDragEnter(ITEM_SLOT.Mount)

      expect(toSlot.value).toEqual(ITEM_SLOT.Mount)
    })

    it('onDragLeave', () => {
      const { onDragEnter, onDragLeave, toSlot } = _useInventoryDnD()

      onDragEnter(ITEM_SLOT.Mount)

      expect(toSlot.value).toEqual(ITEM_SLOT.Mount)

      onDragLeave()

      expect(toSlot.value).toEqual(null)
    })
  })

  describe('onDragEnd', () => {
    const userItemsBySlot: PartialDeep<UserItemsBySlot> = {
      [ITEM_SLOT.Head]: {
        id: 4,
        item: { type: ITEM_TYPE.HeadArmor },
      },
      [ITEM_SLOT.Weapon0]: {
        id: 3,
        item: { type: ITEM_TYPE.OneHandedWeapon },
      },
    }

    it('should not update items if no slot is provided', () => {
      const { onDragEnd, onDragEnter, toSlot } = _useInventoryDnD()

      onDragEnter(ITEM_SLOT.Mount)

      expect(toSlot.value).toEqual(ITEM_SLOT.Mount)

      onDragEnd()

      expect(mockedOnUpdateCharacterItems).not.toBeCalled()
      expect(toSlot.value).toBeNull()
    })

    it('should unequip item when dragged outside (toSlot is null)', () => {
      mockedGetUnEquipItems.mockReturnValueOnce([{ slot: ITEM_SLOT.Mount, userItemId: null }])
      vi.spyOn(mockedEquippedItemsBySlot, 'value', 'get').mockReturnValueOnce(userItemsBySlot)

      const { onDragEnd } = _useInventoryDnD()

      onDragEnd(null, ITEM_SLOT.Mount)

      expect(mockedGetUnEquipItems).toHaveBeenCalledWith(ITEM_SLOT.Mount, userItemsBySlot)
      expect(mockedOnUpdateCharacterItems).toHaveBeenCalledWith([{ slot: ITEM_SLOT.Mount, userItemId: null }])
    })

    it('should unequip multiple linked items when necessary', () => {
      mockedGetUnEquipItems.mockReturnValueOnce([
        { slot: ITEM_SLOT.Leg, userItemId: null },
        { slot: ITEM_SLOT.Body, userItemId: null },
      ])
      vi.spyOn(mockedEquippedItemsBySlot, 'value', 'get').mockReturnValueOnce(userItemsBySlot)

      const { onDragEnd } = _useInventoryDnD()

      onDragEnd(null, ITEM_SLOT.Leg)

      expect(mockedGetUnEquipItems).toHaveBeenCalledWith(ITEM_SLOT.Leg, userItemsBySlot)
      expect(mockedOnUpdateCharacterItems).toHaveBeenCalledWith([
        { slot: ITEM_SLOT.Leg, userItemId: null },
        { slot: ITEM_SLOT.Body, userItemId: null },
      ])
    })
  })

  describe('onDrop', () => {
    it('should ignore drop if target slot is not available', () => {
      mockedGetAvailableSlotsByItem.mockReturnValueOnce({ slots: [ITEM_SLOT.Mount], warning: null })
      const userItem: PartialDeep<UserItem> = { id: 1, item: { type: ITEM_TYPE.Mount } }

      const { onDragStart, onDrop } = _useInventoryDnD()

      onDragStart(userItem as UserItem)

      onDrop(ITEM_SLOT.MountHarness)

      expect(mockedOnUpdateCharacterItems).not.toHaveBeenCalled()
    })

    it('should equip item if target slot is available and empty', () => {
      mockedGetAvailableSlotsByItem.mockReturnValueOnce({ slots: [ITEM_SLOT.Mount], warning: null })
      const userItem: PartialDeep<UserItem> = { id: 1, item: { type: ITEM_TYPE.Mount } }

      const { onDragStart, onDrop } = _useInventoryDnD()

      onDragStart(userItem as UserItem)

      onDrop(ITEM_SLOT.Mount)

      expect(mockedOnUpdateCharacterItems).toHaveBeenCalledWith([{ slot: ITEM_SLOT.Mount, userItemId: 1 }])
    })

    it('should replace item if target slot is already occupied', () => {
      mockedGetAvailableSlotsByItem.mockReturnValueOnce({ slots: [ITEM_SLOT.Head], warning: null })
      const userItem: PartialDeep<UserItem> = { id: 1, item: { type: ITEM_TYPE.HeadArmor } }

      const { onDragStart, onDrop } = _useInventoryDnD()

      onDragStart(userItem as UserItem)

      onDrop(ITEM_SLOT.Head)

      expect(mockedOnUpdateCharacterItems).toHaveBeenCalledWith([{ slot: ITEM_SLOT.Head, userItemId: 1 }])
    })

    it('should swap items when dropping between occupied weapon slots', () => {
      const userItemsBySlot: PartialDeep<UserItemsBySlot> = {
        [ITEM_SLOT.Weapon0]: {
          id: 4,
          item: { type: ITEM_TYPE.OneHandedWeapon },
        },
        [ITEM_SLOT.Weapon1]: {
          id: 3,
          item: { type: ITEM_TYPE.TwoHandedWeapon },
        },
      }
      vi.spyOn(mockedEquippedItemsBySlot, 'value', 'get').mockReturnValue(userItemsBySlot)
      mockedGetAvailableSlotsByItem.mockReturnValue({ slots: [ITEM_SLOT.Weapon0, ITEM_SLOT.Weapon1, ITEM_SLOT.Weapon2, ITEM_SLOT.Weapon3], warning: null })
      const userItem: PartialDeep<UserItem> = { id: 1, item: { type: ITEM_TYPE.TwoHandedWeapon } }

      const { onDragStart, onDrop } = _useInventoryDnD()

      onDragStart(userItem as UserItem, ITEM_SLOT.Weapon1)

      onDrop(ITEM_SLOT.Weapon0)

      expect(mockedOnUpdateCharacterItems).toHaveBeenCalledWith([
        { slot: ITEM_SLOT.Weapon0, userItemId: 1 },
        { slot: ITEM_SLOT.Weapon1, userItemId: 4 },
      ])
    })
  })

  it('should reset all reactive states after onDragEnd', () => {
    mockedGetAvailableSlotsByItem.mockReturnValueOnce({ slots: [ITEM_SLOT.Head], warning: null })
    const userItem: PartialDeep<UserItem> = { id: 123, item: { type: ITEM_TYPE.HeadArmor } }

    const {
      focusedItemId,
      availableSlots,
      fromSlot,
      toSlot,
      onDragStart,
      onDragEnter,
      onDragEnd,
    } = _useInventoryDnD()

    expect(focusedItemId.value).toBeNull()
    expect(availableSlots.value).toEqual([])
    expect(fromSlot.value).toBeNull()
    expect(toSlot.value).toBeNull()

    onDragStart(userItem as UserItem, ITEM_SLOT.Head)
    onDragEnter(ITEM_SLOT.Mount)

    expect(focusedItemId.value).toEqual(123)
    expect(availableSlots.value).toEqual([ITEM_SLOT.Head])
    expect(fromSlot.value).toEqual(ITEM_SLOT.Head)
    expect(toSlot.value).toEqual(ITEM_SLOT.Mount)

    onDragEnd()

    expect(focusedItemId.value).toBeNull()
    expect(availableSlots.value).toEqual([])
    expect(fromSlot.value).toBeNull()
    expect(toSlot.value).toBeNull()
  })
})

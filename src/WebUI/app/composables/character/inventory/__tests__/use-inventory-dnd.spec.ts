import type { PartialDeep } from 'type-fest'

import { describe, expect, it, vi } from 'vitest'
import { ref } from 'vue'

import type { UserItem, UserItemsBySlot } from '~/models/user'

import { ITEM_SLOT, ITEM_TYPE } from '~/models/item'

import { useInventoryDnD } from '../use-inventory-dnd'

const userItemsBySlot: PartialDeep<UserItemsBySlot> = {
  [ITEM_SLOT.Head]: {
    id: 4,
    item: {
      type: ITEM_TYPE.HeadArmor,
    },
  },
  [ITEM_SLOT.Weapon0]: {
    id: 3,
    item: {
      type: ITEM_TYPE.OneHandedWeapon,
    },
  },
}

const {
  mockedGetAvailableSlotsByItem,
  mockedGetUnEquipItemsLinked,
  mockedUseCharacterItems,
  // mockedOnUpdateCharacterItems,
  // mockedEquippedItemsBySlot,
  mockedUseToastAdd,
  mockedUseUser,
} = vi.hoisted(() => {
  const mockedOnUpdateCharacterItems = vi.fn()
  const mockedEquippedItemsBySlot = ref({}) // TODO:

  return {
    mockedGetAvailableSlotsByItem: vi.fn().mockReturnValue([]),
    mockedGetUnEquipItemsLinked: vi.fn().mockReturnValue([]),
    mockedUseCharacterItems: vi.fn().mockReturnValue({
      onUpdateCharacterItems: vi.fn(),
      equippedItemsBySlot: ref({}),
    }),
    // mockedOnUpdateCharacterItems,
    // mockedEquippedItemsBySlot,
    mockedUseToastAdd: vi.fn(),
    mockedUseUser: vi.fn<() => PartialDeep<ReturnType<typeof import('~/composables/user/use-user')['useUser']>>>().mockReturnValue({ user: { value: { id: 1 } } }),
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
  getUnEquipItemsLinked: mockedGetUnEquipItemsLinked,
}))

vi.mock('~/composables/user/use-user', () => ({
  useUser: mockedUseUser,
}))

vi.mock('~/composables/character/use-character-items', () => ({
  useCharacterItems: mockedUseCharacterItems,
}))

describe('useInventoryDnD', () => {
  describe('onDragStart', () => {
    const userItem: PartialDeep<UserItem> = {
      id: 1,
      item: { type: ITEM_TYPE.TwoHandedWeapon },
    }

    const AVAILABLE_SLOTS = [ITEM_SLOT.Weapon0, ITEM_SLOT.Weapon1, ITEM_SLOT.Weapon2, ITEM_SLOT.Weapon3]

    it('no item', () => {
      const { availableSlots, focusedItemId, fromSlot, toSlot, onDragEnd, onDragStart } = useInventoryDnD()

      onDragStart(null, null)

      expect(focusedItemId.value).toEqual(null)
      expect(availableSlots.value).toEqual([])
      expect(fromSlot.value).toEqual(null)
      expect(toSlot.value).toEqual(null)

      onDragEnd() // reset shared state
    })

    it('weapon: from inventory, to doll', () => {
      mockedGetAvailableSlotsByItem.mockReturnValueOnce({ slots: AVAILABLE_SLOTS, warning: null })

      const { availableSlots, focusedItemId, fromSlot, toSlot, onDragEnd, onDragStart } = useInventoryDnD()

      onDragStart(userItem as UserItem, null)

      expect(focusedItemId.value).toEqual(1)
      expect(availableSlots.value).toEqual(AVAILABLE_SLOTS)
      expect(fromSlot.value).toEqual(null)
      expect(toSlot.value).toEqual(null)

      onDragEnd()
    })

    it('weapon: from doll, to doll (another slot)', () => {
      mockedGetAvailableSlotsByItem.mockReturnValueOnce({ slots: AVAILABLE_SLOTS, warning: null })

      const { availableSlots, focusedItemId, fromSlot, onDragEnd, onDragStart } = useInventoryDnD()

      onDragStart(userItem as UserItem, ITEM_SLOT.Weapon0)

      expect(focusedItemId.value).toEqual(1)
      expect(availableSlots.value).toEqual(AVAILABLE_SLOTS)
      expect(fromSlot.value).toEqual(ITEM_SLOT.Weapon0)

      onDragEnd()
    })

    it('validation failed', () => {
      mockedGetAvailableSlotsByItem.mockReturnValueOnce({ slots: [], warning: 'some warn' })

      const { availableSlots, focusedItemId, fromSlot, onDragEnd, onDragStart } = useInventoryDnD()

      onDragStart(userItem as UserItem, null)

      expect(focusedItemId.value).toEqual(null)
      expect(availableSlots.value).toEqual([])
      expect(fromSlot.value).toEqual(null)
      expect(mockedUseToastAdd).toBeCalledWith(expect.objectContaining({ title: 'some warn' }))

      onDragEnd()
    })
  })

  it('onDragEnter', () => {
    const { onDragEnter, toSlot } = useInventoryDnD()

    expect(toSlot.value).toEqual(null)

    onDragEnter(ITEM_SLOT.Mount)

    expect(toSlot.value).toEqual(ITEM_SLOT.Mount)
  })

  it('onDragLeave', () => {
    const { onDragEnter, onDragLeave, toSlot } = useInventoryDnD()

    onDragEnter(ITEM_SLOT.Mount)

    expect(toSlot.value).toEqual(ITEM_SLOT.Mount)

    onDragLeave()

    expect(toSlot.value).toEqual(null)
  })

  describe('onDragEnd', () => {
    it('empty slot', () => {
      const { onUpdateCharacterItems } = mockedUseCharacterItems()

      const { onDragEnd, onDragEnter, toSlot } = useInventoryDnD()

      onDragEnter(ITEM_SLOT.Mount)

      expect(toSlot.value).toEqual(ITEM_SLOT.Mount)

      onDragEnd()

      expect(onUpdateCharacterItems).not.toBeCalled()
      expect(toSlot.value).toBeNull()
    })

    it('with slot, empty toSlot - drag item outside = unEquip item', () => {
      const { equippedItemsBySlot, onUpdateCharacterItems } = mockedUseCharacterItems()
      equippedItemsBySlot.value = userItemsBySlot
      // equippedItemsBySlot.mockReturnValueOnce([{ slot: ITEM_SLOT.Mount, userItemId: null }])
      // mockedUseCharacterItems.mockReturnValueOnce(userItemsBySlot)
      const { onDragEnd } = useInventoryDnD()

      onDragEnd(null, ITEM_SLOT.Mount)

      expect(mockedGetUnEquipItemsLinked).toBeCalledWith(ITEM_SLOT.Mount, userItemsBySlot)
      expect(onUpdateCharacterItems).toBeCalledWith([{ slot: ITEM_SLOT.Mount, userItemId: null }])

      equippedItemsBySlot.value = {}
    })

    // it('unEquip item + unEquip linked items', () => {
    //   mockedGetLinkedSlots.mockReturnValueOnce([ItemSlot.Body])
    //   const { onDragEnd } = useInventoryDnD(ref(userItemsBySlot as UserItemsBySlot))

    //   getUnEquipItemsLinked.mockReturnValueOnce([
    //     { slot: ItemSlot.Leg, userItemId: null },
    //     { slot: ItemSlot.Body, userItemId: null },
    //   ])

    //   onDragEnd(null, ItemSlot.Leg)

    //   expect(getUnEquipItemsLinked).toBeCalledWith(ItemSlot.Leg, userItemsBySlot)
    //   expect(mockedEmit).toBeCalledWith('change', [
    //     { slot: ItemSlot.Leg, userItemId: null },
    //     { slot: ItemSlot.Body, userItemId: null },
    //   ])
    // })
  })

  // describe('onDrop', () => {
  //   it('to empty, not available slots', () => {
  //     mockedGetAvailableSlotsByItem.mockReturnValue([ItemSlot.Mount])

  //     const userItem: PartialDeep<UserItem> = {
  //       id: 1,
  //       item: { flags: [], type: ItemType.Mount },
  //     }

  //     const { onDragEnd, onDragStart, onDrop } = useInventoryDnD(
  //       ref(userItemsBySlot as UserItemsBySlot),
  //     )

  //     onDragStart(userItem as UserItem)

  //     onDrop(ItemSlot.MountHarness)

  //     expect(mockedEmit).not.toBeCalled()

  //     onDragEnd()
  //   })

  //   it('to empty slot, available slot', () => {
  //     mockedGetAvailableSlotsByItem.mockReturnValue([ItemSlot.Mount])

  //     const userItem: PartialDeep<UserItem> = {
  //       id: 1,
  //       item: { flags: [], type: ItemType.Mount },
  //     }

  //     const { onDragEnd, onDragStart, onDrop } = useInventoryDnD(
  //       ref(userItemsBySlot as UserItemsBySlot),
  //     )

  //     onDragStart(userItem as UserItem)

  //     onDrop(ItemSlot.Mount)

  //     expect(mockedEmit).toBeCalledWith('change', [{ slot: ItemSlot.Mount, userItemId: 1 }])

  //     onDragEnd()
  //   })

  //   it('to full slot, available slot', () => {
  //     mockedGetAvailableSlotsByItem.mockReturnValue([ItemSlot.Head])

  //     const userItem: PartialDeep<UserItem> = {
  //       id: 1,
  //       item: { flags: [], type: ItemType.HeadArmor },
  //     }

  //     const { onDragEnd, onDragStart, onDrop } = useInventoryDnD(
  //       ref(userItemsBySlot as UserItemsBySlot),
  //     )

  //     onDragStart(userItem as UserItem)

  //     onDrop(ItemSlot.Head)

  //     expect(mockedEmit).toBeCalledWith('change', [{ slot: ItemSlot.Head, userItemId: 1 }])

  //     onDragEnd()
  //   })

  //   it('swap items - drop item from ItemSlot.Weapon1 to ItemSlot.Weapon0', () => {
  //     const AVAILABLE_SLOTS = [
  //       ItemSlot.Weapon0,
  //       ItemSlot.Weapon1,
  //       ItemSlot.Weapon2,
  //       ItemSlot.Weapon3,
  //     ]

  //     mockedGetAvailableSlotsByItem.mockReturnValue(AVAILABLE_SLOTS)

  //     const userItem: PartialDeep<UserItem> = {
  //       id: 2,
  //       item: { flags: [], type: ItemType.TwoHandedWeapon },
  //     }

  //     const { onDragEnd, onDragStart, onDrop } = useInventoryDnD(
  //       ref({
  //         [ItemSlot.Weapon0]: {
  //           id: 1,
  //           item: {
  //             type: ItemType.OneHandedWeapon,
  //           },
  //         },
  //         [ItemSlot.Weapon1]: {
  //           id: 2,
  //           item: {
  //             type: ItemType.TwoHandedWeapon,
  //           },
  //         },
  //       } as UserItemsBySlot),
  //     )

  //     onDragStart(userItem as UserItem, ItemSlot.Weapon1)

  //     onDrop(ItemSlot.Weapon0)

  //     expect(mockedEmit).toBeCalledWith('change', [
  //       { slot: ItemSlot.Weapon0, userItemId: 2 },
  //       { slot: ItemSlot.Weapon1, userItemId: 1 },
  //     ])

  //     onDragEnd()
  //   })
  // })
})

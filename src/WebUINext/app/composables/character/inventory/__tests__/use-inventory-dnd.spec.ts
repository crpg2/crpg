import type { PartialDeep } from 'type-fest'

import { createTestingPinia } from '@pinia/testing'

import type { UserItem, UserItemsBySlot } from '~/models/user'

import { ItemSlot, ItemType } from '~/models/item'
import { useUserStore } from '~/stores/user'

import { useInventoryDnD } from '../use-inventory-dnd'

const userStore = useUserStore(createTestingPinia())

const {
  mockedEmit,
  mockedGetAvailableSlotsByItem,
  mockedGetLinkedSlots,
  mockedUseInventoryEquipment,
} = vi.hoisted(() => ({
  mockedEmit: vi.fn(),
  mockedGetAvailableSlotsByItem: vi.fn().mockReturnValue([]),
  mockedGetLinkedSlots: vi.fn().mockReturnValue([]),
  mockedUseInventoryEquipment: vi.fn().mockReturnValue({
    getUnEquipItemsLinked: vi.fn(),
    isEquipItemAllowed: vi.fn().mockReturnValue(true),
  }),
}))

vi.mock('vue', async () => ({
  ...(await vi.importActual<typeof import('vue')>('vue')),
  getCurrentInstance: vi.fn().mockImplementation(() => ({
    emit: mockedEmit,
  })),
}))

vi.mock('~/services/item-service', () => ({
  getAvailableSlotsByItem: mockedGetAvailableSlotsByItem,
  getLinkedSlots: mockedGetLinkedSlots,
}))

vi.mock('~/composables/character/use-inventory-equipment', () => ({
  useInventoryEquipment: mockedUseInventoryEquipment,
}))

const userItemsBySlot: PartialDeep<UserItemsBySlot> = {
  [ItemSlot.Head]: {
    id: 4,
    item: {
      type: ItemType.HeadArmor,
    },
  },
  [ItemSlot.Weapon0]: {
    id: 3,
    item: {
      type: ItemType.OneHandedWeapon,
    },
  },
}

const { getUnEquipItemsLinked, isEquipItemAllowed } = mockedUseInventoryEquipment()

describe('useInventoryDnD', () => {
  describe('onDragStart', () => {
    it('no item', () => {
      const { availableSlots, focusedItemId, fromSlot, onDragEnd, onDragStart } = useInventoryDnD(
        ref(userItemsBySlot as UserItemsBySlot),
      )

      onDragStart(null, null)

      expect(focusedItemId.value).toEqual(null)
      expect(availableSlots.value).toEqual([])
      expect(fromSlot.value).toEqual(null)

      onDragEnd() // reset shared state
    })

    it('weapon: from inventory, to doll', () => {
      userStore.$patch({ user: { id: 1 } })

      const userItem: PartialDeep<UserItem> = {
        id: 1,
        item: { flags: [], type: ItemType.TwoHandedWeapon },
      }

      const AVAILABLE_SLOTS = [
        ItemSlot.Weapon0,
        ItemSlot.Weapon1,
        ItemSlot.Weapon2,
        ItemSlot.Weapon3,
      ]

      mockedGetAvailableSlotsByItem.mockReturnValue(AVAILABLE_SLOTS)

      const { availableSlots, focusedItemId, fromSlot, onDragEnd, onDragStart } = useInventoryDnD(
        ref(userItemsBySlot as UserItemsBySlot),
      )

      onDragStart(userItem as UserItem, ItemSlot.Weapon1)

      expect(focusedItemId.value).toEqual(1)
      expect(availableSlots.value).toEqual(AVAILABLE_SLOTS)
      expect(fromSlot.value).toEqual(ItemSlot.Weapon1)

      onDragEnd()
    })

    it('weapon: from doll, to doll (another slot)', () => {
      userStore.$patch({ user: { id: 1 } })

      const userItem: PartialDeep<UserItem> = {
        id: 1,
        item: { flags: [], type: ItemType.TwoHandedWeapon },
      }

      const AVAILABLE_SLOTS = [
        ItemSlot.Weapon0,
        ItemSlot.Weapon1,
        ItemSlot.Weapon2,
        ItemSlot.Weapon3,
      ]

      mockedGetAvailableSlotsByItem.mockReturnValue(AVAILABLE_SLOTS)

      const { availableSlots, focusedItemId, fromSlot, onDragEnd, onDragStart } = useInventoryDnD(
        ref(userItemsBySlot as UserItemsBySlot),
      )

      onDragStart(userItem as UserItem, null)

      expect(focusedItemId.value).toEqual(1)
      expect(availableSlots.value).toEqual(AVAILABLE_SLOTS)
      expect(fromSlot.value).toEqual(null)

      onDragEnd()
    })

    it('broken item', () => {
      userStore.$patch({ user: { id: 1 } })

      const userItem: PartialDeep<UserItem> = {
        id: 42,
        isBroken: true,
        item: { flags: [], type: ItemType.HeadArmor },
      }

      const { availableSlots, focusedItemId, fromSlot, onDragEnd, onDragStart } = useInventoryDnD(
        ref(userItemsBySlot as UserItemsBySlot),
      )

      isEquipItemAllowed.mockReturnValueOnce(false)

      onDragStart(userItem as UserItem, ItemSlot.Head)

      expect(focusedItemId.value).toEqual(null)
      expect(availableSlots.value).toEqual([])
      expect(fromSlot.value).toEqual(null)

      onDragEnd()
    })

    it('isArmoryItem item', () => {
      userStore.$patch({ user: { id: 1 } })

      const userItem: PartialDeep<UserItem> = {
        id: 42,
        isArmoryItem: true,
        item: { flags: [], type: ItemType.HeadArmor },
        userId: 1,
      }

      const { availableSlots, focusedItemId, fromSlot, onDragEnd, onDragStart } = useInventoryDnD(
        ref(userItemsBySlot as UserItemsBySlot),
      )

      isEquipItemAllowed.mockReturnValueOnce(false)

      onDragStart(userItem as UserItem, ItemSlot.Head)

      expect(focusedItemId.value).toEqual(null)
      expect(availableSlots.value).toEqual([])
      expect(fromSlot.value).toEqual(null)

      onDragEnd()
    })
  })

  it('onDragEnter', () => {
    const { onDragEnter, toSlot } = useInventoryDnD(ref(userItemsBySlot as UserItemsBySlot))

    expect(toSlot.value).toBeNull()

    onDragEnter(ItemSlot.Mount)

    expect(toSlot.value).toEqual(ItemSlot.Mount)
  })

  it('onDragLeave', () => {
    const { onDragEnter, onDragLeave, toSlot } = useInventoryDnD(
      ref(userItemsBySlot as UserItemsBySlot),
    )

    onDragEnter(ItemSlot.Mount)

    expect(toSlot.value).toEqual(ItemSlot.Mount)

    onDragLeave()

    expect(toSlot.value).toBeNull()
  })

  describe('onDragEnd', () => {
    it('empty slot', () => {
      const { onDragEnd, onDragEnter, toSlot } = useInventoryDnD(
        ref(userItemsBySlot as UserItemsBySlot),
      )

      onDragEnter(ItemSlot.Mount)

      expect(toSlot.value).toEqual(ItemSlot.Mount)

      onDragEnd()

      expect(mockedEmit).not.toBeCalled()
      expect(toSlot.value).toBeNull()
    })

    it('empty slot, with toSlot', () => {
      const { onDragEnd } = useInventoryDnD(ref(userItemsBySlot as UserItemsBySlot))

      onDragEnd()

      expect(mockedEmit).not.toBeCalled()
    })

    it('with slot, empty toSlot - drag item outside = unEquip item', () => {
      const { onDragEnd } = useInventoryDnD(ref(userItemsBySlot as UserItemsBySlot))

      getUnEquipItemsLinked.mockReturnValueOnce([{ slot: ItemSlot.Mount, userItemId: null }])

      onDragEnd(null, ItemSlot.Mount)

      expect(getUnEquipItemsLinked).toBeCalledWith(ItemSlot.Mount, userItemsBySlot)
      expect(mockedEmit).toBeCalledWith('change', [{ slot: ItemSlot.Mount, userItemId: null }])
    })

    it('unEquip item + unEquip linked items', () => {
      mockedGetLinkedSlots.mockReturnValueOnce([ItemSlot.Body])
      const { onDragEnd } = useInventoryDnD(ref(userItemsBySlot as UserItemsBySlot))

      getUnEquipItemsLinked.mockReturnValueOnce([
        { slot: ItemSlot.Leg, userItemId: null },
        { slot: ItemSlot.Body, userItemId: null },
      ])

      onDragEnd(null, ItemSlot.Leg)

      expect(getUnEquipItemsLinked).toBeCalledWith(ItemSlot.Leg, userItemsBySlot)
      expect(mockedEmit).toBeCalledWith('change', [
        { slot: ItemSlot.Leg, userItemId: null },
        { slot: ItemSlot.Body, userItemId: null },
      ])
    })
  })

  describe('onDrop', () => {
    it('to empty, not available slots', () => {
      mockedGetAvailableSlotsByItem.mockReturnValue([ItemSlot.Mount])

      const userItem: PartialDeep<UserItem> = {
        id: 1,
        item: { flags: [], type: ItemType.Mount },
      }

      const { onDragEnd, onDragStart, onDrop } = useInventoryDnD(
        ref(userItemsBySlot as UserItemsBySlot),
      )

      onDragStart(userItem as UserItem)

      onDrop(ItemSlot.MountHarness)

      expect(mockedEmit).not.toBeCalled()

      onDragEnd()
    })

    it('to empty slot, available slot', () => {
      mockedGetAvailableSlotsByItem.mockReturnValue([ItemSlot.Mount])

      const userItem: PartialDeep<UserItem> = {
        id: 1,
        item: { flags: [], type: ItemType.Mount },
      }

      const { onDragEnd, onDragStart, onDrop } = useInventoryDnD(
        ref(userItemsBySlot as UserItemsBySlot),
      )

      onDragStart(userItem as UserItem)

      onDrop(ItemSlot.Mount)

      expect(mockedEmit).toBeCalledWith('change', [{ slot: ItemSlot.Mount, userItemId: 1 }])

      onDragEnd()
    })

    it('to full slot, available slot', () => {
      mockedGetAvailableSlotsByItem.mockReturnValue([ItemSlot.Head])

      const userItem: PartialDeep<UserItem> = {
        id: 1,
        item: { flags: [], type: ItemType.HeadArmor },
      }

      const { onDragEnd, onDragStart, onDrop } = useInventoryDnD(
        ref(userItemsBySlot as UserItemsBySlot),
      )

      onDragStart(userItem as UserItem)

      onDrop(ItemSlot.Head)

      expect(mockedEmit).toBeCalledWith('change', [{ slot: ItemSlot.Head, userItemId: 1 }])

      onDragEnd()
    })

    it('swap items - drop item from ItemSlot.Weapon1 to ItemSlot.Weapon0', () => {
      const AVAILABLE_SLOTS = [
        ItemSlot.Weapon0,
        ItemSlot.Weapon1,
        ItemSlot.Weapon2,
        ItemSlot.Weapon3,
      ]

      mockedGetAvailableSlotsByItem.mockReturnValue(AVAILABLE_SLOTS)

      const userItem: PartialDeep<UserItem> = {
        id: 2,
        item: { flags: [], type: ItemType.TwoHandedWeapon },
      }

      const { onDragEnd, onDragStart, onDrop } = useInventoryDnD(
        ref({
          [ItemSlot.Weapon0]: {
            id: 1,
            item: {
              type: ItemType.OneHandedWeapon,
            },
          },
          [ItemSlot.Weapon1]: {
            id: 2,
            item: {
              type: ItemType.TwoHandedWeapon,
            },
          },
        } as UserItemsBySlot),
      )

      onDragStart(userItem as UserItem, ItemSlot.Weapon1)

      onDrop(ItemSlot.Weapon0)

      expect(mockedEmit).toBeCalledWith('change', [
        { slot: ItemSlot.Weapon0, userItemId: 2 },
        { slot: ItemSlot.Weapon1, userItemId: 1 },
      ])

      onDragEnd()
    })
  })
})

import type { PartialDeep } from 'type-fest'

import type { EquippedItemsBySlot } from '~/models/character'
import type { UserItem } from '~/models/user'

import { ItemFamilyType, ItemSlot, ItemType } from '~/models/item'

import { useInventoryEquipment } from '../use-inventory-equipment'

const { mockedGetLinkedSlots, mockedNotify } = vi.hoisted(() => ({
  mockedGetLinkedSlots: vi.fn().mockReturnValue([]),
  mockedNotify: vi.fn(),
}))

vi.mock('~/services/notification-service', async () => ({
  ...(await vi.importActual<typeof import('~/services/notification-service')>(
    '~/services/notification-service',
  )),
  notify: mockedNotify,
}))

vi.mock('~/services/item-service', () => ({
  getLinkedSlots: mockedGetLinkedSlots,
}))

describe('useInventoryEquipment', () => {
  describe('isEquipItemAllowed', () => {
    it('broken item', () => {
      const userItem: PartialDeep<UserItem> = {
        id: 42,
        isBroken: true,
        item: { flags: [], type: ItemType.HeadArmor },
      }

      const { isEquipItemAllowed } = useInventoryEquipment()

      const result = isEquipItemAllowed(userItem as UserItem, 1)

      expect(mockedNotify).toBeCalledWith(
        'character.inventory.item.broken.notify.warning',
        'warning',
      )
      expect(result).toBe(false)
    })

    it('item in armory', () => {
      const userItem: PartialDeep<UserItem> = {
        id: 42,
        isArmoryItem: true,
        item: { flags: [], type: ItemType.HeadArmor },
        userId: 1,
      }

      const { isEquipItemAllowed } = useInventoryEquipment()

      const result = isEquipItemAllowed(userItem as UserItem, 1)

      expect(mockedNotify).toBeCalledWith(
        'character.inventory.item.clanArmory.inArmory.notify.warning',
        'warning',
      )
      expect(result).toBe(false)
    })

    it('item should be allowed', () => {
      const userItem: PartialDeep<UserItem> = {
        id: 42,
        item: { flags: [], type: ItemType.HeadArmor },
      }

      const { isEquipItemAllowed } = useInventoryEquipment()

      const result = isEquipItemAllowed(userItem as UserItem, 1)

      expect(result).toBe(true)
    })
  })

  describe('getUnEquipItemsLinked', () => {
    it('returns unequip items correctly', () => {
      const equippedItemsBySlot: PartialDeep<EquippedItemsBySlot> = {
        [ItemSlot.Body]: {
          id: 4,
          item: {
            armor: {
              familyType: ItemFamilyType.EBA,
            },
            type: ItemType.BodyArmor,
          },
        },
        [ItemSlot.Leg]: {
          id: 3,
          item: {
            armor: {
              familyType: ItemFamilyType.EBA,
            },
            type: ItemType.LegArmor,
          },
        },
      }

      mockedGetLinkedSlots.mockReturnValueOnce([ItemSlot.Body])
      const { getUnEquipItemsLinked } = useInventoryEquipment()

      const result = getUnEquipItemsLinked(ItemSlot.Leg, equippedItemsBySlot as EquippedItemsBySlot)

      expect(mockedGetLinkedSlots).toBeCalledWith(ItemSlot.Leg, equippedItemsBySlot as EquippedItemsBySlot)
      expect(result).toStrictEqual([
        { slot: ItemSlot.Leg, userItemId: null },
        { slot: ItemSlot.Body, userItemId: null },
      ])
    })
  })
})

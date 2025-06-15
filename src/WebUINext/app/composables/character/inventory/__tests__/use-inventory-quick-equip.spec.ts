import type { PartialDeep } from 'type-fest'

import { createTestingPinia } from '@pinia/testing'

import type { UserItem, UserItemsBySlot } from '~/models/user'

import { ItemSlot, ItemType } from '~/models/item'
import { useUserStore } from '~/stores/user'
import { characterItemsKey, characterKey } from '~/symbols/character'

import { useInventoryQuickEquip } from '../use-inventory-quick-equip'

const userStore = useUserStore(createTestingPinia())

const {
  mockedCheckIsWeaponBySlot,
  mockedGetAvailableSlotsByItem,
  mockedInjectStrict,
  mockedUpdateCharacterItems,
  mockedUseInventoryEquipment,
} = vi.hoisted(() => ({
  mockedCheckIsWeaponBySlot: vi.fn(),
  mockedEmit: vi.fn(),
  mockedGetAvailableSlotsByItem: vi.fn(),
  mockedInjectStrict: vi.fn().mockImplementation((key) => {
    if (key === characterItemsKey) {
      return { loadCharacterItems: vi.fn() }
    }
    else if (key === characterKey) {
      return {
        value: {
          id: 1,
        },
      }
    }
  }),
  mockedLoadCharacterItems: vi.fn(),
  mockedUpdateCharacterItems: vi.fn(),
  mockedUseInventoryEquipment: vi.fn().mockReturnValue({
    getUnEquipItemsLinked: vi.fn(),
    isEquipItemAllowed: vi.fn().mockReturnValue(true),
  }),
}))

vi.mock('~/composables/character/use-inventory-equipment', () => ({
  useInventoryEquipment: mockedUseInventoryEquipment,
}))

vi.mock('~/services/item-service', () => ({
  checkIsWeaponBySlot: mockedCheckIsWeaponBySlot,
  getAvailableSlotsByItem: mockedGetAvailableSlotsByItem,
}))

vi.mock('~/services/characters-service', () => ({
  updateCharacterItems: mockedUpdateCharacterItems,
}))

vi.mock('~/utils/inject-strict/index', () => ({
  injectStrict: mockedInjectStrict,
}))

describe('useInventoryQuickEquip', () => {
  describe('onQuickEquip', () => {
    it('should update items', () => {
      userStore.$patch({ user: { id: 1 } })

      const userItem: PartialDeep<UserItem> = {
        id: 1,
        item: { flags: [], type: ItemType.BodyArmor },
      }

      const AVAILABLE_SLOTS = [ItemSlot.Body]

      mockedGetAvailableSlotsByItem.mockReturnValue(AVAILABLE_SLOTS)
      mockedCheckIsWeaponBySlot.mockReturnValue(false)

      const { onQuickEquip } = useInventoryQuickEquip(ref({} as UserItemsBySlot))

      onQuickEquip(userItem as UserItem)

      expect(mockedGetAvailableSlotsByItem).toBeCalledWith(userItem.item, {})
      expect(mockedUpdateCharacterItems).toBeCalledWith(1, [
        {
          slot: ItemSlot.Body,
          userItemId: 1,
        },
      ])
    })

    it('should update non weapon items when slot already filled', () => {
      userStore.$patch({ user: { id: 1 } })

      const userItem: PartialDeep<UserItem> = {
        id: 1,
        item: { flags: [], type: ItemType.BodyArmor },
      }

      const AVAILABLE_SLOTS = [ItemSlot.Body]

      mockedGetAvailableSlotsByItem.mockReturnValue(AVAILABLE_SLOTS)
      mockedCheckIsWeaponBySlot.mockReturnValue(false)

      const userItemsBySlot: PartialDeep<UserItemsBySlot> = {
        [ItemSlot.Body]: {
          id: 2,
          item: {
            type: ItemType.BodyArmor,
          },
        },
      }

      const { onQuickEquip } = useInventoryQuickEquip(ref(userItemsBySlot as UserItemsBySlot))

      onQuickEquip(userItem as UserItem)

      expect(mockedGetAvailableSlotsByItem).toBeCalledWith(userItem.item, userItemsBySlot)
      expect(mockedUpdateCharacterItems).toBeCalledWith(1, [
        {
          slot: ItemSlot.Body,
          userItemId: 1,
        },
      ])
    })

    it('should quick equip weapon to the next free slot', () => {
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
      mockedCheckIsWeaponBySlot.mockReturnValue(true)

      const userItemsBySlot: PartialDeep<UserItemsBySlot> = {
        [ItemSlot.Weapon0]: {
          id: 2,
          item: {
            type: ItemType.OneHandedWeapon,
          },
        },
      }

      const { onQuickEquip } = useInventoryQuickEquip(ref(userItemsBySlot as UserItemsBySlot))

      onQuickEquip(userItem as UserItem)

      expect(mockedGetAvailableSlotsByItem).toBeCalledWith(userItem.item, userItemsBySlot)
      expect(mockedUpdateCharacterItems).toBeCalledWith(1, [
        {
          slot: ItemSlot.Weapon1,
          userItemId: 1,
        },
      ])
    })

    it('should not equip weapon when all slots are full', () => {
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
      mockedCheckIsWeaponBySlot.mockReturnValue(true)

      const userItemsBySlot: PartialDeep<UserItemsBySlot> = {
        [ItemSlot.Weapon0]: {
          id: 2,
          item: {
            type: ItemType.OneHandedWeapon,
          },
        },
        [ItemSlot.Weapon1]: {
          id: 3,
          item: {
            type: ItemType.Shield,
          },
        },
        [ItemSlot.Weapon2]: {
          id: 4,
          item: {
            type: ItemType.Bow,
          },
        },
        [ItemSlot.Weapon3]: {
          id: 5,
          item: {
            type: ItemType.Arrows,
          },
        },
      }

      const { onQuickEquip } = useInventoryQuickEquip(ref(userItemsBySlot as UserItemsBySlot))

      onQuickEquip(userItem as UserItem)

      expect(mockedGetAvailableSlotsByItem).toBeCalledWith(userItem.item, userItemsBySlot)
      expect(mockedUpdateCharacterItems).not.toBeCalled()
    })
  })

  describe('onQuickUnEquip', () => {
    it('should update items', () => {
      const userItemsBySlot: PartialDeep<UserItemsBySlot> = {
        [ItemSlot.Weapon0]: {
          id: 1,
          item: {
            type: ItemType.OneHandedWeapon,
          },
        },
      }

      const { onQuickUnEquip } = useInventoryQuickEquip(ref(userItemsBySlot as UserItemsBySlot))
      const { getUnEquipItemsLinked } = mockedUseInventoryEquipment()

      getUnEquipItemsLinked.mockReturnValueOnce([
        {
          slot: ItemSlot.Weapon0,
          userItemId: null,
        },
      ])

      onQuickUnEquip(ItemSlot.Weapon0)

      expect(getUnEquipItemsLinked).toBeCalledWith(ItemSlot.Weapon0, userItemsBySlot)
      expect(mockedUpdateCharacterItems).toBeCalledWith(1, [
        {
          slot: ItemSlot.Weapon0,
          userItemId: null,
        },
      ])
    })
  })
})

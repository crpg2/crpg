import type { PartialDeep } from 'type-fest'

import { afterEach, describe, expect, it, vi } from 'vitest'

import type { UserItem, UserItemsBySlot } from '~/models/user'

import { ITEM_SLOT, ITEM_TYPE } from '~/models/item'

import { useInventoryQuickEquip } from '../use-inventory-quick-equip'

const {
  mockedUseUser,
  mockedUseToastAdd,
  mockedGetUnEquipItems,
  mockedIsWeaponBySlot,
  mockedGetAvailableSlotsByItem,
  mockedUseCharacterItems,
  mockedOnUpdateCharacterItems,
  mockedEquippedItemsBySlot,
} = vi.hoisted(() => {
  const mockedOnUpdateCharacterItems = vi.fn()
  const mockedEquippedItemsBySlot = { value: {} }

  return {
    mockedUseUser: vi.fn().mockReturnValue({ user: { value: { id: 1 } } }),
    mockedUseToastAdd: vi.fn(),
    mockedGetUnEquipItems: vi.fn().mockReturnValue([]),
    mockedIsWeaponBySlot: vi.fn(),
    mockedGetAvailableSlotsByItem: vi.fn().mockReturnValue([]),
    mockedUseCharacterItems: vi.fn().mockReturnValue({
      onUpdateCharacterItems: mockedOnUpdateCharacterItems,
      equippedItemsBySlot: mockedEquippedItemsBySlot,
    }),
    mockedOnUpdateCharacterItems,
    mockedEquippedItemsBySlot,
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
  isWeaponBySlot: mockedIsWeaponBySlot,
}))

vi.mock('~/composables/user/use-user', () => ({
  useUser: mockedUseUser,
}))

vi.mock('~/composables/character/use-character-items', () => ({
  useCharacterItems: mockedUseCharacterItems,
}))

describe('useInventoryQuickEquip', () => {
  afterEach(() => {
    vi.spyOn(mockedEquippedItemsBySlot, 'value', 'get').mockReturnValue({})
  })

  describe('onQuickEquip', () => {
    it('should equip non-weapon item to first available slot', () => {
      mockedGetAvailableSlotsByItem.mockReturnValueOnce({ slots: [ITEM_SLOT.Body], warning: null })
      mockedIsWeaponBySlot.mockReturnValueOnce(false)

      const { onQuickEquip } = useInventoryQuickEquip()
      onQuickEquip({ id: 1, item: { type: ITEM_TYPE.BodyArmor } } as UserItem)

      expect(mockedOnUpdateCharacterItems).toHaveBeenCalledWith([{ slot: ITEM_SLOT.Body, userItemId: 1 }])
    })

    it('should replace non-weapon item if slot is already occupied', () => {
      mockedGetAvailableSlotsByItem.mockReturnValueOnce({ slots: [ITEM_SLOT.Body], warning: null })
      mockedIsWeaponBySlot.mockReturnValueOnce(false)
      const userItemsBySlot: PartialDeep<UserItemsBySlot> = {
        [ITEM_SLOT.Body]: { id: 2, item: { type: ITEM_TYPE.BodyArmor } },
      }
      vi.spyOn(mockedEquippedItemsBySlot, 'value', 'get').mockReturnValueOnce(userItemsBySlot)

      const { onQuickEquip } = useInventoryQuickEquip()
      onQuickEquip({ id: 1, item: { type: ITEM_TYPE.BodyArmor } } as UserItem)

      expect(mockedOnUpdateCharacterItems).toHaveBeenCalledWith([{ slot: ITEM_SLOT.Body, userItemId: 1 }])
    })

    it('should equip weapon to the first free weapon slot', () => {
      mockedGetAvailableSlotsByItem.mockReturnValueOnce({ slots: [ITEM_SLOT.Weapon0, ITEM_SLOT.Weapon1, ITEM_SLOT.Weapon2, ITEM_SLOT.Weapon3], warning: null })
      mockedIsWeaponBySlot.mockReturnValue(true)
      const userItemsBySlot: PartialDeep<UserItemsBySlot> = {
        [ITEM_SLOT.Weapon0]: { id: 2, item: { type: ITEM_TYPE.OneHandedWeapon } },
      }
      vi.spyOn(mockedEquippedItemsBySlot, 'value', 'get').mockReturnValue(userItemsBySlot)

      const { onQuickEquip } = useInventoryQuickEquip()
      onQuickEquip({ id: 1, item: { type: ITEM_TYPE.TwoHandedWeapon } } as UserItem)

      expect(mockedOnUpdateCharacterItems).toHaveBeenCalledWith([{ slot: ITEM_SLOT.Weapon1, userItemId: 1 }])
    })

    it('should not equip weapon when all weapon slots are occupied', () => {
      mockedGetAvailableSlotsByItem.mockReturnValueOnce({ slots: [ITEM_SLOT.Weapon0, ITEM_SLOT.Weapon1, ITEM_SLOT.Weapon2, ITEM_SLOT.Weapon3], warning: null })
      mockedIsWeaponBySlot.mockReturnValue(true)
      const userItemsBySlot: PartialDeep<UserItemsBySlot> = {
        [ITEM_SLOT.Weapon0]: { id: 2, item: { type: ITEM_TYPE.OneHandedWeapon } },
        [ITEM_SLOT.Weapon1]: { id: 3, item: { type: ITEM_TYPE.Shield } },
        [ITEM_SLOT.Weapon2]: { id: 4, item: { type: ITEM_TYPE.Bow } },
        [ITEM_SLOT.Weapon3]: { id: 5, item: { type: ITEM_TYPE.Arrows } },
      }
      vi.spyOn(mockedEquippedItemsBySlot, 'value', 'get').mockReturnValue(userItemsBySlot)

      const { onQuickEquip } = useInventoryQuickEquip()
      onQuickEquip({ id: 1, item: { type: ITEM_TYPE.TwoHandedWeapon } } as UserItem)

      expect(mockedOnUpdateCharacterItems).not.toHaveBeenCalled()
    })

    it('should show warning toast when getAvailableSlotsByItem returns warning', () => {
      mockedGetAvailableSlotsByItem.mockReturnValueOnce({ slots: [], warning: 'no_slots' })

      const { onQuickEquip } = useInventoryQuickEquip()
      onQuickEquip({ id: 1, item: { type: ITEM_TYPE.BodyArmor } } as UserItem)

      expect(mockedUseToastAdd).toHaveBeenCalledWith(expect.objectContaining({ title: 'no_slots' }))
      expect(mockedOnUpdateCharacterItems).not.toHaveBeenCalled()
    })

    it('should do nothing when available slots are empty', () => {
      mockedGetAvailableSlotsByItem.mockReturnValueOnce({ slots: [], warning: null })
      const { onQuickEquip } = useInventoryQuickEquip()
      const userItem: PartialDeep<UserItem> = { id: 1, item: { type: ITEM_TYPE.BodyArmor } }

      onQuickEquip(userItem as UserItem)

      expect(mockedOnUpdateCharacterItems).not.toHaveBeenCalled()
    })
  })

  describe('onQuickUnEquip', () => {
    it('should unequip item from occupied slot', () => {
      const userItemsBySlot: PartialDeep<UserItemsBySlot> = {
        [ITEM_SLOT.Weapon0]: { id: 1, item: { type: ITEM_TYPE.OneHandedWeapon } },
      }
      vi.spyOn(mockedEquippedItemsBySlot, 'value', 'get').mockReturnValue(userItemsBySlot)
      mockedGetUnEquipItems.mockReturnValueOnce([{ slot: ITEM_SLOT.Weapon0, userItemId: null }])

      const { onQuickUnEquip } = useInventoryQuickEquip()
      onQuickUnEquip(ITEM_SLOT.Weapon0)

      expect(mockedOnUpdateCharacterItems).toHaveBeenCalledWith([{ slot: ITEM_SLOT.Weapon0, userItemId: null }])
    })

    it('should do nothing when unequipping an empty slot', () => {
      vi.spyOn(mockedEquippedItemsBySlot, 'value', 'get').mockReturnValue({})
      mockedGetUnEquipItems.mockReturnValueOnce([])

      const { onQuickUnEquip } = useInventoryQuickEquip()
      onQuickUnEquip(ITEM_SLOT.Body)

      expect(mockedOnUpdateCharacterItems).not.toHaveBeenCalled()
    })

    it('should unequip multiple items if getUnEquipItems returns multiple slots', () => {
      const userItemsBySlot: PartialDeep<UserItemsBySlot> = {
        [ITEM_SLOT.Weapon0]: { id: 1, item: { type: ITEM_TYPE.OneHandedWeapon } },
        [ITEM_SLOT.Weapon1]: { id: 2, item: { type: ITEM_TYPE.Shield } },
      }
      vi.spyOn(mockedEquippedItemsBySlot, 'value', 'get').mockReturnValue(userItemsBySlot)
      mockedGetUnEquipItems.mockReturnValueOnce([
        { slot: ITEM_SLOT.Weapon0, userItemId: null },
        { slot: ITEM_SLOT.Weapon1, userItemId: null },
      ])

      const { onQuickUnEquip } = useInventoryQuickEquip()
      onQuickUnEquip(ITEM_SLOT.Weapon0)

      expect(mockedOnUpdateCharacterItems).toHaveBeenCalledWith([
        { slot: ITEM_SLOT.Weapon0, userItemId: null },
        { slot: ITEM_SLOT.Weapon1, userItemId: null },
      ])
    })
  })
})

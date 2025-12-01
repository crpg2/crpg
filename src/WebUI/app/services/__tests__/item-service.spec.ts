import type { PartialDeep } from 'type-fest'
import type { Mock } from 'vitest'

import { assertType, describe, expect, it, vi } from 'vitest'

import type { EquippedItemsBySlot } from '~/models/character'
import type { Item, ItemFlag, ItemFlat, ItemSlot, ItemWeaponComponent } from '~/models/item'
import type { UserItem } from '~/models/user'

import { CULTURE } from '~/models/culture'
import { ARMOR_MATERIAL_TYPE, DAMAGE_TYPE, ITEM_FAMILY_TYPE, ITEM_FLAG, ITEM_SLOT, ITEM_TYPE, ITEM_USAGE, WEAPON_CLASS, WEAPON_FLAG } from '~/models/item'

import * as itemService from '../item-service'

const {
  mockedGetItems,
  mockedGetItemsUpgradesByBaseId,
} = vi.hoisted(() => ({
  mockedGetItems: vi.fn(),
  mockedGetItemsUpgradesByBaseId: vi.fn(),
}))

vi.mock('#api/sdk.gen', () => ({
  getItems: mockedGetItems,
  getItemsUpgradesByBaseId: mockedGetItemsUpgradesByBaseId,
}))

vi.mock('#imports', () => ({
  useI18n: vi.fn(() => ({
    t: (key: string, params?: any) => params ? `${key}:${JSON.stringify(params)}` : key,
    n: (value: number) => `formatted:${value}`,
  })),
}))

describe('item service', () => {
  it('extractItem', () => {
    const userItem = { userId: 1, item: { id: 2 } as unknown as Item }
    const res = itemService.extractItem(userItem)
    expect(res).toEqual({ id: 2 })
    assertType<Item>(res)
  })

  it('getItemImage', () => {
    expect(itemService.getItemImage('crpg_aserai_noble_sword_2_t5')).toEqual(
      '/items/crpg_aserai_noble_sword_2_t5.webp',
    )
  })

  describe('isLargeShield', () => {
    it('returns true when the item is a shield and the first weapon class is LargeShield', () => {
      const item: Item = {
        type: ITEM_TYPE.Shield,
        weapons: [{ class: WEAPON_CLASS.LargeShield }],
      } as Item

      expect(itemService.isLargeShield(item)).toBe(true)
    })

    it('returns false when the item is not a shield', () => {
      const item: Item = {
        type: ITEM_TYPE.OneHandedWeapon,
        weapons: [{ class: WEAPON_CLASS.OneHandedSword }],
      } as Item

      expect(itemService.isLargeShield(item)).toBe(false)
    })
  })

  it.each<[string, PartialDeep<UserItem>, number, PartialDeep<EquippedItemsBySlot>, { slots: ItemSlot[], warning: string | null }]>([
    [
      'returns a warning when the item is broken',
      { isBroken: true } as UserItem,
      1,
      {},
      { slots: [], warning: 'character.inventory.item.broken.notify.warning' },
    ],
    [
      'returns a warning when the item is in the clan armory and belongs to the same user',
      { isArmoryItem: true, userId: 1 } as UserItem,
      1,
      {},
      { slots: [], warning: 'character.inventory.item.clanArmory.inArmory.notify.warning' },
    ],
    [
      'returns no slots when MountHarness family type is incompatible with the equipped Mount',
      {
        item: { type: ITEM_TYPE.MountHarness, armor: { familyType: ITEM_FAMILY_TYPE.Horse } },
      } as UserItem,
      1,
      {
        [ITEM_SLOT.Mount]: { item: { mount: { familyType: ITEM_FAMILY_TYPE.Camel } } },
      },
      { slots: [], warning: null },
    ],
    [
      'returns no slots when Mount family type is incompatible with equipped MountHarness',
      {
        item: { type: ITEM_TYPE.Mount, mount: { familyType: ITEM_FAMILY_TYPE.Camel } },
      } as UserItem,
      1,
      {
        [ITEM_SLOT.MountHarness]: { item: { armor: { familyType: ITEM_FAMILY_TYPE.Horse } } },
      },
      { slots: [], warning: null },
    ],
    [
      'returns WeaponExtra slot when item has DropOnWeaponChange flag (e.g. Pike)',
      {
        item: { flags: [ITEM_FLAG.DropOnWeaponChange] },
      } as UserItem,
      1,
      {},
      { slots: [ITEM_SLOT.WeaponExtra], warning: null },
    ],
    [
      'returns a horseback warning when a large shield is equipped while mounted',
      {
        item: { type: ITEM_TYPE.Shield, weapons: [{ class: WEAPON_CLASS.LargeShield }], flags: [] as ItemFlag[] },
      } as UserItem,
      1,
      {
        [ITEM_SLOT.Mount]: { item: { type: ITEM_TYPE.Mount } },
      },
      { slots: [], warning: 'character.inventory.item.cantUseOnHorseback.notify.warning' },
    ],
    [
      'returns a horseback warning when mounting while having a large shield equipped',
      {
        item: { type: ITEM_TYPE.Mount, flags: [] as ItemFlag[] },
      } as UserItem,
      1,
      {
        [ITEM_SLOT.Weapon0]: { item: { type: ITEM_TYPE.Shield, weapons: [{ class: WEAPON_CLASS.LargeShield }] as ItemWeaponComponent[] } },
      },
      { slots: [], warning: 'character.inventory.item.cantUseOnHorseback.notify.warning' },
    ],
    [
      'returns an EBA armor compatibility warning when EBA BodyArmor and non-EBA LegArmor are mixed',
      {
        item: { type: ITEM_TYPE.BodyArmor, armor: { familyType: ITEM_FAMILY_TYPE.EBA }, flags: [] as ItemFlag[] },
      } as UserItem,
      1,
      {
        [ITEM_SLOT.Leg]: { item: { armor: { } } },
      },
      { slots: [], warning: 'character.inventory.item.EBAArmorCompatible.notify.warning' },
    ],
    [
      'returns an EBA armor compatibility warning when EBA LegArmor is used with non-EBA BodyArmor',
      {
        item: { type: ITEM_TYPE.LegArmor, armor: { }, flags: [] as ItemFlag[] },
      } as UserItem,
      1,
      {
        [ITEM_SLOT.Body]: { item: { armor: { familyType: ITEM_FAMILY_TYPE.EBA } } },
      },
      { slots: [], warning: 'character.inventory.item.EBAArmorCompatible.notify.warning' },
    ],
    [
      'returns default slots from itemSlotsByType when no special conditions are met',
      {
        item: { type: ITEM_TYPE.HeadArmor, flags: [] as ItemFlag[] },
      } as UserItem,
      1,
      {},
      { slots: [ITEM_SLOT.Head], warning: null },
    ],
  ])('getAvailableSlotsByItem - %s', (_, userItem, userId, equipedItems, expectation) => {
    expect(itemService.getAvailableSlotsByItem(userItem as UserItem, userId, equipedItems as EquippedItemsBySlot)).toEqual(expectation)
  })

  it.each<[string, ItemSlot, PartialDeep<EquippedItemsBySlot>, ItemSlot[]]>([
    [
      'returns Body slot when Leg slot is selected and equipped BodyArmor is EBA type',
      ITEM_SLOT.Leg,
      {
        [ITEM_SLOT.Body]: { item: { armor: { familyType: ITEM_FAMILY_TYPE.EBA } } },
      },
      [ITEM_SLOT.Body],
    ],
    [
      'returns empty array when Leg slot is selected but equipped BodyArmor is not EBA type',
      ITEM_SLOT.Leg,
      {
        [ITEM_SLOT.Body]: { item: { armor: { } } },
      },
      [],
    ],
    [
      'returns empty array when Leg slot is selected but Body slot is not equipped',
      ITEM_SLOT.Leg,
      {},
      [],
    ],
    [
      'returns empty array when any slot other than Leg is selected',
      ITEM_SLOT.Body,
      {
        [ITEM_SLOT.Body]: { item: { armor: { familyType: ITEM_FAMILY_TYPE.EBA } } },
      },
      [],
    ],
  ])('getLinkedSlots - %s', (_, slot, equippedItems, expectation) => {
    expect(itemService.getLinkedSlots(slot, equippedItems as EquippedItemsBySlot)).toEqual(expectation)
  })

  it.each<[string, ItemSlot, PartialDeep<EquippedItemsBySlot>, ReturnType<typeof itemService.getUnEquipItems>]>([
    [
      'returns only the provided slot when there are no linked slots',
      ITEM_SLOT.Body,
      {},
      [{ slot: ITEM_SLOT.Body, userItemId: null }],
    ],
    [
      'returns provided slot and linked slots when linked slots exist',
      ITEM_SLOT.Leg,
      {
        [ITEM_SLOT.Body]: { item: { armor: { familyType: ITEM_FAMILY_TYPE.EBA } } },
      },
      [
        { slot: ITEM_SLOT.Leg, userItemId: null },
        { slot: ITEM_SLOT.Body, userItemId: null },
      ],
    ],
  ])('getUnEquipItems - %s', async (_, slot, equippedItemsBySlot, expectation) => {
    let spy: Mock | null = null
    if (slot === ITEM_SLOT.Leg) {
      spy = vi.spyOn(itemService, 'getLinkedSlots').mockReturnValueOnce([ITEM_SLOT.Body])
    }
    else {
      spy = vi.spyOn(itemService, 'getLinkedSlots').mockReturnValueOnce([])
    }

    expect(itemService.getUnEquipItems(slot, equippedItemsBySlot as EquippedItemsBySlot)).toEqual(expectation)
    spy?.mockRestore()
  })

  it.each<[string, keyof ItemFlat, any, ItemFlat | undefined, ReturnType<typeof itemService.humanizeBucket>]>([
    [
      'returns empty bucket when bucket is null',
      'type',
      null,
      undefined,
      { label: '', icon: null, tooltip: null },
    ],
    [
      'handles type aggregationKey',
      'type',
      ITEM_TYPE.Bolts,
      undefined,
      { label: 'item.type.Bolts', icon: 'item-type-bolt', tooltip: { description: '', title: 'item.type.Bolts' } },
    ],
    [
      'handles weaponClass aggregationKey',
      'weaponClass',
      WEAPON_CLASS.TwoHandedSword,
      undefined,
      { label: 'item.weaponClass.TwoHandedSword', icon: 'weapon-class-two-handed-sword', tooltip: { description: '', title: 'item.weaponClass.TwoHandedSword' } },
    ],
    [
      'handles damageType aggregationKey',
      'damageType',
      DAMAGE_TYPE.Cut,
      undefined,
      { label: 'item.damageType.Cut.long', icon: 'damage-type-cut', tooltip: { description: 'item.damageType.Cut.description', title: 'item.damageType.Cut.title' } },
    ],
    [
      'handles culture aggregationKey',
      'culture',
      CULTURE.Empire,
      undefined,
      { label: 'item.culture.Empire', icon: 'culture-empire', tooltip: { description: null, title: 'item.culture.Empire' } },
    ],
    [
      'handles family type keys',
      'armorFamilyType',
      ITEM_FAMILY_TYPE.EBA,
      undefined,
      { label: 'item.familyType.3.title', icon: null, tooltip: { description: 'item.familyType.3.description', title: 'item.familyType.3.title' } },
    ],
    [
      'handles armorMaterialType',
      'armorMaterialType',
      ARMOR_MATERIAL_TYPE.Chainmail,
      undefined,
      { label: 'item.armorMaterialType.Chainmail.title', icon: null, tooltip: null },
    ],
    [
      'handles item flag',
      'flags',
      ITEM_FLAG.UseTeamColor,
      undefined,
      { label: 'item.flags.UseTeamColor', icon: 'item-flag-use-team-color', tooltip: { description: null, title: 'item.flags.UseTeamColor' } },
    ],
    [
      'handles weapon flag',
      'flags',
      WEAPON_FLAG.CanKnockDown,
      undefined,
      { label: 'item.weaponFlags.CanKnockDown', icon: 'item-flag-can-knock-down', tooltip: { description: null, title: 'item.weaponFlags.CanKnockDown' } },
    ],
    [
      'handles item usage',
      'flags',
      ITEM_USAGE.CrossbowLight,
      undefined,
      { label: 'item.usage.crossbow_light.title', icon: 'item-flag-light-crossbow', tooltip: { description: 'item.usage.crossbow_light.description', title: 'item.usage.crossbow_light.title' } },
    ],
    [
      'handles damage format aggregation',
      'damage',
      100,
      { thrustDamageType: DAMAGE_TYPE.Pierce } as ItemFlat,
      { label: 'item.damageTypeFormat:{"type":"item.damageType.Pierce.short","value":100}', icon: null, tooltip: { description: 'item.damageType.Pierce.description', title: 'item.damageType.Pierce.title' } },
    ],
    [
      'handles requirement format aggregation',
      'requirement',
      10,
      undefined,
      { label: 'item.requirementFormat:{"value":10}', icon: null, tooltip: null },
    ],
    [
      'handles number format aggregation',
      'weight',
      50,
      undefined,
      { label: 'formatted:50', icon: null, tooltip: null },
    ],
    [
      'returns default string when no format or key matched',
      'name',
      'some',
      undefined,
      { label: 'some', icon: null, tooltip: null },
    ],
  ])('humanizeBucket - %s', async (_, aggregationKey, bucket, item, expectation) => {
    expect(itemService.humanizeBucket(aggregationKey, bucket, item)).toEqual(expectation)
  })
})

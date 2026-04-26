import { afterEach, beforeEach, describe, expect, expectTypeOf, it, vi } from 'vitest'

import type { Item, ItemFlat } from '~/models/item'

import { ITEM_TYPE, WEAPON_CLASS, WEAPON_FLAG, WEAPON_USAGE } from '~/models/item'

import mockConstants from '../../../../mocks/constants.json'
import { createItemIndex } from '../indexator'
import * as itemMocks from './mocks'

vi.mock('~root/data/constants.json', vi.fn().mockImplementation(() => mockConstants))

const { mockedComputeAverageRepairCostPerHour, mockedIsLargeShield } = vi.hoisted(() => ({
  mockedComputeAverageRepairCostPerHour: vi.fn((price: number) => price * 2),
  mockedIsLargeShield: vi.fn(),
}))

vi.mock('#api/sdk.gen')

vi.mock('~/services/item-service', () => ({
  computeAverageRepairCostPerHour: mockedComputeAverageRepairCostPerHour,
  isLargeShield: mockedIsLargeShield,
  itemIsNewDays: 30,
}))

describe('createItemIndex', () => {
  beforeEach(() => {
    vi.useFakeTimers()
    vi.setSystemTime('2023-07-13T21:43:44.0741909Z')
  })

  afterEach(() => {
    vi.useRealTimers()
  })

  it('weapons > 1, same weaponClass', () => {
    const index = createItemIndex([itemMocks.Pike], { cloneMultipleUsageWeapon: true })

    expect(index.length).toEqual(1)
    const [item] = index

    expect(item?.modId).toEqual('crpg_vlandia_pike_1_t5_Polearm_TwoHandedPolearm')

    // merge weaponUsage + weaponFlags + flags => flags
    expect(item?.flags).toContain('polearm_pike')
    expect(item?.flags).toContain('polearm_bracing')
    expect(item?.weaponUsage).toEqual([WEAPON_USAGE.Primary])
    expect(item?.swingDamageType).toEqual(undefined)
  })

  it('weapons > 1, diff weaponClass - it is necessary to clone the item', () => {
    const index = createItemIndex([itemMocks.Hoe], { cloneMultipleUsageWeapon: true })

    expect(index.length).toEqual(2)
    const [item1, item2] = index

    expect(item1?.modId).toEqual('crpg_peasant_2haxe_1_t1_OneHandedWeapon_OneHandedAxe')
    expect(item1?.type).toEqual('OneHandedWeapon')
    expect(item1?.weaponClass).toEqual('OneHandedAxe')
    expect(item1?.weaponUsage).toEqual([WEAPON_USAGE.Secondary])
    expect(item1?.thrustDamageType).toEqual(undefined)

    expect(item2?.modId).toEqual('crpg_peasant_2haxe_1_t1_TwoHandedWeapon_TwoHandedAxe')
    expect(item2?.type).toEqual('TwoHandedWeapon')
    expect(item2?.weaponClass).toEqual('TwoHandedAxe')
    expect(item2?.weaponUsage).toEqual([WEAPON_USAGE.Primary])
    expect(item2?.thrustDamageType).toEqual(undefined)
  })

  it('weapons > 1, diff weaponClass with cloneMultipleUsageWeapon=false keeps only primary usage', () => {
    const index = createItemIndex([itemMocks.Hoe], { cloneMultipleUsageWeapon: false })

    expect(index.length).toEqual(1)
    const [item] = index

    expect(item?.type).toEqual('TwoHandedWeapon')
    expect(item?.weaponClass).toEqual('TwoHandedAxe')
    expect(item?.weaponUsage).toEqual([WEAPON_USAGE.Primary])
  })

  it('shield', () => {
    mockedIsLargeShield.mockReturnValueOnce(true)
    const index = createItemIndex([itemMocks.Shield])
    const [item] = index

    expect(item?.shieldDurability).toEqual(70)
    expect(item?.shieldSpeed).toEqual(82)
    expect(item?.shieldArmor).toEqual(0)
    expect(item?.weaponClass).toEqual('LargeShield')
    expect(item?.weaponPrimaryClass).toEqual('LargeShield')
    expect(item?.flags).toContain('CantUseOnHorseback')
  })

  it('shield - CantUseOnHorseback', () => {
    mockedIsLargeShield.mockReturnValueOnce(false)
    const index = createItemIndex([itemMocks.Shield])
    const [item] = index

    expect(item?.flags).not.toContain('CantUseOnHorseback')
  })

  it('shield - upkeep', () => {
    mockedIsLargeShield.mockReturnValueOnce(true)
    const index = createItemIndex([itemMocks.Shield])
    const [item] = index

    expect(item?.upkeep).toEqual(item!.price * 2)
  })

  it('bow', () => {
    const index = createItemIndex([itemMocks.Bow])
    const [item] = index

    expect(item?.reloadSpeed).toEqual(98)
    expect(item?.aimSpeed).toEqual(100)
    expect(item?.damage).toEqual(14)
  })

  it('bolts', () => {
    const index = createItemIndex([itemMocks.Bolts])
    const [item] = index

    expect(item?.damage).toEqual(14)
    expect(item?.damageType).toEqual('Pierce')
    expect(item?.weight).toEqual(null)
    expect(item?.stackWeight).toEqual(1.5)
  })

  it('throwing axe', () => {
    const index = createItemIndex([itemMocks.ThrowingAxe], { cloneMultipleUsageWeapon: true })
    const [item1, item2] = index

    expect(item1?.type).toEqual('OneHandedWeapon')
    expect(item1?.weaponClass).toEqual('OneHandedAxe')

    expect(item2?.type).toEqual('Thrown')
    expect(item2?.weaponClass).toEqual('ThrowingAxe')

    expect(item2?.damage).toEqual(32)
    expect(item2?.damageType).toEqual(undefined)
    expect(item2?.weight).toEqual(null)
    expect(item2?.stackWeight).toEqual(3.75)
  })

  it('mount harness', () => {
    const index = createItemIndex([itemMocks.MountHarness])
    const [item] = index

    expect(item?.mountArmor).toEqual(6)
    expect(item?.mountArmorFamilyType).toEqual(1)
  })

  it('mount', () => {
    const index = createItemIndex([itemMocks.Mount])
    const [item] = index

    expect(item?.bodyLength).toEqual(105)
    expect(item?.chargeDamage).toEqual(1)
    expect(item?.maneuver).toEqual(66)
    expect(item?.speed).toEqual(40)
    expect(item?.hitPoints).toEqual(172)
    expect(item?.mountFamilyType).toEqual(1)
  })

  it('helmet', () => {
    const index = createItemIndex([itemMocks.Helmet])
    const [item] = index

    expect(item?.mountArmor).toEqual(null)
    expect(item?.headArmor).toEqual(64)
    expect(item?.modId).toEqual('crpg_sa_1ChurburghHelm_HeadArmor')
  })

  it('isNew', () => {
    const index = createItemIndex([itemMocks.Helmet, itemMocks.Longsword])
    const [item1, item2] = index

    expect(item1?.isNew).toBeTruthy()
    expect(item2?.isNew).toBeFalsy()
  })

  it('does not mutate input weapon flags', () => {
    const shield = structuredClone(itemMocks.Shield)
    const initialFlags = [...shield.weapons[0]!.flags]

    mockedIsLargeShield.mockReturnValueOnce(true)
    createItemIndex([shield])

    expect(shield.weapons[0]!.flags).toEqual(initialFlags)
    expect(shield.weapons[0]!.flags).not.toContain('CantUseOnHorseback')
  })

  it('crossbow adds CanReloadOnHorseback only when allowed', () => {
    const crossbow: Item = {
      ...itemMocks.Bow,
      type: ITEM_TYPE.Crossbow,
      weapons: [{
        ...itemMocks.Bow.weapons[0]!,
        class: WEAPON_CLASS.Crossbow,
        flags: ['RangedWeapon'],
        itemUsage: 'crossbow',
      }],
    }

    const index1 = createItemIndex([crossbow])
    expect(index1[0]?.flags).toContain(WEAPON_FLAG.CanReloadOnHorseback)

    const crossbowCantReload: Item = {
      ...crossbow,
      weapons: [{
        ...crossbow.weapons[0]!,
        flags: ['RangedWeapon', WEAPON_FLAG.CantReloadOnHorseback],
      }],
    }

    const index2 = createItemIndex([crossbowCantReload])
    expect(index2[0]?.flags).toContain(WEAPON_FLAG.CantReloadOnHorseback)
    expect(index2[0]?.flags).not.toContain(WEAPON_FLAG.CanReloadOnHorseback)
  })

  it('flags are deduplicated', () => {
    const shieldWithFlag: Item = {
      ...itemMocks.Shield,
      weapons: [{
        ...itemMocks.Shield.weapons[0]!,
        flags: [...itemMocks.Shield.weapons[0]!.flags, WEAPON_FLAG.CantUseOnHorseback],
      }],
    }

    mockedIsLargeShield.mockReturnValueOnce(true)
    const index = createItemIndex([shieldWithFlag])
    const cantUseOnHorseback = index[0]?.flags.filter(f => f === WEAPON_FLAG.CantUseOnHorseback)

    expect(cantUseOnHorseback?.length).toEqual(1)
  })

  it('writeMeta maps wrapper data for each item', () => {
    const wrappedItems = [
      { item: itemMocks.Helmet, marker: 'h' },
      { item: itemMocks.Longsword, marker: 'l' },
    ]

    const index = createItemIndex(wrappedItems, {
      writeMeta: wrapper => ({ marker: wrapper.marker }),
    })

    expect(index[0]?.meta).toEqual({ marker: 'h' })
    expect(index[1]?.meta).toEqual({ marker: 'l' })
  })

  it('writeMeta infers output types', () => {
    const wrappedItems = [
      { item: itemMocks.Helmet, marker: 'h' },
      { item: itemMocks.Longsword, marker: 'l' },
    ]

    const index = createItemIndex(wrappedItems, {
      writeMeta: wrapper => ({ marker: wrapper.marker, itemId: wrapper.item.id }),
    })

    expectTypeOf(index).toEqualTypeOf<ItemFlat<{ marker: string, itemId: string }>[]>()
    expectTypeOf(index[0]!.meta).toEqualTypeOf<{ marker: string, itemId: string }>()
  })
})

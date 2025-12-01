import type { FilterFnOption } from '@tanstack/vue-table'

import { describe, expect, it, vi } from 'vitest'

import type { ItemFlat, ItemType, WeaponClass } from '~/models/item'

import { ITEM_TYPE, WEAPON_CLASS } from '~/models/item'
import {
  getAggregationsConfig,
  getBuckets,
  getColumnVisibility,
  getFacetsByItemType,
  getFacetsByWeaponClass,
  getFilterFn,
  // getItemAggregations, // TODO: FIXME: SPEC
} from '~/services/item-search-service'

import type { AggregationConfig, AggregationOptions } from '../aggregations'

const { mockedAggregationsConfig, mockGetWeaponClassesByItemType } = vi.hoisted(
  () => ({
    mockedAggregationsConfig: {
      price: {
        view: 'Range',
      },
      thrustDamage: {
        view: 'Range',
      },
      tier: {
        view: 'Range',
        hidden: true,
      },
      type: {
        view: 'Checkbox',
        hidden: true,
      },
      weaponClass: {
        view: 'Checkbox',
      },
    } satisfies AggregationConfig,
    mockGetWeaponClassesByItemType: vi.fn(),
  }),
)

vi.mock('~/services/item-service', () => ({
  getWeaponClassesByItemType: mockGetWeaponClassesByItemType,
}))

vi.mock(
  import('~/services/item-search-service/aggregations'),
  async importOriginal => ({
    ...(await importOriginal()),
    aggregationsConfig: mockedAggregationsConfig,
    aggregationsKeysByItemType: {
      [ITEM_TYPE.Banner]: ['price'],
      [ITEM_TYPE.OneHandedWeapon]: ['thrustDamage'],
    } as Partial<Record<ItemType, Array<keyof ItemFlat>>>,
    aggregationsKeysByWeaponClass: {
      [WEAPON_CLASS.OneHandedSword]: ['price'],
    } as Partial<Record<WeaponClass, Array<keyof ItemFlat>>>,
  }),
)

it.each<[ItemType, WeaponClass | null, Array<keyof ItemFlat>, Array<keyof ItemFlat>]>([
  [ITEM_TYPE.Banner, null, [], ['type', 'weaponClass', 'modId', 'isNew', 'price']],
  [ITEM_TYPE.OneHandedWeapon, WEAPON_CLASS.OneHandedSword, [], ['type', 'weaponClass', 'modId', 'isNew', 'price']],
  [ITEM_TYPE.OneHandedWeapon, WEAPON_CLASS.OneHandedAxe, [], ['type', 'weaponClass', 'modId', 'isNew', 'thrustDamage']],
  [ITEM_TYPE.OneHandedWeapon, WEAPON_CLASS.OneHandedAxe, ['type'], ['weaponClass', 'modId', 'isNew', 'thrustDamage']],
])(
  'getAggregationsConfig - itemType: %s, weaponClass: %s',
  (itemType, weaponClass, excludeKeys, expectation) => {
    expect(Object.keys(getAggregationsConfig(itemType, weaponClass, excludeKeys))).toEqual(expectation)
  },
)

it('getColumnVisibility', () => {
  expect(getColumnVisibility(mockedAggregationsConfig)).toEqual(
    {
      tier: false,
      type: false,
    },
  )
})

it.each<[AggregationOptions, FilterFnOption<any>]>([
  [{ view: 'Toggle', format: 'String' }, 'equalsString'],
  [{ view: 'Toggle' }, 'auto'],
  [{ view: 'Range' }, 'inNumberRange'],
  [{ view: 'Checkbox', format: 'List' }, 'arrIncludesSome'],
  [{ view: 'Checkbox' }, 'includesSome'],
])(
  'getFilterFn %s',
  (options, expectation) => {
    expect(getFilterFn(options)).toEqual(expectation)
  },
)

describe('getBuckets', () => {
  it('getBuckets returns empty object for empty Map', () => {
    expect(getBuckets(new Map())).toEqual({})
  })

  it('getBuckets aggregates counts for single values', () => {
    const facets = new Map<string | number, number>([
      ['foo', 2],
      ['bar', 3],
    ])
    expect(getBuckets(facets)).toEqual({ foo: 2, bar: 3 })
  })

  it('getBuckets aggregates counts for array buckets', () => {
    const facets = new Map<any, number>([
      [['a', 'b'], 5],
      ['c', 1],
    ])
    expect(getBuckets(facets)).toEqual({ a: 5, b: 5, c: 1 })
  })

  it('getBuckets skips null and undefined buckets', () => {
    const facets = new Map<any, number>([
      [null, 10],
      [undefined, 20],
      ['valid', 1],
    ])
    expect(getBuckets(facets)).toEqual({ valid: 1 })
  })

  it('getBuckets aggregates counts for duplicate items', () => {
    const facets = new Map<any, number>([
      ['x', 2],
      [['x', 'y'], 3],
      ['y', 1],
    ])
    expect(getBuckets(facets)).toEqual({ x: 5, y: 4 })
  })
})

it('getFacetsByItemType returns sorted unique item types', () => {
  const input: ItemType[] = [
    ITEM_TYPE.OneHandedWeapon,
    ITEM_TYPE.Banner,
    ITEM_TYPE.Banner,
    ITEM_TYPE.TwoHandedWeapon,
  ]

  const orders = new Map([
    [ITEM_TYPE.OneHandedWeapon, 0],
    [ITEM_TYPE.TwoHandedWeapon, 1],
    [ITEM_TYPE.Banner, 2],
  ])

  expect(getFacetsByItemType(input, orders)).toEqual([
    ITEM_TYPE.OneHandedWeapon,
    ITEM_TYPE.TwoHandedWeapon,
    ITEM_TYPE.Banner,
  ])
})

describe('getFacetsByWeaponClass', () => {
  const orders = new Map([
    [WEAPON_CLASS.OneHandedSword, 0],
    [WEAPON_CLASS.OneHandedAxe, 1],
    [WEAPON_CLASS.TwoHandedSword, 2],
  ])

  it('returns sorted and filtered weapon classes', () => {
    mockGetWeaponClassesByItemType.mockReturnValue([
      WEAPON_CLASS.OneHandedSword,
      WEAPON_CLASS.OneHandedAxe,
      WEAPON_CLASS.TwoHandedSword,
    ])
    const input = [WEAPON_CLASS.OneHandedAxe, WEAPON_CLASS.TwoHandedSword]
    const result = getFacetsByWeaponClass(input, ITEM_TYPE.OneHandedWeapon, orders)
    expect(result).toEqual([WEAPON_CLASS.OneHandedAxe, WEAPON_CLASS.TwoHandedSword])
  })
})

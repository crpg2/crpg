import { expect, it, vi } from 'vitest'

import type { ItemFlat, ItemType, WeaponClass } from '~/models/item'

import { ITEM_TYPE, WEAPON_CLASS } from '~/models/item'
import {
  getAggregationsConfig,
  // getFacetsByItemType, // TODO: FIXME:
  // getFacetsByWeaponClass, // TODO: FIXME:
  getVisibleAggregationsConfig,
} from '~/services/item-search-service'

import type { AggregationConfig } from '../aggregations'

const { mockedAggregationsConfig } = vi.hoisted(
  () => ({
    mockedAggregationsConfig: {
      price: {
        title: 'Price',
        view: 'Range',
      },
      thrustDamage: {
        title: 'Thrust damage',
        view: 'Range',
      },
      tier: {
        hidden: true,
        title: 'Tier',
        view: 'Range',
      },
      type: {
        hidden: true,
        title: 'Type',
        view: 'Checkbox',
      },
      weaponClass: {
        title: 'Weapon class',
        view: 'Checkbox',
      },
    } as AggregationConfig,
  }),
)

vi.mock('~/services/item-search-service/aggregations.ts', () => ({
  aggregationsConfig: mockedAggregationsConfig,
  aggregationsKeysByItemType: {
    [ITEM_TYPE.Banner]: ['price'],
    [ITEM_TYPE.OneHandedWeapon]: ['thrustDamage'],
  } as Partial<Record<ItemType, Array<keyof ItemFlat>>>,
  aggregationsKeysByWeaponClass: {
    [WEAPON_CLASS.OneHandedSword]: ['price'],
  } as Partial<Record<WeaponClass, Array<keyof ItemFlat>>>,
}))

it.each<[ItemType, WeaponClass | null, string[]]>([
  [ITEM_TYPE.Banner, null, ['modId', 'new', 'price']],
  [ITEM_TYPE.OneHandedWeapon, WEAPON_CLASS.OneHandedSword, ['modId', 'new', 'price']],
  [ITEM_TYPE.OneHandedWeapon, WEAPON_CLASS.OneHandedAxe, ['modId', 'new', 'thrustDamage']],
])(
  'getAggregationsConfig - itemType: %s, weaponClass: %s',
  (itemType, weaponClass, expectation) => {
    expect(Object.keys(getAggregationsConfig(itemType, weaponClass))).toEqual(expectation)
  },
)

it('getVisibleAggregationsConfig', () => {
  expect(getVisibleAggregationsConfig(mockedAggregationsConfig)).not.toContain(['tier', 'type'])
})

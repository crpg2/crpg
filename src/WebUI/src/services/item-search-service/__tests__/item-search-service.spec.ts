import type { ItemFlat } from '~/models/item'
import type { AggregationConfig, Buckets } from '~/models/item-search'

import { ItemType, WeaponClass } from '~/models/item'
import { AggregationView } from '~/models/item-search'
import {
  filterItemsByType,
  filterItemsByWeaponClass,
  generateEmptyFiltersModel,
  getAggregationBy,
  getAggregationsConfig,
  getBucketValues,
  getMaxRange,
  getMinRange,
  getScopeAggregations,
  getSearchResult,
  getSortingConfig,
  getStepRange,
  getVisibleAggregationsConfig,
} from '~/services/item-search-service'

const { mockedAggregationsConfig, mockedItemsJSAggregation, mockedItemsJSSearch } = vi.hoisted(
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
    mockedItemsJSAggregation: vi.fn(),
    mockedItemsJSSearch: vi.fn(),
  }),
)

vi.mock('itemsjs', () => ({
  default: vi.fn().mockImplementation(() => ({
    aggregation: mockedItemsJSAggregation,
    search: mockedItemsJSSearch,
  })),
}))

vi.mock('~/services/item-search-service/helpers.ts', () => ({
  applyRangeFilters: vi.fn(),
  excludeRangeFilters: vi.fn(val => val),
}))

vi.mock('~/services/item-search-service/aggregations.ts', () => ({
  aggregationsConfig: mockedAggregationsConfig,
  aggregationsKeysByItemType: {
    [ItemType.Banner]: ['price'],
    [ItemType.OneHandedWeapon]: ['thrustDamage'],
  } as Partial<Record<ItemType, Array<keyof ItemFlat>>>,
  aggregationsKeysByWeaponClass: {
    [WeaponClass.OneHandedSword]: ['price'],
  } as Partial<Record<WeaponClass, Array<keyof ItemFlat>>>,
}))

it.each([
  [[{ key: 'null' }], []],
  [[{ key: '1' }], [1]],
  [[{ key: 'null' }, { key: '123' }], [123]],
])('getBucketValues - buckets: %j', (buckets, expectation) => {
  expect(getBucketValues(buckets as Buckets)).toEqual(expectation)
})

it.each([
  [[], 0],
  [[1, 2, 3], 1],
  [[1.1], 1],
  [[1.1, 1.05], 1],
  [[1.2, 1.001], 1],
  [[2.2, 100.001], 2],
])('getMinRange - values: %j', (values, expectation) => {
  expect(getMinRange(values)).toEqual(expectation)
})

it.each([
  [[], 0],
  [[1, 2, 3], 3],
  [[1.1], 2],
  [[1.1, 1.05], 2],
  [[1.2, 1.001], 2],
  [[2.2, 100.001], 101],
])('getMaxRange - values: %j', (values, expectation) => {
  expect(getMaxRange(values)).toEqual(expectation)
})

it('generateFiltersModel', () => {
  const aggregations = {
    price: {
      title: 'price',
      view: AggregationView.Range,
    },
    type: {
      title: 'type',
      view: AggregationView.Checkbox,
    },
  }

  expect(generateEmptyFiltersModel(aggregations)).toEqual({ price: [], type: [] })
})

it.each<[ItemType, WeaponClass | null, string[]]>([
  [ItemType.Banner, null, ['modId', 'new', 'price']],
  [ItemType.OneHandedWeapon, WeaponClass.OneHandedSword, ['modId', 'new', 'price']],
  [ItemType.OneHandedWeapon, WeaponClass.OneHandedAxe, ['modId', 'new', 'thrustDamage']],
])(
  'getAggregationsConfig - itemType: %s, weaponClass: %s',
  (itemType, weaponClass, expectation) => {
    expect(Object.keys(getAggregationsConfig(itemType, weaponClass))).toEqual(expectation)
  },
)

it('getVisibleAggregationsConfig', () => {
  expect(getVisibleAggregationsConfig(mockedAggregationsConfig)).not.toContain(['tier', 'type'])
})

it('getSortingConfig', () => {
  const aggregations = {
    price: {
      title: 'price',
      view: AggregationView.Range,
    },
    type: {
      title: 'type',
      view: AggregationView.Checkbox,
    },
  }

  expect(Object.keys(getSortingConfig(aggregations))).toEqual(['price_asc', 'price_desc'])
})

it.each<[number[], number]>([
  [[1, 2, 3], 1],
  [[1.5, 1.6, 0.8, 1.12, 1.2], 0.1],
  [[120, 130, 30, 125, 135, 145, 20, 21, 22, 22.5, 23], 1],
  [
    [
      0.1,
      0.2,
      0.3,
      0.4,
      0.5,
      0.6,
      0.7,
      0.8,
      0.9,
      1,
      1.1,
      1.2,
      1.3,
      1.4,
      1.5,
      1.6,
      1.7,
      1.8,
      1.9,
      2,
      2.1,
    ],
    0.1,
  ],
])('getStepRange - values: %j', (values, expectation) => {
  expect(getStepRange(values)).toEqual(expectation)
})

describe('filterItemsByType ', () => {
  it('polearm', () => {
    expect(
      filterItemsByType(
        [
          { type: 'TwoHandedWeapon' },
          {
            type: 'Polearm',
          },
          {
            type: 'Bow',
          },
        ] as ItemFlat[],
        ItemType.Polearm,
      ),
    ).toEqual([
      {
        type: 'Polearm',
      },
    ])
  })

  it('undefinded', () => {
    expect(
      filterItemsByType(
        [
          { type: 'TwoHandedWeapon' },
          {
            type: 'Polearm',
          },
          {
            type: 'Bow',
          },
        ] as ItemFlat[],
        ItemType.Undefined,
      ),
    ).toEqual([
      { type: 'TwoHandedWeapon' },
      {
        type: 'Polearm',
      },
      {
        type: 'Bow',
      },
    ])
  })
})

describe('filterItemsByWeaponClass ', () => {
  it('twoHandedSword', () => {
    expect(
      filterItemsByWeaponClass(
        [
          { weaponClass: 'OneHandedAxe' },
          {
            weaponClass: 'OneHandedPolearm',
          },
          {
            weaponClass: 'TwoHandedSword',
          },
        ] as ItemFlat[],
        WeaponClass.TwoHandedSword,
      ),
    ).toEqual([
      {
        weaponClass: 'TwoHandedSword',
      },
    ])
  })

  it('empty weapon class filter condition', () => {
    expect(
      filterItemsByWeaponClass(
        [
          { weaponClass: 'OneHandedAxe' },
          {
            weaponClass: 'OneHandedPolearm',
          },
        ] as ItemFlat[],
        null,
      ),
    ).toEqual([
      { weaponClass: 'OneHandedAxe' },
      {
        weaponClass: 'OneHandedPolearm',
      },
    ])
  })
})

describe('getAggregationBy ', () => {
  it('item type - the buckets must be sorted', () => {
    mockedItemsJSAggregation.mockReturnValue({
      data: {
        buckets: [
          {
            key: 'Arrows',
          },
          {
            key: 'OneHandedWeapon',
          },
        ],
      },
    })

    expect(getAggregationBy([], 'type')).toEqual({
      data: {
        buckets: [
          {
            key: 'OneHandedWeapon',
          },
          {
            key: 'Arrows',
          },
        ],
      },
    })
  })

  it('weapon class - the buckets must be sorted', () => {
    mockedItemsJSAggregation.mockReturnValue({
      data: {
        buckets: [
          {
            key: 'Dagger',
          },
          {
            key: 'OneHandedAxe',
          },
        ],
      },
    })

    expect(getAggregationBy([], 'weaponClass')).toEqual({
      data: {
        buckets: [
          {
            key: 'OneHandedAxe',
          },
          {
            key: 'Dagger',
          },
        ],
      },
    })
  })

  it('handling - no custom buckets sorting', () => {
    mockedItemsJSAggregation.mockReturnValue({
      data: {
        buckets: [
          {
            key: '10',
          },
          {
            key: '20',
          },
        ],
      },
    })

    expect(getAggregationBy([], 'handling')).toEqual({
      data: {
        buckets: [
          {
            key: '10',
          },
          {
            key: '20',
          },
        ],
      },
    })
  })
})

it('getScopeAggregations', () => {
  mockedItemsJSSearch.mockReturnValue({
    data: {
      aggregations: {
        handling: {},
        price: {},
      },
    },
  })

  const result = getScopeAggregations([], {})

  expect(mockedItemsJSSearch).toHaveBeenCalledWith({
    per_page: 1,
  })

  expect(result).toEqual({
    handling: {},
    price: {},
  })
})

it('getSearchResult', () => {
  mockedItemsJSSearch.mockReturnValue({
    data: {
      aggregations: {
        handling: {},
        price: {},
      },
    },
  })

  getSearchResult({
    aggregationConfig: {},
    filter: {},
    items: [],
    page: 3,
    perPage: 15,
    query: '123',
    sort: 'price_asc',
    sortingConfig: {},
    userItemsIds: [],
  })

  expect(mockedItemsJSSearch).toHaveBeenCalledWith({
    filter: expect.any(Function),
    filters: {},
    page: 3,
    per_page: 15,
    query: '123',
    sort: 'price_asc',
  })
})

import type { FilterFnOption, VisibilityState } from '@tanstack/vue-table'

import { uniq } from 'es-toolkit'

import type { Item, ItemFieldFormat, ItemFlat, ItemType, WeaponClass } from '~/models/item'

import { ITEM_FIELD_FORMAT, ITEM_TYPE, WEAPON_CLASS } from '~/models/item'
import { getWeaponClassesByItemType } from '~/services/item-service'
import { objectEntries } from '~/utils/object'

import type { AggregationConfig, AggregationOptions, AggregationView } from './aggregations'

import {
  AGGREGATION_VIEW,
  aggregationsConfig,
  aggregationsKeysByItemType,
  aggregationsKeysByWeaponClass,
} from './aggregations'

export interface SortingOption { field: keyof Item, order: 'desc' | 'asc' }

export type SortingConfig = Record<string, SortingOption>

export const getAggregationsConfig = (
  itemType: ItemType,
  weaponClass: WeaponClass | null,
): AggregationConfig => {
  const keys = [
    // common aggregations keys
    'type',
    'weaponClass',
    'modId',
    'isNew',
    ...(weaponClass !== null && weaponClass in aggregationsKeysByWeaponClass
      ? aggregationsKeysByWeaponClass[weaponClass] ?? []
      : aggregationsKeysByItemType[itemType] ?? []),
  ] satisfies Array<keyof ItemFlat>

  return Object.fromEntries(keys.map(k => [k, aggregationsConfig[k]]))
}

export const getVisibleAggregationsConfig = (
  aggregations: AggregationConfig,
  excludeFields: (keyof ItemFlat)[] = [],
): AggregationConfig =>
  Object.fromEntries(
    objectEntries(aggregations)
      .filter(([key, value]) => !excludeFields.includes(key) && !value!.hidden),
  )

export const getColumnVisibility = (aggregations: AggregationConfig): VisibilityState =>
  Object.fromEntries(
    objectEntries(aggregations)
      .filter(([, value]) => value!.hidden)
      .map(([key]) => [key, false]),
  )

export const itemTypeOrder = new Map<ItemType, number>(Object.values(ITEM_TYPE).map((value, index) => [value, index]))

export const getFacetsByItemType = (itemTypes: ItemType[], orders: Map<ItemType, number> = itemTypeOrder) => {
  return uniq(itemTypes).sort((a, b) => orders.get(a)! - orders.get(b)!)
}

export const weaponClassOrder = new Map<WeaponClass, number>(Object.values(WEAPON_CLASS).map((value, index) => [value, index]))

export const getFacetsByWeaponClass = (weaponClasses: WeaponClass[], itemType: ItemType, orders: Map<WeaponClass, number> = weaponClassOrder) => {
  return getWeaponClassesByItemType(itemType)
    .filter(wc => weaponClasses.includes(wc))
    .sort((a, b) => orders.get(a)! - orders.get(b)!)
}

const filterFnMap: Record<
  AggregationView,
  (format?: ItemFieldFormat) => FilterFnOption<any>
> = {
  [AGGREGATION_VIEW.Toggle]: format =>
    format === ITEM_FIELD_FORMAT.String ? 'equalsString' : 'auto',

  [AGGREGATION_VIEW.Range]: () => 'inNumberRange',

  [AGGREGATION_VIEW.Checkbox]: format =>
    format === ITEM_FIELD_FORMAT.List ? 'arrIncludesSome' : 'includesSome',
}

export const getFilterFn = (
  options: AggregationOptions,
): FilterFnOption<any> => filterFnMap[options.view]?.(options.format) ?? 'auto'

export function getBuckets(
  rawFacets: Map<string | number | string[] | null | undefined, number>,
): Record<string, number> {
  const result: Record<string, number> = {}

  for (const [bucket, count] of rawFacets) {
    if (bucket == null) {
      continue
    }

    const items = Array.isArray(bucket) ? bucket : [bucket]
    for (const item of items) {
      result[item] = (result[item] ?? 0) + count
    }
  }

  return result
}

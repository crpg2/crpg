import type { FilterFnOption, VisibilityState } from '@tanstack/vue-table'

import { omitBy, uniq } from 'es-toolkit'

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
  excludeKeys: Array<keyof ItemFlat> = [],
  withHidden: boolean = true,
): AggregationConfig => {
  const keys = ([
    // common aggregations keys
    'type',
    'weaponClass',
    'modId',
    'isNew',
    ...(weaponClass !== null && weaponClass in aggregationsKeysByWeaponClass
      ? aggregationsKeysByWeaponClass[weaponClass] ?? []
      : aggregationsKeysByItemType[itemType] ?? []),
  ] satisfies Array<keyof ItemFlat>)
    .filter(key =>
      !excludeKeys.includes(key)
      && (withHidden || !aggregationsConfig[key]?.hidden),
    )
  return Object.fromEntries(keys.map(key => [key, aggregationsConfig[key]]))
}

export const getColumnVisibility = (aggregations: AggregationConfig): VisibilityState =>
  Object.fromEntries(
    objectEntries(aggregations)
      .filter(([, value]) => value?.hidden)
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

const filterFnMap: Record<AggregationView, (format?: ItemFieldFormat) => FilterFnOption<any>> = {
  [AGGREGATION_VIEW.Toggle]: format => format === ITEM_FIELD_FORMAT.String ? 'equalsString' : 'auto',
  [AGGREGATION_VIEW.Range]: () => 'inNumberRange',
  [AGGREGATION_VIEW.Checkbox]: format => format === ITEM_FIELD_FORMAT.List ? 'arrIncludesSome' : 'includesSome',
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

const itemParamIsEmpty = (field: keyof ItemFlat, itemFlat: ItemFlat) => {
  const value = itemFlat[field]
  if (Array.isArray(value) && value.length === 0) {
    return true
  }
  if (!value) {
    return true
  }
  return false
}

// TODO: FIXME: SPEC
export const getItemAggregations = (itemFlat: ItemFlat, omitEmpty = true): AggregationConfig => {
  const aggsConfig = getAggregationsConfig(itemFlat.type, itemFlat.weaponClass, [], false)
  return omitEmpty
    ? omitBy(aggsConfig, (_value, field) => itemParamIsEmpty(field as keyof ItemFlat, itemFlat))
    : aggsConfig
}

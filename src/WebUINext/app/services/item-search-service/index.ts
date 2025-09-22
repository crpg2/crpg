import { uniq } from 'es-toolkit'

import type { Item, ItemFlat, ItemType, WeaponClass } from '~/models/item'

import { ITEM_TYPE, WEAPON_CLASS } from '~/models/item'

import type { AggregationConfig } from './aggregations'

import { getWeaponClassesByItemType } from '../item-service'
import {
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
  const output: AggregationConfig = {
    // common aggregations
    type: aggregationsConfig.type,
    weaponClass: aggregationsConfig.weaponClass,
    modId: aggregationsConfig.modId,
    isNew: aggregationsConfig.isNew,
  }

  if (weaponClass !== null && weaponClass in aggregationsKeysByWeaponClass) {
    aggregationsKeysByWeaponClass[weaponClass]!.forEach((aggKey) => {
      if (aggKey in aggregationsConfig) {
        output[aggKey] = aggregationsConfig[aggKey]
      }
    })
  }

  else if (itemType in aggregationsKeysByItemType) {
    aggregationsKeysByItemType[itemType]!.forEach((aggKey) => {
      if (aggKey in aggregationsConfig) {
        output[aggKey] = aggregationsConfig[aggKey]
      }
    })
  }

  return output
}

export const getVisibleAggregationsConfig = (aggregationsConfig: AggregationConfig, excludeFields: (keyof ItemFlat)[] = []): AggregationConfig => {
  return (Object.keys(aggregationsConfig) as Array<keyof ItemFlat>)
    .filter(key => excludeFields.includes(key) || !aggregationsConfig[key]?.hidden)
    .reduce((obj, key) => ({ ...obj, [key]: aggregationsConfig[key] }), {} as AggregationConfig)
}

export const getFacetsByItemType = (itemTypes: ItemType[]) => {
  const orders = Object.values(ITEM_TYPE)
  return uniq(itemTypes)
    .sort((a, b) => orders.indexOf(a) - orders.indexOf(b))
}

export const getFacetsByWeaponClass = (weaponClasses: WeaponClass[], itemType: ItemType) => {
  const orders = Object.values(WEAPON_CLASS)
  return uniq(weaponClasses)
    .filter(weaponClass => getWeaponClassesByItemType(itemType).includes(weaponClass))
    .sort((a, b) => orders.indexOf(a) - orders.indexOf(b))
}

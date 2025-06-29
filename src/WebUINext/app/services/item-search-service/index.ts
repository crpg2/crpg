import { uniq } from 'es-toolkit'

import type { ItemFlat } from '~/models/item'

import { ItemType, WeaponClass } from '~/models/item'

import type { AggregationConfig } from './aggregations'

import { getWeaponClassesByItemType } from '../item-service'
import {
  aggregationsConfig,
  aggregationsKeysByItemType,
  aggregationsKeysByWeaponClass,
} from './aggregations'

export type SortingConfig = Record<string, { field: keyof ItemFlat, order: 'desc' | 'asc' }>

export const getAggregationsConfig = (
  itemType: ItemType,
  weaponClass: WeaponClass | null,
): AggregationConfig => {
  const output: AggregationConfig = {
    // common aggregations
    type: aggregationsConfig.type,
    weaponClass: aggregationsConfig.weaponClass,
    modId: aggregationsConfig.modId,
    new: aggregationsConfig.new,
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

export const getVisibleAggregationsConfig = (aggregationsConfig: AggregationConfig) => {
  return (Object.keys(aggregationsConfig) as Array<keyof ItemFlat>)
    .filter(key => aggregationsConfig[key]?.hidden !== true)
    .reduce((obj, key) => ({ ...obj, [key]: aggregationsConfig[key] }), {} as AggregationConfig)
}

export const getFacetsByItemType = (items: ItemFlat[]) => {
  const orders = Object.values(ItemType)
  return uniq(items.map(({ type }) => type))
    .sort((a, b) => orders.indexOf(a) - orders.indexOf(b))
}

export const getFacetsByWeaponClass = (items: ItemFlat[], itemType: ItemType) => {
  const orders = Object.values(WeaponClass)
  return uniq(items.map<WeaponClass>(({ weaponClass }) => weaponClass as WeaponClass))
    .filter(weaponClass => getWeaponClassesByItemType(itemType).includes(weaponClass))
    .sort((a, b) => orders.indexOf(a) - orders.indexOf(b))
}

export const filterItemsByType = (items: ItemFlat[], type: ItemType) =>
  type === ItemType.Undefined ? items : items.filter(item => item.type === type)

export const filterItemsByWeaponClass = (items: ItemFlat[], weaponClass: WeaponClass | null) =>
  weaponClass === null ? items : items.filter(fi => fi.weaponClass === weaponClass)

// export const getSortingConfig = (aggregations: AggregationConfig): SortingConfig => {
//   return (Object.keys(aggregations) as Array<keyof ItemFlat>)
//     .filter(key => aggregationsConfig[key]?.view === AggregationView.Range)
//     .reduce((out, agg) => {
//       out[`${agg}_asc`] = {
//         field: agg,
//         order: 'asc',
//       }

//       out[`${agg}_desc`] = {
//         field: agg,
//         order: 'desc',
//       }

//       return out
//     }, {} as SortingConfig)
// }

// export const generateEmptyFiltersModel = (aggregations: AggregationConfig) => {
//   return (Object.keys(aggregations) as [keyof ItemFlat]).reduce(
//     (model, aggKey) => {
//       // model[aggKey] = []; // alwaysArray? TODO: https://github.com/DefinitelyTyped/DefinitelyTyped/blob/master/types/itemsjs/index.d.ts#L38

//       if (aggKey === 'weaponUsage') {
//         model[aggKey] = ['Primary']
//       }
//       else {
//         model[aggKey] = []
//       }

//       return model
//     },
//     {} as FiltersModel<string[]>,
//   )
// }

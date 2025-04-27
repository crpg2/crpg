import { uniq } from 'es-toolkit'

import type { Item, ItemFlat } from '~/models/item'
import type { UserItem } from '~/models/user'

import { ItemType, WeaponClass } from '~/models/item'

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

export const getVisibleAggregationsConfig = (aggregationsConfig: AggregationConfig, excludeFields: (keyof ItemFlat)[] = []): AggregationConfig => {
  return (Object.keys(aggregationsConfig) as Array<keyof ItemFlat>)
    .filter(key => excludeFields.includes(key) || !aggregationsConfig[key]?.hidden)
    .reduce((obj, key) => ({ ...obj, [key]: aggregationsConfig[key] }), {} as AggregationConfig)
}

export const getFacetsByItemType = (itemTypes: ItemType[]) => {
  const orders = Object.values(ItemType)
  return uniq(itemTypes)
    .sort((a, b) => orders.indexOf(a) - orders.indexOf(b))
}

export const getFacetsByWeaponClass = (weaponClasses: WeaponClass[], itemType: ItemType) => {
  const orders = Object.values(WeaponClass)
  return uniq(weaponClasses)
    .filter(weaponClass => getWeaponClassesByItemType(itemType).includes(weaponClass))
    .sort((a, b) => orders.indexOf(a) - orders.indexOf(b))
}

export const filterItemsByType = <T extends { item: Item }>(items: T[], type: ItemType): T[] =>
  type === ItemType.Undefined ? items : items.filter(wrapper => wrapper.item.type === type)

export const filterItemsByName = <T extends { item: Item }>(items: T[], term: string): T[] => {
  if (!term) {
    return items
  }
  const searchTerm = term.toLowerCase()
  return items.filter(wrapper => wrapper.item.name.toLowerCase().includes(searchTerm))
}

export const sortItemsByField = <T extends { item: Item }>(items: T[], sortingOption: SortingOption): T[] => {
  const { field, order } = sortingOption
  const compare = (a: T, b: T) => {
    const aValue = a.item[field]
    const bValue = b.item[field]

    // Special handling for 'type' field to preserve ItemType order
    if (field === 'type') {
      const itemTypes = Object.values(ItemType)
      return (itemTypes.indexOf(aValue as ItemType) - itemTypes.indexOf(bValue as ItemType)) * (order === 'asc' ? 1 : -1)
    }

    // Handle undefined/null values
    if (aValue === null && bValue === null) {
      return 0
    }
    if (aValue === null) {
      return order === 'asc' ? 1 : -1
    }
    if (bValue === null) {
      return order === 'asc' ? -1 : 1
    }

    // String comparison (case-insensitive)
    if (typeof aValue === 'string' && typeof bValue === 'string') {
      const cmp = aValue.localeCompare(bValue, undefined, { sensitivity: 'base' })
      return order === 'asc' ? cmp : -cmp
    }

    // Numeric or other types
    if (aValue < bValue) {
      return order === 'asc' ? -1 : 1
    }
    if (aValue > bValue) {
      return order === 'asc' ? 1 : -1
    }
    return 0
  }

  return items.toSorted(compare)
}

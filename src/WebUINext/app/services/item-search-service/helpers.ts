// import type { ItemFlat } from '~/models/item'
// import type { FiltersModel } from '~/models/item-search'

// import { AggregationView } from '~/models/item-search'
// import { roundFLoat } from '~/utils/math'

// import { aggregationsConfig } from './aggregations'

// export const excludeRangeFilters = (filterModel: FiltersModel<string[] | number[]>) => {
//   return (Object.keys(filterModel) as [keyof ItemFlat])
//     .filter(key => aggregationsConfig[key]!.view !== AggregationView.Range)
//     .reduce((obj, key) => {
//       obj[key] = filterModel[key] as string[]
//       return obj
//     }, {} as FiltersModel<string[]>)
// }

// const applyRangeFilters = (
//   item: ItemFlat,
//   filtersModel: FiltersModel<string[] | number[]>,
// ): boolean => {
//   let result = true;

//   (Object.keys(filtersModel) as [keyof ItemFlat])
//     .filter(f => aggregationsConfig[f]!.view === AggregationView.Range)
//     .forEach((key) => {
//       if (result === false) { return }

//       const values = filtersModel[key] as string[]

//       result
//         = values.length === 0
//           ? true
//           : roundFLoat(item[key] as number) >= Number(values[0])
//             && roundFLoat(item[key] as number) <= Number(values[1])
//     })

//   return result
// }

// export const applyFilters = (
//   item: ItemFlat,
//   filtersModel: FiltersModel<string[] | number[]>,
//   userItemsIds: string[],
// ) => {
//   let result = applyRangeFilters(item, filtersModel)

//   if (userItemsIds.includes(item.id)) {
//     result = false
//   }

//   return result
// }

export const getMinRange = (buckets: number[]): number => {
  if (buckets.length === 0) {
    return 0
  }
  return Math.floor(Math.min(...buckets))
}

export const getMaxRange = (buckets: number[]): number => {
  if (buckets.length === 0) {
    return 0
  }
  return Math.ceil(Math.max(...buckets))
}

export const getStepRange = (values: number[]): number => {
  if (values.every(Number.isInteger)) {
    return 1
  } // Ammo, stackAmount

  const [min, max] = [getMinRange(values), getMaxRange(values)]
  const diff = max - min

  if ((values.length < 20 && diff < 10) || (values.length > 20 && diff < 5)) {
    return 0.1
  }

  return 1
}

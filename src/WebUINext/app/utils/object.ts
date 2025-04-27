import type { Entries } from 'type-fest'

export const mergeObjectWithSum = (obj1: Record<string, number>, obj2: Record<string, number>) =>
  Object.keys(obj1).reduce(
    (obj, key) => {
      obj[key] += obj1[key]
      return obj
    },
    { ...obj2 },
  )

export const getEntries = <T extends object>(obj: T) => Object.entries(obj) as Entries<T>

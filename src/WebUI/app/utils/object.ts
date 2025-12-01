import type { Entries } from 'type-fest'

import { mergeWith } from 'es-toolkit'

export const mergeObjectWithSum = (
  obj1: Record<string, number>,
  obj2: Record<string, number>,
) => mergeWith({ ...obj2 }, obj1, (a: number, b: number) => (a ?? 0) + (b ?? 0))

export const objectEntries = <T extends object>(obj: T) => Object.entries(obj) as Entries<T>

export const objectKeys = <T extends object>(obj: T) => Object.keys(obj) as (keyof T)[]

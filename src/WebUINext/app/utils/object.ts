import type { Entries } from 'type-fest'

import { mergeWith } from 'es-toolkit'

export const mergeObjectWithSum = (obj1: Record<string, number>, obj2: Record<string, number>) => mergeWith(obj1, obj2, item => item)

export const getEntries = <T extends object>(obj: T) => Object.entries(obj) as Entries<T>

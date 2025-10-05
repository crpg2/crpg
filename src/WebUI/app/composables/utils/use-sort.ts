import type { ValueOf } from 'type-fest'

export const SORT = {
  ASC: 'asc',
  DESC: 'desc',
} as const

export type Sort = ValueOf<typeof SORT>

const encodeValue = (key: string, value: Sort) => `${key}_${value}` as Sort
const decodeValue = (key: string, value: string) => value.replace(`${key}_`, '') as Sort

export const useSort = (key: string) => {
  const sort = useRouteQuery<Sort>('sort', SORT.ASC, {
    transform: {
      get(value) {
        return decodeValue(key, value)
      },
      set(value) {
        return encodeValue(key, value)
      },
    },
  })

  const toggleSort = () => {
    sort.value = sort.value === SORT.ASC ? SORT.DESC : SORT.ASC
  }

  return {
    sort,
    toggleSort,
  }
}

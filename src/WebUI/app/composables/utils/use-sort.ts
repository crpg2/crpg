import type { ValueOf } from 'type-fest'

export const SORT = {
  ASC: 'asc',
  DESC: 'desc',
} as const

export type Sort = ValueOf<typeof SORT>

const encodeValue = (key: string, value: Sort) => `${key}_${value}`
const decodeValue = (key: string, value: string) => value.replace(`${key}_`, '') as Sort

// TODO:
export const useSort = (key: string) => {
  const route = useRoute()
  const router = useRouter()

  const sort = computed({
    get() {
      return route.query?.sort !== undefined
        ? decodeValue(key, route.query?.sort as string)
        : SORT.ASC
    },

    set(val: Sort) {
      router.push({
        query: {
          ...route.query,
          sort: val === SORT.ASC ? undefined : encodeValue(key, val),
        },
      })
    },
  })

  const toggleSort = () => {
    sort.value = sort.value === SORT.ASC ? SORT.DESC : SORT.ASC
  }

  return {
    sort: readonly(sort),
    toggleSort,
  }
}

import type { FilterFn } from '@tanstack/table-core'

declare module '@tanstack/vue-table' {
  interface FilterFns {
    includesSome: FilterFn<unknown>
  }
}
export const includesSome: FilterFn<any> = (
  row,
  columnId: string,
  filterValue: unknown[],
) => filterValue.includes(row.getValue<unknown>(columnId))
includesSome.autoRemove = (val: any) => testFalse(val) || !val?.length

function testFalse(val: any) {
  return val === undefined || val === null || val === ''
}

import type { FilterFn } from '@tanstack/table-core'

export const includesSome: FilterFn<any> = (
  row,
  columnId: string,
  filterValue: unknown[],
) => filterValue.includes(row.getValue<unknown>(columnId))

includesSome.autoRemove = (val: any) => testFalse(val) || !val?.length

function testFalse(val: any) {
  return val === undefined || val === null || val === ''
}

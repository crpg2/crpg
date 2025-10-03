import type { FilterFn } from '@tanstack/table-core'

export const includesSome: FilterFn<unknown> = (
  row,
  columnId: string,
  filterValue: unknown[],
) => {
  return filterValue.includes(String(row.getValue<unknown>(columnId)))
}

includesSome.autoRemove = (val: any) => testFalse(val) || !val?.length

function testFalse(val: any) {
  return val === undefined || val === null || val === ''
}

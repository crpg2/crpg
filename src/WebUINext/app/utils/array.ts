import { xor } from 'es-toolkit'

export const range = (start: number, end: number) =>
  Array.from({ length: end - start + 1 })
    .fill(null)
    .map((_, idx) => start + idx)

// TODO: SPEC
export const getIndexToIns = (arr: number[], num: number): number => {
  const index = arr.findIndex(currentNum => num <= currentNum)
  return index === -1 ? arr.length : index
}

export const toggle = <T = unknown>(arr: T[], el: T): T[] => xor(arr, [el])

import { expect, it } from 'vitest'

import { getIndexToInsert, range } from '../array'

it('range', () => {
  expect(range(3, 6)).toEqual([3, 4, 5, 6])
})

it.each([
  { arr: [1, 3, 5, 7], num: 4, expected: 2 },
  { arr: [1, 3, 5, 7], num: 0, expected: 0 },
  { arr: [1, 3, 5, 7], num: 8, expected: 4 },
  { arr: [], num: 10, expected: 0 },
])('getIndexToInsert should return $expected for getIndexToInsert($arr, $num)', ({ arr, num, expected }) => {
  expect(getIndexToInsert(arr, num)).toBe(expected)
})

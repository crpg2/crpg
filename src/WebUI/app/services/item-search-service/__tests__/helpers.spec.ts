import { expect, it } from 'vitest'

import { getMaxRange, getMinRange, getStepRange } from '~/services/item-search-service/helpers'

it.each([
  [[], 0],
  [[1, 2, 3], 1],
  [[1.1], 1],
  [[1.1, 1.05], 1],
  [[1.2, 1.001], 1],
  [[2.2, 100.001], 2],
])('getMinRange - values: %j', (values, expectation) => {
  expect(getMinRange(values)).toEqual(expectation)
})

it.each([
  [[], 0],
  [[1, 2, 3], 3],
  [[1.1], 2],
  [[1.1, 1.05], 2],
  [[1.2, 1.001], 2],
  [[2.2, 100.001], 101],
])('getMaxRange - values: %j', (values, expectation) => {
  expect(getMaxRange(values)).toEqual(expectation)
})

it.each<[number[], number]>([
  [[1, 2, 3], 1],
  [[1.5, 1.6, 0.8, 1.12, 1.2], 0.1],
  [[120, 130, 30, 125, 135, 145, 20, 21, 22, 22.5, 23], 1],
  [
    [
      0.1,
      0.2,
      0.3,
      0.4,
      0.5,
      0.6,
      0.7,
      0.8,
      0.9,
      1,
      1.1,
      1.2,
      1.3,
      1.4,
      1.5,
      1.6,
      1.7,
      1.8,
      1.9,
      2,
      2.1,
    ],
    0.1,
  ],
])('getStepRange - values: %j', (values, expectation) => {
  expect(getStepRange(values)).toEqual(expectation)
})

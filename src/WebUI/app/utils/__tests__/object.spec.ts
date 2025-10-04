import { expect, it } from 'vitest'

import { mergeObjectWithSum } from '../object'

it('mergeObjectWithSum', () => {
  expect(mergeObjectWithSum({
    applejack: 10,
    rarity: 1,
  }, {
    applejack: 1,
    rarity: 0,
  })).toEqual({
    applejack: 11,
    rarity: 1,
  })
})

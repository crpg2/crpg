import { describe, expect, it } from 'vitest'

import { argbIntToRgbHexColor, rgbHexColorToArgbInt } from '../color'

const CASES = [
  ['#ffffff', 4294967295],
  ['#000000', 4278190080],
  ['#7ec699', 4286498457],
]

describe('color', () => {
  it.each(CASES)('rgbHexColorToArgbInt', (a, b) => {
    expect(rgbHexColorToArgbInt(a as string)).toBe(b)
  })

  it.each(CASES)('argbIntToRgbHexColor', (a, b) => {
    expect(argbIntToRgbHexColor(b as number)).toBe(a)
  })
})

import { describe, expect, it } from 'vitest'

import { applyPolynomialFunction, roundFLoat } from '../math'

describe('roundFLoat', () => {
  it.each([
    [0, 0],
    [1, 1],
    [0.0001, 0],
    [0.4999, 0.5],
    [0.5001, 0.5],
    [0.5499, 0.55],
    [0.545, 0.55],
    [0.5449, 0.54],
    [1.1, 1.1],
    [1.1222, 1.12],
    [1.0001, 1],
    [1.1029, 1.1],
  ])('roundFLoat', (num, expectation) => {
    expect(roundFLoat(num)).toEqual(expectation)
  })
})

describe('applyPolynomialFunction', () => {
  it.each([
    // Linear: f(x) = 2x + 3
    [2, [2, 3], 7],
    [0, [2, 3], 3],
    [1, [2, 3], 5],
    // Quadratic: f(x) = x^2 + 2x + 1
    [1, [1, 2, 1], 4],
    [2, [1, 2, 1], 9],
    [0, [1, 2, 1], 1],
    // Cubic: f(x) = 2x^3 - x^2 + 3x - 4
    [1, [2, -1, 3, -4], 0],
    [2, [2, -1, 3, -4], 14],
    [0, [2, -1, 3, -4], -4],
    // Constant: f(x) = 5
    [10, [5], 5],
    [0, [5], 5],
    // Zero coefficients
    [3, [0, 0, 0], 0],
    // Negative coefficients
    [2, [-1, -2, -3], -11],
    // Empty coefficients
    [2, [], 0],
  ])('applyPolynomialFunction(%p, %p) === %p', (x, coefficients, expected) => {
    expect(applyPolynomialFunction(x, coefficients)).toBe(expected)
  })
})

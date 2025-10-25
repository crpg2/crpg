import type { Row } from '@tanstack/vue-table'

import { describe, expect, it } from 'vitest'

import { includesSome } from '../grid'

describe('includesSome', () => {
  it('returns true if row value is included in filterValue', () => {
    const row = { getValue: () => 'foo' }

    expect(includesSome(
      // @ts-expect-error ///
      row as Row<unknown>,
      'col',
      ['foo', 'bar'],
      () => {},
    )).toBe(true)
  })

  it('returns false if row value is not included in filterValue', () => {
    const row = { getValue: () => 'baz' }
    expect(includesSome(
      // @ts-expect-error ///
      row,
      'col',
      ['foo', 'bar'],
      () => {},
    )).toBe(false)
  })

  it('autoRemove returns true for undefined, null, empty string, or empty array', () => {
    expect(includesSome.autoRemove?.(undefined)).toBe(true)
    expect(includesSome.autoRemove?.(null)).toBe(true)
    expect(includesSome.autoRemove?.('')).toBe(true)
    expect(includesSome.autoRemove?.([])).toBe(true)
  })

  it('autoRemove returns false for non-empty array', () => {
    expect(includesSome.autoRemove?.(['foo'])).toBe(false)
  })
})

import { describe, expect, it } from 'vitest'

import {
  checkIsDateExpired,
  computeLeftMs,
  convertHumanDurationToMs,
  daysToMs,
  isBetween,
  msToHours,
  msToMinutes,
  msToSeconds,
  parseTimestamp,
} from '../../../app/utils/date'

describe('msToHours', () => {
  it.each([
    [0, 0],
    [3600000, 1],
    [7200000, 2],
    [3599999, 0],
    [86399999, 23],
  ])('converts %i ms to %i hours', (input, expected) => {
    expect(msToHours(input)).toBe(expected)
  })
})

describe('msToMinutes', () => {
  it.each([
    [0, 0],
    [60000, 1],
    [120000, 2],
    [59999, 0],
    [3599999, 59],
  ])('converts %i ms to %i minutes', (input, expected) => {
    expect(msToMinutes(input)).toBe(expected)
  })
})

describe('msToSeconds', () => {
  it.each([
    [0, 0],
    [1000, 1],
    [2000, 2],
    [999, 0],
    [59999, 59],
  ])('converts %i ms to %i seconds', (input, expected) => {
    expect(msToSeconds(input)).toBe(expected)
  })
})

describe('daysToMs', () => {
  it.each([
    [0, 0],
    [1, 86400000],
    [2, 172800000],
  ])('converts %i days to %i ms', (input, expected) => {
    expect(daysToMs(input)).toBe(expected)
  })
})

describe('convertHumanDurationToMs', () => {
  it.each([
    [{ days: 0, hours: 0, minutes: 0 }, 0],
    [{ days: 1, hours: 1, minutes: 1 }, 90060000],
    [{ days: 2, hours: 0, minutes: 30 }, 172800000 + 1800000],
  ])('converts %o to %i ms', (input, expected) => {
    expect(convertHumanDurationToMs(input)).toBe(expected)
  })
})

describe('parseTimestamp', () => {
  it.each([
    [0, { days: 0, hours: 0, minutes: 0 }],
    [90061000, { days: 1, hours: 1, minutes: 1 }],
    [172980000, { days: 2, hours: 0, minutes: 3 }],
  ])('parses %i ms to %o', (input, expected) => {
    expect(parseTimestamp(input)).toEqual(expected)
  })
})

describe('checkIsDateExpired', () => {
  it.each([
    [new Date(Date.now() - 10000), 5000, true],
    [new Date(Date.now()), 10000, false],
    [new Date(Date.now() - 5000), 10000, false],
  ])('checks if date %p with duration %i is expired: %s', (createdAt, duration, expected) => {
    expect(checkIsDateExpired(createdAt, duration)).toBe(expected)
  })
})

describe('computeLeftMs', () => {
  it.each([
    [new Date(Date.now() - 10000), 5000, 0],
    [new Date(Date.now()), 10000, expect.any(Number)],
    [new Date(Date.now() - 5000), 10000, expect.any(Number)],
  ])('computes left ms for %p with duration %i', (createdAt, duration, expected) => {
    const result = computeLeftMs(createdAt, duration)
    if (expected === 0) {
      expect(result).toBe(0)
    }
    else {
      expect(result).toBeGreaterThan(0)
    }
  })
})

describe('isBetween', () => {
  const now = new Date()
  const before = new Date(now.getTime() - 1000)
  const after = new Date(now.getTime() + 1000)
  it.each([
    [now, before, after, true],
    [before, now, after, false],
    [after, before, now, false],
  ])('checks if %p is between %p and %p: %s', (date, start, end, expected) => {
    expect(isBetween(date, start, end)).toBe(expected)
  })
})

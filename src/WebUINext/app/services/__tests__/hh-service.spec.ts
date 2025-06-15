// @vitest-environment node
import { describe, expect, it, vi } from 'vitest'

import { Region } from '~/models/region'

import { getHHEventByRegion, getHHEventRemaining, } from '../hh-service'



describe('getHHEventByRegion', () => {
  const cfg = 'Eu|00:00|23:59|Europe/Paris,Na|20:00|22:00|America/Chicago';

  it.each<[string, Region, { start: Date, end: Date },]>([
    [
      cfg,
      Region.Eu,
      {
        start: new Date('2000-01-31T23:00:00.000Z'),
        end: new Date('2000-02-01T22:59:00.000Z'),
      },
    ],
    [
      cfg,
      Region.Na,
      {
        start: new Date('2000-02-02T02:00:00.000Z'),
        end: new Date('2000-02-02T04:00:00.000Z'),
      },
    ],
  ])('getHHEventByRegion - region: %s', (config, region, expectation) => {
    vi.setSystemTime(new Date(2000, 1, 1, 13))
    expect(getHHEventByRegion(config, region)).toEqual(expectation)
  })
})

describe('getHHEventRemaining', () => {
  const event = {
    start: new Date('2000-02-01T18:30:00.000Z'),
    end: new Date('2000-02-01T20:30:00.000Z'),
  }

  it.each<[Date, number]>([
    [new Date('2000-02-01T20:29:59.999Z'), 1],
    [new Date('2000-02-01T20:30:00.000Z'), 0],
    [new Date('2000-02-01T20:31:00.000Z'), 0],
    [new Date('2000-02-01T18:30:00.000Z'), 7200000],
    [new Date('2000-02-01T18:29:00.000Z'), 0],
  ])('getHHEventRemaining - now: %s', (now, expectation) => {
    vi.setSystemTime(now)
    expect(getHHEventRemaining(event)).toEqual(expectation)
  })
})

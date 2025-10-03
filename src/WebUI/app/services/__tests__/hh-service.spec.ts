import { describe, expect, it, vi } from 'vitest'

import type { Region } from '~/models/region'

import { getHHEventByRegion, getHHEventRemainingSeconds, parseHHScheduleConfig } from '~/services/hh-service'

it('parseHHScheduleConfig', () => {
  expect(parseHHScheduleConfig('Eu|19:30|21:30|Europe/Paris,Na|20:00|22:00|America/Chicago')).toEqual({
    Eu: {
      start: { hour: 19, minute: 30 },
      end: { hour: 21, minute: 30 },
      tz: 'Europe/Paris',
    },
    Na: {
      end: { hour: 22, minute: 0 },
      start: { hour: 20, minute: 0 },
      tz: 'America/Chicago',
    },
  })
})

it.each<[ Region, string, { start: Date, end: Date }]>([
  [
    'Eu',
    'Eu|19:30|21:30|Europe/Paris',
    {
      start: new Date('2000-02-01T18:30:00.000Z'),
      end: new Date('2000-02-01T20:30:00.000Z'),
    },
  ],
  [
    'Eu',
    'Eu|23:30|01:30|Europe/Paris', // begins at the end of one day and ends at the beginning of another day
    {
      start: new Date('2000-02-01T22:30:00.000Z'),
      end: new Date('2000-02-02T00:30:00.000Z'),
    },
  ],
  [
    'Na',
    'Na|20:00|22:00|America/Chicago',
    {
      end: new Date('2000-02-02T04:00:00.000Z'),
      start: new Date('2000-02-02T02:00:00.000Z'),
    },
  ],
])('getHHEventByRegion - region: %s', (region, config, expectation) => {
  vi.setSystemTime(new Date(2000, 1, 1, 13))
  expect(getHHEventByRegion(config, region)).toEqual(expectation)
})

describe('getHHEventRemainingSeconds', () => {
  const event = {
    start: new Date('2000-02-01T18:30:00.000Z'),
    end: new Date('2000-02-01T20:30:00.000Z'),
  }

  it.each<[string, Date, number]>([
    ['', new Date('2000-02-01T18:29:00.000Z'), 0],
    ['', new Date('2000-02-01T18:30:00.000Z'), 7200],
    ['', new Date('2000-02-01T20:29:59.000Z'), 1],
    ['', new Date('2000-02-01T20:30:00.000Z'), 0],
    ['', new Date('2000-02-01T20:30:01.000Z'), 0],
  ])('getHHEventRemaining - now: %s', (_, now, expectation) => {
    vi.setSystemTime(now)
    expect(getHHEventRemainingSeconds(event)).toEqual(expectation)
  })
})

import type { TimeDuration } from '@internationalized/date'

import { now, toLocalTimeZone } from '@internationalized/date'

import type { Region } from '~/models/region'

import { isBetween, msToSeconds } from '~/utils/date'

type HHScheduleTime = Pick<TimeDuration, 'hours' | 'minutes'>

interface HHScheduleConfig {
  tz: string
  start: HHScheduleTime
  end: HHScheduleTime
}

const getHHScheduleConfig = (config: string) => {
  // TODO: FIXME: error handling
  return config.split(',').reduce(
    (out, cur) => {
      const [region, start, end, tz] = cur.split('|') as [Region, string, string, string]

      const [startHours, startMinutes] = start.split(':')
      const [endHours, endMinutes] = end.split(':')

      out[region] = {
        start: {
          hours: Number(startHours),
          minutes: Number(startMinutes),
        },
        end: {
          hours: Number(endHours),
          minutes: Number(endMinutes),
        },
        tz,
      }
      return out
    },
    {} as Record<Region, HHScheduleConfig>,
  )
}

export interface HHEvent {
  end: Date
  start: Date
}

export const getHHEventByRegion = (config: string, region: Region): HHEvent => {
  const { start, end, tz } = getHHScheduleConfig(config)[region]

  const startDt = now(tz).set({ hour: start.hours, minute: start.minutes })
  const endDt = now(tz).set({ hour: end.hours, minute: end.minutes })

  return {
    start: toLocalTimeZone(startDt).toDate(),
    end: toLocalTimeZone(endDt).toDate(),
  }
}

/**
 * @returns seconds
 */
export const getHHEventRemaining = (event: HHEvent): number => {
  const { start, end } = event

  const now = new Date()

  // TODO: FIXME:
  console.log({ start, end, now })

  if (!isBetween(now, start, end)) {
    console.log('!isBetween')
    return 0
  }

  return msToSeconds(end.getTime() - now.getTime())
}

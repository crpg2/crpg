import { DateTime } from 'luxon' // TODO: someday try to do without any library ;)

import type { Region } from '~/models/region'

import { isBetween } from '~/utils/date'

interface HHScheduleTime {
  hours: number
  minutes: number
}
interface HHScheduleConfig {
  tz: string
  end: HHScheduleTime
  start: HHScheduleTime
}

export const getHHScheduleConfig = (config: string) => {
  return config.split(',').reduce(
    (out, cur) => {
      const [region, start, end, tz] = cur.split('|') as [Region, string, string, string]

      const [startHours, startMinutes] = start.split(':')
      const [endHours, endMinutes] = end.split(':')

      out[region] = {
        end: {
          hours: Number(endHours),
          minutes: Number(endMinutes),
        },
        start: {
          hours: Number(startHours),
          minutes: Number(startMinutes),
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
  const cfg = getHHScheduleConfig(config)[region]

  const startDt = DateTime.fromObject(
    { hour: cfg.start.hours, minute: cfg.start.minutes, second: 0 },
    { zone: cfg.tz },
  )

  const endDt = DateTime.fromObject(
    { hour: cfg.end.hours, minute: cfg.end.minutes, second: 0 },
    { zone: cfg.tz },
  )

  return {
    end: endDt.setZone(DateTime.local().zoneName!).toJSDate(),
    start: startDt.setZone(DateTime.local().zoneName!).toJSDate(),
  }
}

export const getHHEventRemaining = (event: HHEvent) => {
  if (!isBetween(new Date(), event.start, event.end)) {
    return 0
  }
  return event.end.getTime() - new Date().getTime()
}

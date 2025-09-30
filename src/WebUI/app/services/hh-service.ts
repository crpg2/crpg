import type { ZonedDateTime } from '@internationalized/date'

import { now, toLocalTimeZone } from '@internationalized/date'

import type { Region } from '~/models/region'

import { isBetween, msToSeconds } from '~/utils/date'

interface HHScheduleTime {
  hour: number
  minute: number
}

interface HHScheduleConfig {
  tz: string
  start: HHScheduleTime
  end: HHScheduleTime
}

export const parseHHScheduleConfig = (config: string): Partial<Record<Region, HHScheduleConfig>> => {
  return Object.fromEntries(
    config.split(',').map(
      (cur) => {
        const [region, start, end, tz] = cur.split('|') as [Region, string, string, string]

        if (!region || !start || !end || !tz) {
          throw new Error(`Invalid schedule config: ${cur}`)
        }

        const [startHourStr, startMinuteStr] = start.split(':')
        const [endHourStr, endMinuteStr] = end.split(':')

        const startHour = Number(startHourStr)
        const startMinute = Number(startMinuteStr)
        const endHour = Number(endHourStr)
        const endMinute = Number(endMinuteStr)

        if (
          startHourStr === undefined || startMinuteStr === undefined
          || endHourStr === undefined || endMinuteStr === undefined
          || Number.isNaN(startHour) || Number.isNaN(startMinute)
          || Number.isNaN(endHour) || Number.isNaN(endMinute)
        ) {
          throw new TypeError(`Invalid time format in: ${cur}`)
        }

        return [
          region,
          {
            start: { hour: startHour, minute: startMinute },
            end: { hour: endHour, minute: endMinute },
            tz,
          } satisfies HHScheduleConfig,
        ]
      },
    ),
  )
}

export interface HHEvent {
  end: Date
  start: Date
}

const _normalizeEventDates = (startDt: ZonedDateTime, endDt: ZonedDateTime) => {
  if (endDt <= startDt) {
    return { startDt, endDt: endDt.add({ days: 1 }) }
  }
  return { startDt, endDt }
}

const _buildDateTime = (tz: string, hour: number, minute: number) =>
  now(tz).set({ hour, minute, second: 0, millisecond: 0 })

export const getHHEventByRegion = (config: string, region: Region): HHEvent => {
  const cfg = parseHHScheduleConfig(config)[region]

  if (!cfg) {
    console.error('Invalid schedule config', { config, region })
    throw new Error(`Schedule config not found for region "${region}"`)
  }

  const { start, end, tz } = cfg
  const startDt = _buildDateTime(tz, start.hour, start.minute)
  const endDt = _buildDateTime(tz, end.hour, end.minute)

  const { startDt: normalizedStart, endDt: normalizedEnd } = _normalizeEventDates(startDt, endDt)

  return {
    start: toLocalTimeZone(normalizedStart).toDate(),
    end: toLocalTimeZone(normalizedEnd).toDate(),
  }
}

/**
 * @returns seconds
 */
export const getHHEventRemainingSeconds = (event: HHEvent): number => {
  const { start, end } = event
  const now = new Date()

  return isBetween(now, start, end) ? msToSeconds(end.getTime() - now.getTime()) : 0
}

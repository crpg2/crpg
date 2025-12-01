import type { Platform as _Platform } from '#api'
import type { ValueOf } from 'type-fest'

export const PLATFORM = {
  Steam: 'Steam',
  EpicGames: 'EpicGames',
  /**
   * @deprecated
   */
  Microsoft: 'Microsoft',
} as const satisfies Record<_Platform, _Platform>

export type Platform = ValueOf<typeof PLATFORM>

export const AVAILABLE_PLATFORM = [
  PLATFORM.Steam,
  PLATFORM.EpicGames,
]

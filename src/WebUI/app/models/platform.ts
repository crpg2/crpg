import type { ValueOf } from 'type-fest'

import type { Platform as _Platform } from '~/api'

export const PLATFORM = {
  Steam: 'Steam',
  EpicGames: 'EpicGames',
  /**
   * @deprecated
   */
  Microsoft: 'Microsoft',
} as const satisfies Record<_Platform, _Platform>

export type Platform = ValueOf<typeof PLATFORM>

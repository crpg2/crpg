import type { ValueOf } from 'type-fest'

import type { Platform as _Platform } from '~/api'

export const PLATFORM = {
  Steam: 'Steam',
  EpicGames: 'EpicGames',
} as const satisfies Record<Exclude<_Platform, 'Microsoft'>, _Platform>

export type Platform = ValueOf<typeof PLATFORM>

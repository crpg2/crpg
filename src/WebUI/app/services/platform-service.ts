import type { Platform } from '~/models/platform'

import { PLATFORM } from '~/models/platform'

export const platformToIcon: Record<Platform, string> = {
  [PLATFORM.EpicGames]: 'epic-games',
  [PLATFORM.Steam]: 'steam-transparent',
  [PLATFORM.Microsoft]: 'xbox',
}

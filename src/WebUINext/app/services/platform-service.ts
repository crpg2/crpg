import type { Platform } from '~/models/platform'

import { PLATFORM } from '~/models/platform'

export const platformToIcon: Record<Platform, string> = {
  [PLATFORM.EpicGames]: 'epic-games',
  [PLATFORM.Microsoft]: 'xbox',
  [PLATFORM.Steam]: 'steam-transparent',
}

import { Platform } from '~/models/platform'

export const platformToIcon: Record<Platform, string> = {
  [Platform.EpicGames]: 'epic-games',
  [Platform.Microsoft]: 'xbox',
  [Platform.Steam]: 'steam-transparent',
}

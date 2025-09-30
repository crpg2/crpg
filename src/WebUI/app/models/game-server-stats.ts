import type { GameMode } from './game-mode'
import type { Region } from './region'

interface GameStats {
  playingCount: number
}

export type GameServerModeStats = Partial<Record<GameMode, GameStats>>

export type GameServerRegionStats = Partial<Record<Region, GameServerModeStats>>

export interface GameServerStats {
  total: GameStats
  regions: GameServerRegionStats
}

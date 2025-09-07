import { getGameServerStatistics } from '#hey-api/sdk.gen'

import type { GameServerModeStats, GameServerRegionStats, GameServerStats } from '~/models/game-server-stats'

// TODO: move to backend
const omitEmptyGameServerStats = (regions: GameServerRegionStats): GameServerRegionStats => Object.fromEntries(
  Object.entries(regions)
    .map(([region, gameModes]) => {
      const filteredModes = Object.fromEntries(
        Object.entries(gameModes).filter(
          ([_, modeData]) => modeData.playingCount > 0,
        ),
      )

      return Object.keys(filteredModes).length > 0
        ? [region, filteredModes]
        : null
    })
    .filter((region): region is [string, GameServerModeStats] => region !== null),
)

export const getGameServerStats = async (): Promise<GameServerStats> => {
  const { data: { regions, total } } = await getGameServerStatistics({ })
  return {
    total,
    regions: omitEmptyGameServerStats(regions),
  }
}

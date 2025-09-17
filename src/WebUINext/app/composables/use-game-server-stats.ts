import { getGameServerStats } from '~/services/game-server-statistics-service'
import { pollGameServerStatsSymbol } from '~/symbols'

import { useAsyncStateWithPoll } from './utils/use-async-state'

export const useGameServerStats = (immediate = true) => {
  const {
    state: gameServerStats,
    execute: loadGameServerStats,
  } = useAsyncStateWithPoll(
    () => getGameServerStats(),
    {
      regions: {},
      total: { playingCount: 0 },
    },
    {
      immediate,
      resetOnExecute: false,
      pollKey: pollGameServerStatsSymbol,
    },
  )

  return {
    gameServerStats,
    loadGameServerStats,
  }
}

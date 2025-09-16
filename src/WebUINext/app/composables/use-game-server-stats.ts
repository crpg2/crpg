import { usePollInterval } from '~/composables/utils/use-poll-interval'
import { getGameServerStats } from '~/services/game-server-statistics-service'
import { pollGameServerStatsSymbol } from '~/symbols'

export const useGameServerStats = (immediate = true) => {
  const {
    state: gameServerStats,
    execute: loadGameServerStats,
  } = useAsyncState(
    () => getGameServerStats(),
    {
      regions: {},
      total: { playingCount: 0 },
    },
    {
      immediate,
    },
  )

  usePollInterval(
    {
      key: pollGameServerStatsSymbol,
      fn: loadGameServerStats,
    },
  )

  return {
    gameServerStats,
    loadGameServerStats,
  }
}

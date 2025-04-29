import { usePollInterval } from '~/composables/use-poll-interval'
// import { getGameServerStats } from '~/services/game-server-statistics-service'
import { getGameServerStatistics } from '#hey-api/sdk.gen'

export const useGameServerStats = () => {
  const {
    state: gameServerStats,
    execute: loadGameServerStats,
  } = useAsyncState(
    async () => {
      const { data } = await getGameServerStatistics({ composable: '$fetch' })
      return data
    },
    {
      regions: {},
      total: { playingCount: 0 },
    },
    {
      immediate: false,
    },
  )

  // TODO:
  const { subscribe, unsubscribe } = usePollInterval()
  const id = Symbol('loadGameServerStats')

  onMounted(() => {
    subscribe(id, loadGameServerStats)
  })

  onBeforeUnmount(() => {
    unsubscribe(id)
  })

  return {
    gameServerStats,
    loadGameServerStats,
  }
}

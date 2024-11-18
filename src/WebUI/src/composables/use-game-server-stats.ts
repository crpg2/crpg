import { usePollInterval } from '~/composables/use-poll-interval'
import { getGameServerStats } from '~/services/game-server-statistics-service'

export const useGameServerStats = () => {
  const { execute: loadGameServerStats, state: gameServerStats } = useAsyncState(
    () => getGameServerStats(),
    {
      regions: {},
      total: { playingCount: 0 },
    },
    {
      immediate: false,
    },
  )

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

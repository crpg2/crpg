import { usePollInterval } from '~/composables/utils/use-poll-interval'
import { getGameServerStats } from '~/services/game-server-statistics-service'

export const useGameServerStats = () => {
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
      immediate: false,
    },
  )

  // TODO: nuxt plugin
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

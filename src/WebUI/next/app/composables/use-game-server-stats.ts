import { getGameServerStatistics } from '#hey-api/sdk.gen'

import { usePollInterval } from '~/composables/use-poll-interval'

export const useGameServerStats = () => {
  const {
    state: gameServerStats,
    execute: loadGameServerStats,
  } = useAsyncState(
    async () => {
      const { data } = await getGameServerStatistics({ composable: '$fetch' })
      return data!
    },
    {
      // TODO: fix swagger types
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

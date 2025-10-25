import { getGameServerStats } from '~/services/game-server-statistics-service'

export const useGameServerStats = () => {
  return useAsyncDataCustom(
    ['gameServerStats'],
    () => getGameServerStats(),
    {
      default: () => ({ regions: {}, total: { playingCount: 0 } }),
      loadingIndicator: false,
    },
  )
}

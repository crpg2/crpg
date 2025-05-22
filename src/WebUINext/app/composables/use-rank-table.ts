import { createRankTable } from '~/services/leaderboard-service'

export const useRankTable = () => {
  const rankTable = computed(() => createRankTable())

  return {
    rankTable,
  }
}

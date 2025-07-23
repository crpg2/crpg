import type { GameMode } from '~/models/game-mode'

import { GAME_MODE } from '~/models/game-mode'

export const useGameModeQuery = () => {
  const route = useRoute()
  const router = useRouter()

  const gameModeModel = computed({
    get() {
      return (route.query?.gameMode as GameMode) || GAME_MODE.CRPGBattle
    },

    set(gameMode: GameMode) {
      router.replace({
        query: {
          ...route.query,
          gameMode,
        },
      })
    },
  })

  const gameModes = Object.values(GAME_MODE)

  return {
    gameModeModel,
    gameModes,
  }
}

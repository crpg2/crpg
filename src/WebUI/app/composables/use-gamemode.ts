import type { GameMode } from '~/models/game-mode'

import { GAME_MODE } from '~/models/game-mode'

export const useGameModeQuery = () => {
  const gameModeModel = useRouteQuery<GameMode>('gameMode', GAME_MODE.CRPGBattle)

  const gameModes = Object.values(GAME_MODE)

  return {
    gameModeModel,
    gameModes,
  }
}

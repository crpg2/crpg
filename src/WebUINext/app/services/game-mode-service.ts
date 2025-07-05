import type { GameMode } from '~/models/game-mode'

import { GAME_MODE } from '~/models/game-mode'

export const gameModeToIcon: Record<GameMode, string> = {
  [GAME_MODE.CRPGBattle]: 'game-mode-battle',
  [GAME_MODE.CRPGConquest]: 'game-mode-conquest',
  [GAME_MODE.CRPGDTV]: 'game-mode-dtv',
  [GAME_MODE.CRPGDuel]: 'game-mode-duel',
  [GAME_MODE.CRPGTeamDeathmatch]: 'game-mode-teamdeathmatch',
  [GAME_MODE.CRPGCaptain]: 'game-mode-captain',
  [GAME_MODE.CRPGUnknownGameMode]: '',
  [GAME_MODE.CRPGSiege]: '',
  [GAME_MODE.CRPGSkirmish]: '',
}

export const rankedGameModes: GameMode[] = [GAME_MODE.CRPGBattle, GAME_MODE.CRPGDuel]

export const checkIsRankedGameMode = (gameMode: GameMode) => rankedGameModes.includes(gameMode)

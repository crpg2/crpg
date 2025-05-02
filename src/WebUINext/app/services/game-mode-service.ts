import { GameMode } from '~/models/game-mode'

export const gameModeToIcon: Record<GameMode, string> = {
  [GameMode.Battle]: 'game-mode-battle',
  [GameMode.Conquest]: 'game-mode-conquest',
  [GameMode.DTV]: 'game-mode-dtv',
  [GameMode.Duel]: 'game-mode-duel',
  [GameMode.TeamDeathmatch]: 'game-mode-teamdeathmatch',
  [GameMode.Captain]: 'game-mode-captain',
}

export const rankedGameModes: GameMode[] = [GameMode.Battle, GameMode.Duel]

export const checkIsRankedGameMode = (gameMode: GameMode) => rankedGameModes.includes(gameMode)

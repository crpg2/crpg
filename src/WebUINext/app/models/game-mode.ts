import type { ValueOf } from 'type-fest'

import type { GameMode as _GameMode } from '~/api'

export const GAME_MODE = {
  CRPGUnknownGameMode: 'CRPGUnknownGameMode',
  CRPGBattle: 'CRPGBattle',
  CRPGConquest: 'CRPGConquest',
  CRPGDTV: 'CRPGDTV',
  CRPGDuel: 'CRPGDuel',
  CRPGTeamDeathmatch: 'CRPGTeamDeathmatch',
  CRPGCaptain: 'CRPGCaptain',
  CRPGSiege: 'CRPGSiege',
  CRPGSkirmish: 'CRPGSkirmish',
} as const satisfies Record<_GameMode, _GameMode>

export type GameMode = ValueOf<typeof GAME_MODE>

export const ACTUAL_GAME_MODES: GameMode[] = [
  GAME_MODE.CRPGBattle,
  GAME_MODE.CRPGDuel,
  GAME_MODE.CRPGDTV,
  GAME_MODE.CRPGConquest,
  GAME_MODE.CRPGTeamDeathmatch,
  GAME_MODE.CRPGCaptain,
]

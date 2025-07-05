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

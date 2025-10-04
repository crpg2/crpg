// import { mockGet } from 'vi-fetch'
import { expect, it, vi } from 'vitest'

import type { Result } from '~/api.config'
import type { GameServerStats } from '~/models/game-server-stats'

// import { response } from '~/__mocks__/crpg-client'
import { GAME_MODE } from '~/models/game-mode'
import { REGION } from '~/models/region'
import { getGameServerStats } from '~/services/game-server-statistics-service'

const response = <T>(data: T): Result<T> => ({
  data,
  errors: null,
})

const { getGameServerStatistics } = vi.hoisted(() => ({
  getGameServerStatistics: vi.fn(),
}))

vi.mock('#api/sdk.gen', () => ({
  getGameServerStatistics,
}))

it('getGameServerStats', async () => {
  const mockServerStats: GameServerStats = {
    total: {
      playingCount: 12,
    },
    regions: {
      [REGION.Eu]: {
        [GAME_MODE.CRPGBattle]: {
          playingCount: 12,
        },
        [GAME_MODE.CRPGDTV]: {
          playingCount: 0,
        },
      },
      [REGION.Na]: {
        [GAME_MODE.CRPGDTV]: {
          playingCount: 0,
        },
      },
    },
  }

  getGameServerStatistics.mockResolvedValue(response(mockServerStats))

  expect(await getGameServerStats()).toEqual({
    total: {
      playingCount: 12,
    },
    regions: {
      [REGION.Eu]: {
        [GAME_MODE.CRPGBattle]: {
          playingCount: 12,
        },
      },
    },
  })
})

import type { DeepPartial } from '@nuxt/ui/runtime/types/utils.js'
import type { GameServerStats } from '~~/generated/api'

import { describe, expect, it, vi } from 'vitest'

import { getGameServerStats } from '~/services/game-server-statistics-service'

const { getGameServerStatistics } = vi.hoisted(() => ({
  getGameServerStatistics: vi.fn(),
}))

vi.mock('#api/sdk.gen', () => ({
  getGameServerStatistics,
}))

describe('getGameServerStats', () => {
  it('filters out empty game modes and regions correctly', async () => {
    getGameServerStatistics.mockResolvedValueOnce({
      data: {
        total: {
          playingCount: 12,
        },
        regions: {
          Eu: {
            CRPGBattle: {
              playingCount: 12,
            },
            CRPGDTV: {
              playingCount: 0,
            },
          },
          Na: {
            CRPGDTV: {
              playingCount: 0,
            },
          },
        },
      } satisfies DeepPartial<GameServerStats>,
      errors: null,
    })

    expect(await getGameServerStats()).toEqual({
      total: {
        playingCount: 12,
      },
      regions: {
        Eu: {
          CRPGBattle: {
            playingCount: 12,
          },
        },
      },
    })
  })
})

import { afterAll, afterEach, beforeAll, it, expect, vi } from 'vitest'
import { setupServer } from 'msw/node'
import { http, HttpResponse } from 'msw'
import { $fetch } from 'ofetch'
import type { GameServerStats } from '~/models/game-server-stats'

// import { response } from '~/__mocks__/crpg-client'
import { GameMode } from '~/models/game-mode'
import { Region } from '~/models/region'
import { getGameServerStats } from '~/services/game-server-statistics-service'

const restHandlers = [
  http.get('/game-server-statistics', () => {
    return HttpResponse.json<GameServerStats>({
      total: {
        playingCount: 12,
      },
      regions: {
        [Region.Eu]: {
          [GameMode.Battle]: {
            playingCount: 12,
          },
          [GameMode.DTV]: {
            playingCount: 0,
          },
        },
        [Region.Na]: {
          [GameMode.DTV]: {
            playingCount: 0,
          },
        },
      },
    })
  }),
]

const server = setupServer(...restHandlers)

beforeAll(() => server.listen({ onUnhandledRequest: 'error' }))
afterAll(() => server.close())
afterEach(() => server.resetHandlers())

it('getGameServerStats', async () => {
  expect(await getGameServerStats($fetch.create({}))).toEqual({
    total: {
      playingCount: 12,
    },
    regions: {
      [Region.Eu]: {
        [GameMode.Battle]: {
          playingCount: 12,
        },
      },
    },
  })
})

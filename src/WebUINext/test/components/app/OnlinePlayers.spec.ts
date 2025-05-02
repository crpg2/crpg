import { mount } from '@vue/test-utils'

import type { GameServerStats } from '~/models/game-server-stats'

import { GameMode } from '~/models/game-mode'
import { Region } from '~/models/region'

import OnlinePlayers from '~/components/app/OnlinePlayers.vue'

const getWrapper = (props: { gameServerStats: GameServerStats | null, showLabel: boolean }) =>
  mount(OnlinePlayers, {
    global: {
      renderStubDefaultSlot: true,
      stubs: {
        VTooltip: {
          template: `<div>
                      <slot/>
                      <slot name="popper"/>
                    </div>`,
        },
      },
    },
    props,
    shallow: true,
  })

it('testing api returning gameStats valid', async () => {
  const wrapper = getWrapper({
    gameServerStats: {
      regions: {
        [Region.Eu]: {
          [GameMode.Battle]: {
            playingCount: 12,
          },
        },
      },
      total: { playingCount: 12 },
    },
    showLabel: false,
  })

  const onlinePlayersDiv = wrapper.find('[data-aq-online-players-count]')
  const regionsPopperContent = wrapper.find('[data-aq-region-stats]')

  expect(onlinePlayersDiv.text()).toEqual('12')
  expect(regionsPopperContent.exists()).toBeTruthy()
})

it('testing api returning gameStats null/Error', async () => {
  const wrapper = getWrapper({
    gameServerStats: null,
    showLabel: false,
  })

  const onlinePlayersDiv = wrapper.find('[data-aq-online-players-count]')
  const regionsPopperContent = wrapper.find('[data-aq-region-stats]')

  expect(onlinePlayersDiv.text()).toEqual('?')
  expect(regionsPopperContent.exists()).toBeFalsy()
})

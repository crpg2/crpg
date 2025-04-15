import { createTestingPinia } from '@pinia/testing'
import { flushPromises } from '@vue/test-utils'

import { mountWithRouter } from '~/__test__/unit/utils'
import { useUserStore } from '~/stores/user'

import Page from './create.vue'

const { NEW_CLAN_FORM, NEW_CLAN_ID } = vi.hoisted(() => ({
  NEW_CLAN_FORM: { name: 'My Little Pony', tag: 'mlp' },
  NEW_CLAN_ID: 2,
}))
const { mockedCreateClan } = vi.hoisted(() => ({
  mockedCreateClan: vi.fn().mockResolvedValue({ id: 2 }),
}))
vi.mock('~/services/clan-service', () => ({
  createClan: mockedCreateClan,
}))

const { mockedNotify } = vi.hoisted(() => ({ mockedNotify: vi.fn() }))
vi.mock('~/services/notification-service', () => ({
  notify: mockedNotify,
}))
const userStore = useUserStore(createTestingPinia())

const routes = [
  {
    component: Page,
    name: 'ClansCreate',
    path: '/clans-create',
  },
  {
    component: {
      template: `<div></div>`,
    },
    name: 'ClansId',
    path: '/clans/:id',
  },
]

const route = {
  name: 'ClansCreate',
}

const mountOptions = {
  global: {
    stubs: ['ClanForm'],
  },
}

it('emit - submit', async () => {
  const { router, wrapper } = await mountWithRouter(mountOptions, routes, route)
  const spyRouterReplace = vi.spyOn(router, 'replace')

  const clanFormComponent = wrapper.findComponent({ name: 'ClanForm' })

  await clanFormComponent.vm.$emit('submit', NEW_CLAN_FORM)
  await flushPromises()

  expect(mockedCreateClan).toBeCalledWith(NEW_CLAN_FORM)
  expect(userStore.fetchUser).toBeCalled()
  expect(spyRouterReplace).toBeCalledWith({
    name: 'ClansId',
    params: {
      id: NEW_CLAN_ID,
    },
  })
  expect(mockedNotify).toBeCalledWith('clan.create.notify.success')
})

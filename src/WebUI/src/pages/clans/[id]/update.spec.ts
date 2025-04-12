import { createTestingPinia } from '@pinia/testing'
import { flushPromises } from '@vue/test-utils'

import type { Clan } from '~/models/clan'
import type { User } from '~/models/user'

import { mountWithRouter } from '~/__test__/unit/utils'
import { useUserStore } from '~/stores/user'

import Page from './update.vue'

const CLAN_FORM = { name: 'My Little Pony New', tag: 'mlp' } as Omit<Clan, 'id'>
const { CLAN_ID } = vi.hoisted(() => ({ CLAN_ID: 1 }))
const { CLAN } = vi.hoisted(() => ({
  CLAN: {
    id: CLAN_ID,
    name: 'My Little Pony',
    tag: 'mlp',
  },
}))

const { mockedKickClanMember, mockedUpdateClan } = vi.hoisted(() => ({
  mockedKickClanMember: vi.fn(),
  mockedUpdateClan: vi.fn().mockResolvedValue({ id: CLAN_ID }),
}))
vi.mock('~/services/clan-service', () => ({
  kickClanMember: mockedKickClanMember,
  updateClan: mockedUpdateClan,
}))

const { mockedNotify } = vi.hoisted(() => ({ mockedNotify: vi.fn() }))
vi.mock('~/services/notification-service', () => ({
  notify: mockedNotify,
}))

const { mockedUseClan } = vi.hoisted(() => ({
  mockedUseClan: vi.fn().mockImplementation(() => ({
    clan: computed(() => CLAN),
    clanId: computed(() => CLAN_ID),
    loadClan: vi.fn(),
  })),
}))
vi.mock('~/composables/clan/use-clan', () => ({
  useClan: mockedUseClan,
}))

const { mockedUseClanMembers } = vi.hoisted(() => ({
  mockedUseClanMembers: vi.fn().mockImplementation(() => ({
    isLastMember: computed(() => false),
    loadClanMembers: vi.fn(),
  })),
}))
vi.mock('~/composables/clan/use-clan-members', () => ({
  useClanMembers: mockedUseClanMembers,
}))

const userStore = useUserStore(createTestingPinia())

const routes = [
  {
    component: Page,
    name: 'ClansIdUpdate',
    path: '/clans/:id/update',
    props: true,
  },
  {
    component: {
      template: `<div></div>`,
    },
    name: 'ClansId',
    path: '/clans/:id',
    props: true,
  },
  {
    component: {
      template: `<div></div>`,
    },
    name: 'Clans',
    path: '/clans',
  },
]
const route = {
  name: 'ClansIdUpdate',
  params: {
    id: String(CLAN_ID),
  },
}

const mountOptions = {
  global: {
    stubs: ['ClanForm', 'RouterLink'],
  },
}

beforeEach(() => {
  userStore.$reset()
})

it('emit - submit', async () => {
  const { router, wrapper } = await mountWithRouter(mountOptions, routes, route)
  const spyRouterReplace = vi.spyOn(router, 'replace')

  const clanFormComponent = wrapper.findComponent({ name: 'ClanForm' })

  await clanFormComponent.vm.$emit('submit', CLAN_FORM)
  await flushPromises()

  expect(mockedUpdateClan).toBeCalledWith(CLAN_ID, { ...CLAN, ...CLAN_FORM })
  expect(userStore.fetchUser).toBeCalled()
  expect(spyRouterReplace).toBeCalledWith({
    name: 'ClansId',
    params: {
      id: CLAN_ID,
    },
  })
  expect(mockedNotify).toBeCalledWith('clan.update.notify.success')
})

describe('delete clan', () => {
  it('it shouldn\'t be possible to delete a clan if the member is the only', async () => {
    const { wrapper } = await mountWithRouter(mountOptions, routes, route)

    expect(wrapper.find('[data-aq-clan-delete-required-message]').exists()).toStrictEqual(true)
    expect(wrapper.find('[data-aq-clan-delete-confirm-action-form]').exists()).toStrictEqual(false)
  })

  it('it should be possible to delete a clan if the member isn\'t the only', async () => {
    userStore.user = { id: 1 } as User

    mockedUseClanMembers.mockImplementationOnce(() => ({
      isLastMember: computed(() => true),
      loadClanMembers: vi.fn(),
    }))

    const { router, wrapper } = await mountWithRouter(mountOptions, routes, route)
    const spyRouterReplace = vi.spyOn(router, 'replace')

    const ConfirmActionForm = wrapper.findComponent({ name: 'ConfirmActionForm' })

    expect(ConfirmActionForm.exists()).toStrictEqual(true)
    expect(wrapper.find('[data-aq-clan-delete-required-message]').exists()).toStrictEqual(false)

    await ConfirmActionForm.vm.$emit('confirm')
    await flushPromises()

    expect(userStore.fetchUser).toBeCalled()
    expect(mockedNotify).toBeCalledWith('clan.delete.notify.success')
    expect(spyRouterReplace).toBeCalledWith({
      name: 'Clans',
    })
    expect(mockedKickClanMember).toBeCalledWith(1, 1)
  })
})

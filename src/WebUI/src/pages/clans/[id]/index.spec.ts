import { createTestingPinia } from '@pinia/testing'
import { flushPromises } from '@vue/test-utils'

import type { Clan } from '~/models/clan'
import type { User } from '~/models/user'

import { mountWithRouter } from '~/__test__/unit/utils'
import { useUserStore } from '~/stores/user'

import Page from './index.vue'

const {
  clanLeaderUserId: CLAN_LEADER_USER_ID,
  clanMemberUserId: CLAN_MEMBER_USER_ID,
  clanOfficerUserId: CLAN_OFFICER_USER_ID,
  noClanUserId: NO_CLAN_USER_ID,
} = vi.hoisted(() => ({
  clanLeaderUserId: 1,
  clanMemberUserId: 3,
  clanOfficerUserId: 3,
  noClanUserId: 4,
}))
const { clanMembers: CLAN_MEMBERS } = vi.hoisted(() => ({
  clanMembers: [
    {
      role: 'Leader',
      user: {
        avatar: 'test-avatar',
        id: CLAN_LEADER_USER_ID,
        name: 'Rarity',
        platform: 'Steam',
        platformUserId: '122341242562',
      },
    },
    {
      role: 'Officer',
      user: {
        avatar: 'test-avatar-2',
        id: CLAN_OFFICER_USER_ID,
        name: 'Fluttershy',
        platform: 'Steam',
        platformUserId: '121313',
      },
    },
    {
      role: 'Member',
      user: {
        avatar: 'test-avatar-3',
        id: CLAN_MEMBER_USER_ID,
        name: 'Applejack',
        platform: 'Steam',
        platformUserId: '121313',
      },
    },
  ],
}))

const {
  clanLeader: CLAN_LEADER,
  //   clanMember: CLAN_MEMBER,
  clanOfficer: CLAN_OFFICER,
} = vi.hoisted(() => ({
  clanLeader: CLAN_MEMBERS.find(m => m.user.id === CLAN_LEADER_USER_ID)!,
  //   clanMember: CLAN_MEMBERS.find(m => m.user.id === CLAN_MEMBER_USER_ID)!,
  clanOfficer: CLAN_MEMBERS.find(m => m.user.id === CLAN_OFFICER_USER_ID)!,
}))

const {
  mockedCanKickMemberValidate,
  mockedCanManageApplicationsValidate,
  mockedCanUpdateClanValidate,
  mockedCanUpdateMemberValidate,
  mockedGetClanMember,
  mockedGetClanMembers,
  mockedInviteToClan,
  mockedKickClanMember,
  mockedUpdateClanMember,
} = vi.hoisted(() => ({
  mockedCanKickMemberValidate: vi.fn().mockReturnValue(false),
  mockedCanManageApplicationsValidate: vi.fn().mockReturnValue(false),
  mockedCanUpdateClanValidate: vi.fn().mockReturnValue(false),
  mockedCanUpdateMemberValidate: vi.fn().mockReturnValue(false),
  mockedGetClanMember: vi.fn().mockReturnValue(null),
  mockedGetClanMembers: vi.fn().mockResolvedValue(CLAN_MEMBERS),
  mockedInviteToClan: vi.fn().mockReturnValue(false),
  mockedKickClanMember: vi.fn().mockReturnValue(false),
  mockedUpdateClanMember: vi.fn().mockReturnValue(false),
}))
vi.mock('~/services/clan-service', () => ({
  canKickMemberValidate: mockedCanKickMemberValidate,
  canManageApplicationsValidate: mockedCanManageApplicationsValidate,
  canUpdateClanValidate: mockedCanUpdateClanValidate,
  canUpdateMemberValidate: mockedCanUpdateMemberValidate,
  getClanMember: mockedGetClanMember,
  getClanMembers: mockedGetClanMembers,
  inviteToClan: mockedInviteToClan,
  kickClanMember: mockedKickClanMember,
  updateClanMember: mockedUpdateClanMember,
}))

const { mockedNotify } = vi.hoisted(() => ({ mockedNotify: vi.fn() }))
vi.mock('~/services/notification-service', () => ({
  notify: mockedNotify,
}))

const {
  clan: CLAN,
  clanId: CLAN_ID,
  mockedLoadClan,
} = vi.hoisted(() => ({
  clan: {
    bannerKey: '123456',
    discord: 'https://discord.gg',
    id: 1,
    name: 'My Little Pony',
    primaryColor: '4278190080',
    region: 'Eu',
    secondaryColor: '4278190080',
    tag: 'mlp',
  },
  clanId: 1,
  mockedLoadClan: vi.fn(),
}))
const { mockedUseClan } = vi.hoisted(() => ({
  mockedUseClan: vi.fn().mockImplementation(() => ({
    clan: computed(() => CLAN),
    clanId: computed(() => CLAN_ID),
    loadClan: mockedLoadClan,
  })),
}))
vi.mock('~/composables/clan/use-clan', () => ({
  useClan: mockedUseClan,
}))

const { mockedUseClanApplications } = vi.hoisted(() => ({
  mockedUseClanApplications: vi.fn().mockImplementation(() => ({
    applicationsCount: computed(() => 2),
    loadClanApplications: vi.fn(),
  })),
}))
vi.mock('~/composables/clan/use-clan-applications', () => ({
  useClanApplications: mockedUseClanApplications,
}))

const { mockedUsePagination } = vi.hoisted(() => ({
  mockedUsePagination: vi.fn().mockImplementation(() => ({
    pageModel: ref(1),
    perPage: 2,
  })),
}))
vi.mock('~/composables/use-pagination', () => ({
  usePagination: mockedUsePagination,
}))

const mountOptions = {
  global: {
    plugins: [createTestingPinia()],
    renderStubDefaultSlot: true,
    stubs: {
      ClanMemberDetail: true,
      ClanTagIcon: true,
      ResultNotFound: true,
      UserMedia: true,
    },
  },
}

const userStore = useUserStore()

const routes = [
  {
    component: Page,
    name: 'ClansId',
    path: '/clans/:id',
    props: true,
  },
]
const route = {
  name: 'ClansId',
  params: {
    id: CLAN_ID,
  },
}

beforeEach(() => {
  userStore.$reset()
  userStore.user = {
    id: NO_CLAN_USER_ID,
  } as User
  userStore.clan = null
})

describe('common', () => {
  it('should visible clan`s info fields', async () => {
    const { wrapper } = await mountWithRouter(mountOptions, routes, route)

    expect(wrapper.find('[data-aq-clan-info="name"]').text()).toEqual(CLAN.name)
    expect(wrapper.find('[data-aq-clan-info="tag"]').text()).toEqual(CLAN.tag)
    expect(wrapper.find('[data-aq-clan-info="region"]').text()).toEqual('region.Eu')
    expect(wrapper.find('[data-aq-clan-info="member-count"]').text()).toEqual('3')
    expect(wrapper.find('[data-aq-clan-info="description"]').exists()).toBeFalsy()
  })
})

describe('user not in a clan', () => {
  it('should visible "Apply to join" btn', async () => {
    const { wrapper } = await mountWithRouter(mountOptions, routes, route)

    expect(wrapper.find('[data-aq-clan-action="apply-to-join"]').exists()).toBeTruthy()
    expect(wrapper.find('[data-aq-clan-action="application-sent"]').exists()).toBeFalsy()
    expect(wrapper.find('[data-aq-clan-action="clan-application"]').exists()).toBeFalsy()
    expect(wrapper.find('[data-aq-clan-action="clan-update"]').exists()).toBeFalsy()

    await wrapper.find('[data-aq-clan-action="apply-to-join"]').trigger('click')

    expect(wrapper.find('[data-aq-clan-action="apply-to-join"]').exists()).toBeFalsy()
    expect(wrapper.find('[data-aq-clan-action="application-sent"]').exists()).toBeTruthy()
  })

  it('try to open member detail modal', async () => {
    const { wrapper } = await mountWithRouter(mountOptions, routes, route)

    const detailModal = wrapper.find('[data-aq-clan-member-detail-modal]')
    const rows = wrapper.findAll('tr')

    await rows.at(1)!.trigger('click')
    expect(detailModal.attributes('shown')).toEqual('false')

    await rows.at(2)!.trigger('click')
    expect(detailModal.attributes('shown')).toEqual('false')
  })
})

describe('user in another clan', () => {
  beforeEach(() => {})

  it('should`t visible "Apply to join" btn', async () => {
    userStore.clan = {
      id: 112,
    } as Clan
    mockedGetClanMember.mockReturnValue(null)
    mockedCanManageApplicationsValidate.mockReturnValue(false)
    mockedCanUpdateClanValidate.mockReturnValue(false)
    mockedCanUpdateMemberValidate.mockReturnValue(false)
    mockedCanKickMemberValidate.mockReturnValue(false)

    const { wrapper } = await mountWithRouter(mountOptions, routes, route)

    expect(wrapper.find('[data-aq-clan-action="apply-to-join"]').exists()).toBeFalsy()
    expect(wrapper.find('[data-aq-clan-action="application-sent"]').exists()).toBeFalsy()

    expect(wrapper.find('[data-aq-clan-action="clan-application"]').exists()).toBeFalsy()
    expect(wrapper.find('[data-aq-clan-action="clan-update"]').exists()).toBeFalsy()
  })

  it('try to open member detail modal', async () => {
    const { wrapper } = await mountWithRouter(mountOptions, routes, route)

    const detailModal = wrapper.find('[data-aq-clan-member-detail-modal]')
    const rows = wrapper.findAll('tr')

    await rows.at(1)!.trigger('click')
    expect(detailModal.attributes('shown')).toEqual('false')

    await rows.at(2)!.trigger('click')
    expect(detailModal.attributes('shown')).toEqual('false')
  })
})

describe('user in the clan', () => {
  describe('leader', () => {
    beforeEach(() => {
      mockedGetClanMember.mockReturnValue(CLAN_LEADER)
      mockedCanManageApplicationsValidate.mockReturnValue(true)
      mockedCanUpdateClanValidate.mockReturnValue(true)
      mockedCanUpdateMemberValidate.mockReturnValue(true)
      mockedCanKickMemberValidate.mockReturnValue(true)
    })

    it('should visible "Clan applications" & "Clan Update" btns', async () => {
      mockedCanKickMemberValidate.mockReturnValue(false)
      const { wrapper } = await mountWithRouter(mountOptions, routes, route)

      expect(wrapper.find('[data-aq-clan-action="clan-application"]').exists()).toBeTruthy()
      expect(wrapper.find('[data-aq-clan-action="clan-update"]').exists()).toBeTruthy()

      expect(wrapper.find('[data-aq-clan-action="apply-to-join"]').exists()).toBeFalsy()
      expect(wrapper.find('[data-aq-clan-action="leave-clan"]').exists()).toBeFalsy()
    })

    it('open member detail modal', async () => {
      const { wrapper } = await mountWithRouter(mountOptions, routes, route)

      const detailModal = wrapper.find('[data-aq-clan-member-detail-modal]')
      const rows = wrapper.findAll('tr')

      await rows.at(1)!.trigger('click')
      expect(detailModal.attributes('shown')).toEqual('false') // cannot open a detailed window of self

      await rows.at(2)!.trigger('click')
      expect(detailModal.attributes('shown')).toEqual('true')

      const ClanMemberDetail = wrapper.findComponent({ name: 'ClanMemberDetail' })

      expect(ClanMemberDetail.props()).toEqual({
        canKick: true,
        canUpdate: true,
        member: CLAN_MEMBERS[1],
      })
    })

    it('kick some member', async () => {
      const { wrapper } = await mountWithRouter(mountOptions, routes, route)

      const rows = wrapper.findAll('tr')

      await rows.at(2)!.trigger('click')

      wrapper.findComponent({ name: 'ClanMemberDetail' }).vm.$emit('kick')
      await flushPromises()

      expect(mockedKickClanMember).toBeCalledWith(CLAN_ID, CLAN_MEMBERS[1].user.id)
      expect(mockedGetClanMembers).toHaveBeenNthCalledWith(2, CLAN_ID)
      expect(userStore.fetchUser).not.toHaveBeenCalled()
      expect(mockedNotify).toBeCalledWith('clan.member.kick.notify.success')
    })

    it('update some member', async () => {
      const NEW_ROLE = 'Member'

      const { wrapper } = await mountWithRouter(mountOptions, routes, route)
      const rows = wrapper.findAll('tr')

      await rows.at(2)!.trigger('click')

      wrapper.findComponent({ name: 'ClanMemberDetail' }).vm.$emit('update', NEW_ROLE)
      await flushPromises()

      expect(mockedUpdateClanMember).toBeCalledWith(CLAN_ID, CLAN_MEMBERS[1].user.id, NEW_ROLE)
      expect(mockedGetClanMembers).toHaveBeenNthCalledWith(2, CLAN_ID)
      expect(mockedNotify).toBeCalledWith('clan.member.update.notify.success')
    })
  })

  describe('officer', () => {
    beforeEach(() => {
      mockedGetClanMember.mockReturnValue(CLAN_OFFICER)
      mockedCanManageApplicationsValidate.mockReturnValue(true)
      mockedCanUpdateClanValidate.mockReturnValue(false)
      mockedCanUpdateMemberValidate.mockReturnValue(true)
      mockedCanKickMemberValidate.mockReturnValue(true)
    })

    it('leave from clan', async () => {
      const { wrapper } = await mountWithRouter(mountOptions, routes, route)

      expect(wrapper.find('[data-aq-clan-action="leave-clan"]').exists()).toBeTruthy()

      await wrapper.find('[data-aq-clan-action="leave-clan-confirm"]').trigger('click')
      await flushPromises()

      expect(mockedKickClanMember).toBeCalledWith(CLAN_ID, CLAN_MEMBERS[1].user.id)
      expect(mockedGetClanMembers).toHaveBeenNthCalledWith(2, CLAN_ID)
      expect(userStore.fetchUser).toHaveBeenCalled()
      expect(mockedNotify).toBeCalledWith('clan.member.leave.notify.success')
    })
  })
})

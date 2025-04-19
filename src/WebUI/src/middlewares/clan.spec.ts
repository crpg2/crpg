import { createTestingPinia } from '@pinia/testing'

import type { Clan } from '~/models/clan'

import { getRoute, next } from '~/__mocks__/router'
import { ClanMemberRole } from '~/models/clan'
import { useUserStore } from '~/stores/user'

import {
  canManageApplications,
  canUpdateClan,
  canUseClanArmory,
  clanExistValidate,
  clanIdParamValidate,
} from './clan'

const userStore = useUserStore(createTestingPinia())

beforeEach(() => {
  userStore.$reset()
})

describe('clan id format validate', () => {
  it('404', () => {
    const to = getRoute({
      name: 'ClansId',
      params: {
        id: 'abc',
      },
      path: '/clans/:id',
    })

    const result = clanIdParamValidate(to, getRoute(), next)

    expect(result).toEqual({ name: 'PageNotFound' })
  })

  it('ok', () => {
    const to = getRoute({
      name: 'ClansId',
      params: {
        id: '1',
      },
      path: '/clans/:id',
    })

    const result = clanIdParamValidate(to, getRoute(), next)

    expect(result).toStrictEqual(true)
  })
})

describe('clan exist validate', () => {
  it('user already have a clan', async () => {
    const CLAN_ID = 1
    userStore.clan = { id: CLAN_ID } as Clan

    const result = await clanExistValidate(getRoute(), getRoute(), next)

    expect(result).toEqual({ name: 'ClansId', params: { id: String(CLAN_ID) } })
  })

  it('user already have a clan TODO: change test title', async () => {
    const result = await clanExistValidate(getRoute(), getRoute(), next)

    expect(result).toStrictEqual(true)
  })
})

describe('can update clan', () => {
  const CLAN_ID = 1
  const to = getRoute({
    name: 'ClansIdUpdate',
    params: {
      id: String(CLAN_ID),
    },
    path: '/clans/:id/update',
  })

  it('officer', async () => {
    userStore.clan = { id: CLAN_ID } as Clan
    userStore.clanMemberRole = ClanMemberRole.Officer

    const result = await canUpdateClan(to, getRoute(), next)

    expect(result).toEqual({ name: 'Clans' })
  })

  it('leader', async () => {
    userStore.clan = { id: CLAN_ID } as Clan
    userStore.clanMemberRole = ClanMemberRole.Leader

    const result = await canUpdateClan(to, getRoute(), next)

    expect(result).toStrictEqual(true)
  })
})

describe('can manage clan application', () => {
  const CLAN_ID = 1
  const to = getRoute({
    name: 'ClansIdApplications',
    params: {
      id: String(CLAN_ID),
    },
    path: '/clans/:id/application',
  })

  it('member', async () => {
    const CLAN_ID = 1
    userStore.clan = { id: CLAN_ID } as Clan
    userStore.clanMemberRole = ClanMemberRole.Member

    const result = await canManageApplications(to, getRoute(), next)

    expect(result).toEqual({ name: 'Clans' })
  })

  it('officer', async () => {
    const CLAN_ID = 1
    userStore.clan = { id: CLAN_ID } as Clan
    userStore.clanMemberRole = ClanMemberRole.Leader

    const result = await canManageApplications(to, getRoute(), next)

    expect(result).toStrictEqual(true)
  })
})

describe('can manage clan armory', () => {
  const CLAN_ID = 1
  const to = getRoute({
    name: 'ClansIdArmory',
    params: {
      id: String(CLAN_ID),
    },
    path: '/clans/:id/armory',
  })

  it('member', async () => {
    const CLAN_ID = 1
    userStore.clan = { id: CLAN_ID } as Clan
    userStore.clanMemberRole = ClanMemberRole.Member

    const result = await canUseClanArmory(to, getRoute(), next)

    expect(result).toStrictEqual(true)
  })

  it('not member', async () => {
    userStore.clan = { id: 2 } as Clan
    userStore.clanMemberRole = ClanMemberRole.Leader

    const result = await canManageApplications(to, getRoute(), next)

    expect(result).toEqual({ name: 'Clans' })
  })
})

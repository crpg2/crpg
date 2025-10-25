import { describe, expect, it, vi } from 'vitest'

import type { User, UserPublic } from '~/models/user'

import { mapUserToUserPublic } from '../user-service'

const {
  mockedDeleteUsersSelf,
  mockedDeleteUsersSelfItemsById,
  mockedDeleteUsersSelfNotificationsById,
  mockedDeleteUsersSelfNotificationsDeleteAll,
  mockedGetUsersSelf,
  mockedGetUsersSelfItems,
  mockedGetUsersSelfNotifications,
  mockedGetUsersSelfRestriction,
  mockedPostUsersSelfItems,
  mockedPutUsersSelfItemsByIdReforge,
  mockedPutUsersSelfItemsByIdRepair,
  mockedPutUsersSelfItemsByIdUpgrade,
  mockedPutUsersSelfNotificationsById,
  mockedPutUsersSelfNotificationsReadAll,
} = vi.hoisted(() => ({
  mockedDeleteUsersSelf: vi.fn(),
  mockedDeleteUsersSelfItemsById: vi.fn(),
  mockedDeleteUsersSelfNotificationsById: vi.fn(),
  mockedDeleteUsersSelfNotificationsDeleteAll: vi.fn(),
  mockedGetUsersSelf: vi.fn(),
  mockedGetUsersSelfItems: vi.fn(),
  mockedGetUsersSelfNotifications: vi.fn(),
  mockedGetUsersSelfRestriction: vi.fn(),
  mockedPostUsersSelfItems: vi.fn(),
  mockedPutUsersSelfItemsByIdReforge: vi.fn(),
  mockedPutUsersSelfItemsByIdRepair: vi.fn(),
  mockedPutUsersSelfItemsByIdUpgrade: vi.fn(),
  mockedPutUsersSelfNotificationsById: vi.fn(),
  mockedPutUsersSelfNotificationsReadAll: vi.fn(),
}))

vi.mock('#api/sdk.gen', () => ({
  deleteUsersSelf: mockedDeleteUsersSelf,
  deleteUsersSelfItemsById: mockedDeleteUsersSelfItemsById,
  deleteUsersSelfNotificationsById: mockedDeleteUsersSelfNotificationsById,
  deleteUsersSelfNotificationsDeleteAll: mockedDeleteUsersSelfNotificationsDeleteAll,
  getUsersSelf: mockedGetUsersSelf,
  getUsersSelfItems: mockedGetUsersSelfItems,
  getUsersSelfNotifications: mockedGetUsersSelfNotifications,
  getUsersSelfRestriction: mockedGetUsersSelfRestriction,
  postUsersSelfItems: mockedPostUsersSelfItems,
  putUsersSelfItemsByIdReforge: mockedPutUsersSelfItemsByIdReforge,
  putUsersSelfItemsByIdRepair: mockedPutUsersSelfItemsByIdRepair,
  putUsersSelfItemsByIdUpgrade: mockedPutUsersSelfItemsByIdUpgrade,
  putUsersSelfNotificationsById: mockedPutUsersSelfNotificationsById,
  putUsersSelfNotificationsReadAll: mockedPutUsersSelfNotificationsReadAll,
}))

describe('user service', () => {
  it('returns only public fields when user contains additional private data', () => {
    expect(mapUserToUserPublic({
      id: 1,
      platform: 'Steam',
      platformUserId: '123',
      name: '',
      region: 'Eu',
      avatar: '',
      clanMembership: null,
      activeCharacterId: 1,
      gold: 100,
    } as User)).toEqual({
      id: 1,
      platform: 'Steam',
      platformUserId: '123',
      name: '',
      region: 'Eu',
      avatar: '',
      clanMembership: null,
    } satisfies UserPublic)
  })
})

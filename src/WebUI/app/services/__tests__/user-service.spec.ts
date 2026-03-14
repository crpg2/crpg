import { describe, expect, it, vi } from 'vitest'

import type { User, UserPublic } from '~/models/user'

import { ITEM_SLOT } from '~/models/item'

import {
  createUserItemPreset,
  deleteUserItemPreset,
  getUserItemPresets,
  mapUserToUserPublic,
} from '../user-service'

const {
  mockedDeleteUsersSelf,
  mockedDeleteUsersSelfItemPresetsById,
  mockedDeleteUsersSelfItemsById,
  mockedDeleteUsersSelfNotificationsById,
  mockedDeleteUsersSelfNotificationsDeleteAll,
  mockedGetUsersSelf,
  mockedGetUsersSelfItemPresets,
  mockedGetUsersSelfItems,
  mockedGetUsersSelfNotifications,
  mockedGetUsersSelfRestriction,
  mockedPostUsersSelfItemPresets,
  mockedPostUsersSelfItems,
  mockedPutUsersSelfItemsByIdReforge,
  mockedPutUsersSelfItemsByIdRepair,
  mockedPutUsersSelfItemsByIdUpgrade,
  mockedPutUsersSelfNotificationsById,
  mockedPutUsersSelfNotificationsReadAll,
} = vi.hoisted(() => ({
  mockedDeleteUsersSelf: vi.fn(),
  mockedDeleteUsersSelfItemPresetsById: vi.fn(),
  mockedDeleteUsersSelfItemsById: vi.fn(),
  mockedDeleteUsersSelfNotificationsById: vi.fn(),
  mockedDeleteUsersSelfNotificationsDeleteAll: vi.fn(),
  mockedGetUsersSelf: vi.fn(),
  mockedGetUsersSelfItemPresets: vi.fn(),
  mockedGetUsersSelfItems: vi.fn(),
  mockedGetUsersSelfNotifications: vi.fn(),
  mockedGetUsersSelfRestriction: vi.fn(),
  mockedPostUsersSelfItemPresets: vi.fn(),
  mockedPostUsersSelfItems: vi.fn(),
  mockedPutUsersSelfItemsByIdReforge: vi.fn(),
  mockedPutUsersSelfItemsByIdRepair: vi.fn(),
  mockedPutUsersSelfItemsByIdUpgrade: vi.fn(),
  mockedPutUsersSelfNotificationsById: vi.fn(),
  mockedPutUsersSelfNotificationsReadAll: vi.fn(),
}))

vi.mock('#api/sdk.gen', () => ({
  deleteUsersSelf: mockedDeleteUsersSelf,
  deleteUsersSelfItemPresetsById: mockedDeleteUsersSelfItemPresetsById,
  deleteUsersSelfItemsById: mockedDeleteUsersSelfItemsById,
  deleteUsersSelfNotificationsById: mockedDeleteUsersSelfNotificationsById,
  deleteUsersSelfNotificationsDeleteAll: mockedDeleteUsersSelfNotificationsDeleteAll,
  getUsersSelf: mockedGetUsersSelf,
  getUsersSelfItemPresets: mockedGetUsersSelfItemPresets,
  getUsersSelfItems: mockedGetUsersSelfItems,
  getUsersSelfNotifications: mockedGetUsersSelfNotifications,
  getUsersSelfRestriction: mockedGetUsersSelfRestriction,
  postUsersSelfItemPresets: mockedPostUsersSelfItemPresets,
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

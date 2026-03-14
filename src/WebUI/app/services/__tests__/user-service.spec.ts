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

  it('maps item presets and normalizes undefined itemId to null', async () => {
    mockedGetUsersSelfItemPresets.mockResolvedValueOnce({
      data: [
        {
          id: 10,
          name: 'Preset A',
          slots: [
            { slot: 'Head', itemId: undefined },
            { slot: 'Weapon0', itemId: 'itm_1' },
          ],
        },
      ],
    })

    const result = await getUserItemPresets()

    expect(mockedGetUsersSelfItemPresets).toHaveBeenCalledWith({})
    expect(result).toEqual([
      {
        id: 10,
        name: 'Preset A',
        slots: [
          { slot: 'Head', itemId: null },
          { slot: 'Weapon0', itemId: 'itm_1' },
        ],
      },
    ])
  })

  it('creates preset via sdk and returns mapped preset', async () => {
    const payload = {
      name: 'Preset C',
      slots: [{ slot: ITEM_SLOT.Leg, itemId: 'itm_2' }],
    }

    mockedPostUsersSelfItemPresets.mockResolvedValueOnce({
      data: {
        id: 12,
        name: payload.name,
        slots: [{ slot: ITEM_SLOT.Leg, itemId: 'itm_2' }],
      },
    })

    const result = await createUserItemPreset(payload)

    expect(mockedPostUsersSelfItemPresets).toHaveBeenCalledWith({ body: payload })
    expect(result.id).toBe(12)
  })

  it('deletes preset by id', async () => {
    await deleteUserItemPreset(14)
    expect(mockedDeleteUsersSelfItemPresetsById).toHaveBeenCalledWith({ path: { id: 14 } })
  })
})

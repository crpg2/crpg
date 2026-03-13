import { describe, expect, it, vi } from 'vitest'

import type { User, UserPublic } from '~/models/user'

import { ITEM_SLOT } from '~/models/item'

import {
  createUserItemPreset,
  deleteUserItemPreset,
  getUserItemPreset,
  getUserItemPresets,
  mapUserToUserPublic,
  updateUserItemPreset,
} from '../user-service'

const {
  mockedDeleteUsersSelf,
  mockedDeleteUsersSelfItemPresetsById,
  mockedDeleteUsersSelfItemsById,
  mockedDeleteUsersSelfNotificationsById,
  mockedDeleteUsersSelfNotificationsDeleteAll,
  mockedGetUsersSelf,
  mockedGetUsersSelfItemPresets,
  mockedGetUsersSelfItemPresetsById,
  mockedGetUsersSelfItems,
  mockedGetUsersSelfNotifications,
  mockedGetUsersSelfRestriction,
  mockedPostUsersSelfItemPresets,
  mockedPostUsersSelfItems,
  mockedPutUsersSelfItemPresetsById,
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
  mockedGetUsersSelfItemPresetsById: vi.fn(),
  mockedGetUsersSelfItems: vi.fn(),
  mockedGetUsersSelfNotifications: vi.fn(),
  mockedGetUsersSelfRestriction: vi.fn(),
  mockedPostUsersSelfItemPresets: vi.fn(),
  mockedPostUsersSelfItems: vi.fn(),
  mockedPutUsersSelfItemPresetsById: vi.fn(),
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
  getUsersSelfItemPresetsById: mockedGetUsersSelfItemPresetsById,
  getUsersSelfItems: mockedGetUsersSelfItems,
  getUsersSelfNotifications: mockedGetUsersSelfNotifications,
  getUsersSelfRestriction: mockedGetUsersSelfRestriction,
  postUsersSelfItemPresets: mockedPostUsersSelfItemPresets,
  postUsersSelfItems: mockedPostUsersSelfItems,
  putUsersSelfItemPresetsById: mockedPutUsersSelfItemPresetsById,
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

  it('gets preset by id and normalizes slots', async () => {
    mockedGetUsersSelfItemPresetsById.mockResolvedValueOnce({
      data: {
        id: 11,
        name: 'Preset B',
        slots: [{ slot: 'Body', itemId: undefined }],
      },
    })

    const result = await getUserItemPreset(11)

    expect(mockedGetUsersSelfItemPresetsById).toHaveBeenCalledWith({ path: { id: 11 } })
    expect(result).toEqual({
      id: 11,
      name: 'Preset B',
      slots: [{ slot: 'Body', itemId: null }],
    })
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

  it('updates preset via sdk and returns mapped preset', async () => {
    const payload = {
      name: 'Preset D',
      slots: [{ slot: ITEM_SLOT.Weapon1, itemId: null }],
    }

    mockedPutUsersSelfItemPresetsById.mockResolvedValueOnce({
      data: {
        id: 13,
        name: payload.name,
        slots: [{ slot: ITEM_SLOT.Weapon1, itemId: undefined }],
      },
    })

    const result = await updateUserItemPreset(13, payload)

    expect(mockedPutUsersSelfItemPresetsById).toHaveBeenCalledWith({
      path: { id: 13 },
      body: payload,
    })
    const [firstSlot] = result.slots
    expect(firstSlot).toBeDefined()
    expect(firstSlot?.itemId).toBeNull()
  })

  it('deletes preset by id', async () => {
    await deleteUserItemPreset(14)
    expect(mockedDeleteUsersSelfItemPresetsById).toHaveBeenCalledWith({ path: { id: 14 } })
  })
})

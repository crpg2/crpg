import { mockDelete, mockGet, mockPost, mockPut } from 'vi-fetch'

import type { User, UserItem, UserPublic } from '~/models/user'

import { response } from '~/__mocks__/crpg-client'
import mockUserItems from '~/__mocks__/user-items.json'
import mockUserPublic from '~/__mocks__/user-public.json'
import mockUser from '~/__mocks__/user.json'

import {
  buyUserItem,
  deleteUser,
  extractItemFromUserItem,
  getUser,
  getUserById,
  getUserItems,
  getUserRestrictions,
  groupUserItemsByType,
  mapUserToUserPublic,
  repairUserItem,
  searchUser,
  sellUserItem,
} from './users-service'

vi.mock('~/services/auth-service', () => ({
  getToken: vi.fn().mockResolvedValue('mockedToken'),
}))

const { mockedMapRestrictions } = vi.hoisted(() => ({
  mockedMapRestrictions: vi.fn(),
}))
vi.mock('~/services/restriction-service', () => ({
  mapRestrictions: mockedMapRestrictions,
}))

it('getUser', async () => {
  mockGet('/users/self').willResolve(response<User>(mockUser as User))

  expect(await getUser()).toEqual(mockUser)
})

it('getUserById', async () => {
  mockGet('/users/123').willResolve(response<UserPublic>(mockUserPublic as UserPublic))

  expect(await getUserById(123)).toEqual(mockUserPublic)
})

it('deleteUser', async () => {
  mockDelete('/users/self').willResolve(null, 204)

  expect(await deleteUser()).toEqual(null)
})

it('extractItemFromUserItem', () => {
  expect(
    extractItemFromUserItem([
      {
        id: 1,
        item: {
          id: '123',
        },
      } as UserItem,
    ]),
  ).toEqual([
    {
      id: '123',
    },
  ])
})

it('getUserItems', async () => {
  mockGet('/users/self/items').willResolve(response(mockUserItems))

  expect(await getUserItems()).toEqual(
    mockUserItems.map(ui => ({ ...ui, createdAt: new Date(ui.createdAt) })),
  )
})

it('buyUserItem', async () => {
  const mock = mockPost('/users/self/items').willResolve(response(mockUserItems[0]))

  expect(await buyUserItem('123')).toEqual({
    ...mockUserItems[0],
    createdAt: new Date(mockUserItems[0].createdAt),
  })

  expect(mock).toHaveFetchedWithBody({ itemId: '123' })
})

it('repairUserItem', async () => {
  mockPut('/users/self/items/123/repair').willResolve(response(mockUserItems[0]))

  expect(await repairUserItem(123)).toEqual({
    ...mockUserItems[0],
    createdAt: new Date(mockUserItems[0].createdAt),
  })
})

it('sellUserItem', async () => {
  mockDelete('/users/self/items/123').willResolve(null, 204)

  expect(await sellUserItem(123)).toEqual(null)
})

describe('userItems: filterBy, sortBy, groupBy', () => {
  const userItems = [
    {
      id: 1,
      item: {
        name: 'Fluttershy',
        type: 'HeadArmor',
      },
    },
    {
      id: 2,
      item: {
        name: 'Rainbow Dash',
        type: 'Thrown',
      },
    },
    {
      id: 3,
      item: {
        name: 'Rarity',
        type: 'Thrown',
      },
    },
  ] as UserItem[]

  it('groupUserItemsByType', () => {
    expect(groupUserItemsByType(userItems)).toMatchObject([
      {
        items: [
          {
            id: 1,
          },
        ],
        type: 'HeadArmor',
      },
      {
        items: [
          {
            id: 2,
          },
          {
            id: 3,
          },
        ],
        type: 'Thrown',
      },
    ])
  })
})

it('getUserRestrictions', async () => {
  const USER_ID = 123
  const USER_RESTRICTIONS = [{ id: 1 }]
  mockGet(`/users/${USER_ID}/restrictions`).willResolve(response(USER_RESTRICTIONS))

  await getUserRestrictions(USER_ID)

  expect(mockedMapRestrictions).toBeCalledWith(USER_RESTRICTIONS)
})

it('searchUser', async () => {
  const NAME = 'Fluttershy'
  const USER = {
    id: 1,
    name: 'Fluttershy',
    platform: 'Steam',
  }

  mockGet(`/users/search/?name=${NAME}`, true).willResolve(response(USER))

  expect(await searchUser({ name: NAME })).toEqual(USER)
})

it('mapUserToUserPublic', async () => {
  expect(mapUserToUserPublic(mockUser as User)).toEqual(mockUserPublic as UserPublic)
})

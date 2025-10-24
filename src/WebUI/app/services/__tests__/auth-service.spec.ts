// @vitest-environment jsdom
import { describe, expect, it, vi } from 'vitest'

import { PLATFORM } from '~/models/platform'

import { getToken, getUser, login, logout } from '../auth-service'

const { mockedGetUser, mockedSigninRedirect, mockedSignoutRedirect } = vi.hoisted(() => ({
  mockedGetUser: vi.fn(),
  mockedSigninRedirect: vi.fn(),
  mockedSignoutRedirect: vi.fn(),
}))

vi.mock('oidc-client-ts', () => ({
  UserManager: vi.fn().mockImplementation(() => ({
    getUser: mockedGetUser,
    signinRedirect: mockedSigninRedirect,
    signoutRedirect: mockedSignoutRedirect,
  })),
  WebStorageStateStore: vi.fn(),
}))

it('getUser', async () => {
  mockedGetUser.mockResolvedValueOnce({ foo: 'bar' })
  expect(await getUser()).toEqual({ foo: 'bar' })
  expect(mockedGetUser).toHaveBeenCalled()
})

it('login', async () => {
  await login(PLATFORM.Steam)
  expect(mockedSigninRedirect).toHaveBeenCalledWith({
    extraQueryParams: {
      identity_provider: 'Steam',
    },
  })
})

it('logout', async () => {
  await logout()
  expect(mockedSignoutRedirect).toHaveBeenCalled()
})

it('getToken', async () => {
  mockedGetUser.mockResolvedValueOnce({ access_token: 'access_token' })

  const token = await getToken()

  expect(token).toEqual('access_token')
  expect(mockedGetUser).toHaveBeenCalled()
})

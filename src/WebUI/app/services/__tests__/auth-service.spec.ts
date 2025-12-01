// @vitest-environment jsdom
import { expect, it, vi } from 'vitest'

import { PLATFORM } from '~/models/platform'

import { getToken, getUser, login, logout, signinCallback } from '../auth-service'

const {
  mockedGetUser,
  mockedSigninRedirect,
  mockedSignoutRedirect,
  mockedSigninCallback,
} = vi.hoisted(() => ({
  mockedGetUser: vi.fn(),
  mockedSigninRedirect: vi.fn(),
  mockedSignoutRedirect: vi.fn(),
  mockedSigninCallback: vi.fn(),
}))

vi.mock('oidc-client-ts', () => ({
  UserManager: vi.fn(function (this: any) {
    this.getUser = mockedGetUser
    this.signinRedirect = mockedSigninRedirect
    this.signoutRedirect = mockedSignoutRedirect
    this.signinCallback = mockedSigninCallback
  }),
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

it('signinCallback', async () => {
  await signinCallback()
  expect(mockedSigninCallback).toHaveBeenCalled()
})

it('getToken', async () => {
  mockedGetUser.mockResolvedValueOnce({ access_token: 'access_token' })
  expect(await getToken()).toEqual('access_token')
  expect(mockedGetUser).toHaveBeenCalled()
})

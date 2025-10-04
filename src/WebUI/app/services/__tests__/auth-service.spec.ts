// @vitest-environment jsdom
import { describe, expect, it, vi } from 'vitest'

import { PLATFORM } from '~/models/platform'

import { getToken, getUser, login, logout, parseJwt } from '../auth-service'

const { mockedGetUser, mockedSigninRedirect, mockedSignoutRedirect } = vi.hoisted(() => ({
  mockedGetUser: vi.fn(),
  mockedSigninRedirect: vi.fn(),
  mockedSignoutRedirect: vi.fn(),
}))

const JWT_TOKEN = 'eyJhbGciOiJSUzI1NiIsImtpZCI6IkUyMUZDMEIzMUYwRkUxOEQwM0I2ODUzNzAxQkRFQUIyIiwidHlwIjoiYXQrand0In0.eyJuYmYiOjE2NjU5NDk4NjUsImV4cCI6MTY2NTk1MDQ2NSwiaXNzIjoiaHR0cHM6Ly9sb2NhbGhvc3Q6ODAwMCIsImNsaWVudF9pZCI6ImNycGctd2ViLXVpIiwic3ViIjoiMiIsImF1dGhfdGltZSI6MTY2NTk0OTg1MiwiaWRwIjoibG9jYWwiLCJyb2xlIjoiVXNlciIsImp0aSI6IjY2N0RFMzZDOTg0RUFDRENBMDZBNUVDQkVGNTBDRDBEIiwic2lkIjoiOTAxRTFBNUNEQUVBRjMxMjRBMjZCODJDRDUzNjM2RTYiLCJpYXQiOjE2NjU5NDk4NTMsInNjb3BlIjpbIm9wZW5pZCIsInVzZXJfYXBpIiwib2ZmbGluZV9hY2Nlc3MiXSwiYW1yIjpbInB3ZCJdfQ.Js5ze2JNSew0m5UD86HyA62cMvPxnFJACa8IhQW59vzTABticqtdo8070soza-11JJyT_zHAI97SWSQRUoQ4w1pCdPmMsh5HyMueMx-OO_cFpkZg6PkpQlaSYB_Z_916k5nhCRVkNK7X4H2MByhkMd1rh0yFFGvYKVAnWKNYZFYL2y9VPv510b8RV0JyfjZvwLRN-cU2n7xVHkSzSE7WYz6X3D9l4MS-cOZRt3Y62EqjuHbBY3sJk-VIJCAuc0puGmms69_-9KV5cMLNfwoHOihYctSB7JVh-oSlXxuvkZHz_23eQnvsW5DLCgrOKnxMQBj44TRBwqHmrLl89UV3GQ'

const DECODED_TOKEN = {
  amr: ['pwd'],
  auth_time: 1665949852,
  client_id: 'crpg-web-ui',
  exp: 1665950465,
  iat: 1665949853,
  idp: 'local',
  iss: 'https://localhost:8000',
  jti: '667DE36C984EACDCA06A5ECBEF50CD0D',
  nbf: 1665949865,
  role: 'User',
  scope: ['openid', 'user_api', 'offline_access'],
  sid: '901E1A5CDAEAF3124A26B82CD53636E6',
  sub: '2',
}

vi.mock('oidc-client-ts', () => ({
  UserManager: vi.fn().mockImplementation(() => ({
    getUser: mockedGetUser,
    signinRedirect: mockedSigninRedirect,
    signoutRedirect: mockedSignoutRedirect,
  })),
  WebStorageStateStore: vi.fn(),
}))

describe('utils', () => {
  it('parseJwt', () => {
    expect(parseJwt(JWT_TOKEN)).toEqual(DECODED_TOKEN)
  })
})

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

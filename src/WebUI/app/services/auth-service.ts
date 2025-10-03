// @vitest-environment jsdom

import type { User } from 'oidc-client-ts'

import {
  // Log,
  UserManager,
  WebStorageStateStore,
} from 'oidc-client-ts'

import type { Platform } from '~/models/platform'

// Log.setLogger(console)
// Log.setLevel(Log.DEBUG)

export const extractToken = (user: User | null): string | null =>
  user !== null ? user.access_token : null

export const parseJwt = (token: string) =>
  // eslint-disable-next-line node/prefer-global/buffer
  JSON.parse(Buffer.from(token.split('.')[1] as string, 'base64').toString())

export const userManager = new UserManager({
  authority: import.meta.env.NUXT_PUBLIC_API_BASE_URL,
  client_id: 'crpg-web-ui',
  post_logout_redirect_uri: globalThis.location.origin,
  redirect_uri: `${window.location.origin}/signin-callback`,
  response_type: 'code',
  scope: 'openid offline_access user_api',
  silent_redirect_uri: `${window.location.origin}/signin-silent-callback`, // TODO: FIXME:
  userStore: new WebStorageStateStore({ store: window.localStorage }),
})

export const getUser = () => userManager.getUser()

export const login = (platform: Platform) => userManager.signinRedirect({
  extraQueryParams: {
    identity_provider: platform,
  },
})

export const logout = () => userManager.signoutRedirect()

export const getToken = async () => extractToken(await getUser())

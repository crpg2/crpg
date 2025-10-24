import { UserManager, WebStorageStateStore } from 'oidc-client-ts'

import type { Platform } from '~/models/platform'

let _userManager: UserManager | null = null
const getUserManager = () => {
  if (!_userManager) {
    _userManager = new UserManager({
      authority: import.meta.env.NUXT_PUBLIC_API_BASE_URL,
      client_id: 'crpg-web-ui',
      post_logout_redirect_uri: globalThis.location.origin,
      redirect_uri: `${window.location.origin}/signin-callback`,
      response_type: 'code',
      scope: 'openid offline_access user_api',
      silent_redirect_uri: `${window.location.origin}/signin-silent-callback`, // TODO: FIXME:
      userStore: new WebStorageStateStore({ store: window.localStorage }),
    })
  }
  return _userManager
}

export const getUser = () => getUserManager().getUser()

export const login = (platform: Platform) => getUserManager().signinRedirect({
  extraQueryParams: {
    identity_provider: platform,
  },
})

export const logout = () => getUserManager().signoutRedirect()

export const signinCallback = () => getUserManager().signinCallback()

export const getToken = async () => (await getUser())?.access_token

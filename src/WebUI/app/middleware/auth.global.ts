import type { RouteLocationNormalized } from 'vue-router'

import type { Role } from '~/models/role'

import { getUser } from '~/services/auth-service'

const routeHasAnyRoles = (
  route: RouteLocationNormalized,
): boolean => Boolean(route.meta.roles?.length)

const userAllowedAccess = (
  route: RouteLocationNormalized,
  role: Role,
): boolean => Boolean(route.meta.roles?.includes(role))

export default defineNuxtRouteMiddleware(async (to) => {
  /*
    (1) service/public route, for example - oidc callback pages: /signin-callback
    (2) user data is not loaded but user is logged in - get user data
    (3) to-route has a role requirement
    (4) user has access
  */

  // (1)
  if (to.meta.skipAuth) {
    return true
  }

  const userStore = useUserStore()

  // (2)
  if (!userStore.user && (await getUser()) !== null) {
    await userStore.fetchUser()
  }

  // (3)
  if (routeHasAnyRoles(to)) {
    // (4)
    if (userStore.user === null || !userAllowedAccess(to, userStore.user.role)) {
      return navigateTo({ name: 'index' }, { replace: true })
    }
  }
})

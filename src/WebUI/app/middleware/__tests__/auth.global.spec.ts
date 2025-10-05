import type { RouteLocationNormalized } from 'vue-router'

import { navigateTo } from '#app'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import type { User } from '~/models/user'

import { useUser } from '~/composables/user/use-user'
import { getUser } from '~/services/auth-service'

import authMiddleware from '../auth.global'

vi.mock('~/composables/user/use-user', () => ({
  useUser: vi.fn(),
}))

vi.mock('~/services/auth-service', () => ({
  getUser: vi.fn(),
}))

vi.mock('#app', () => ({
  defineNuxtRouteMiddleware: vi.fn(fn => fn),
  navigateTo: vi.fn(route => route),
}))

const makeRoute = (
  routePart: Partial<RouteLocationNormalized> = {},
): RouteLocationNormalized => ({
  fullPath: '',
  hash: '',
  matched: [],
  meta: {},
  // @ts-expect-error ///
  name: '',
  params: {},
  path: '',
  query: {},
  redirectedFrom: undefined,
  ...routePart,
})

describe('auth middleware', () => {
  const fetchUser = vi.fn().mockResolvedValue(undefined)
  const mockUser: { value: Partial<User> | null } = { value: null }
  const from = makeRoute()

  beforeEach(() => {
    vi.clearAllMocks();
    (useUser as any).mockReturnValue({
      user: mockUser,
      fetchUser,
    })
  })

  it('skip route validation with meta.skipAuth', async () => {
    const route = makeRoute({ meta: { skipAuth: true } })
    expect(await authMiddleware(route, from)).toBe(true)
    expect(navigateTo).not.toHaveBeenCalled()
    expect(fetchUser).not.toBeCalled()
  })

  describe('route not requires any role', () => {
    const route = makeRoute()

    it('!user && !isSignIn - should be call getUser', async () => {
      (getUser as any).mockResolvedValue(null)
      await authMiddleware(route, from)
      expect(getUser).toHaveBeenCalled()
      expect(fetchUser).not.toHaveBeenCalled()
    })

    it('!user && isSignIn - should be call fetchUser', async () => {
      (getUser as any).mockResolvedValue({ id: 1 })
      await authMiddleware(route, from)
      expect(fetchUser).toHaveBeenCalled()
    })
  })

  describe('route requires role', () => {
    it('user with role:User -> validation passed', async () => {
      const route = makeRoute({
        meta: {
          roles: ['User'],
        },
      })
      mockUser.value = { role: 'User' }
      expect(await authMiddleware(route, from)).toBeUndefined()
      expect(navigateTo).not.toHaveBeenCalled()
    })

    it('user with role:User, admin route -> go to index page', async () => {
      const route = makeRoute({
        meta: {
          roles: ['Admin'],
        },
      })
      mockUser.value = { role: 'User' }
      await authMiddleware(route, from)
      expect(navigateTo).toHaveBeenCalledWith({ name: 'index' }, { replace: true })
    })
  })
})

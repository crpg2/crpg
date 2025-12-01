import type { User as oidcUser } from 'oidc-client-ts'
import type { RouteLocationNormalized } from 'vue-router'

import { navigateTo } from '#app'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { ref } from 'vue'

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
  meta: {},
  ...routePart,
} as RouteLocationNormalized)

describe('auth middleware', () => {
  const fetchUser = vi.fn().mockResolvedValue(undefined)
  const mockUser = ref<Partial<User> | null>(null)

  vi.mocked(useUser).mockReturnValue({
    user: mockUser,
    fetchUser,
  } as unknown as ReturnType<typeof useUser>)

  vi.mocked(getUser).mockResolvedValue(null)

  const from = makeRoute()
  const to = makeRoute()

  beforeEach(() => {
    mockUser.value = null
  })

  describe('authentication', () => {
    it('skip route validation with meta.skipAuth', async () => {
      const to = makeRoute({ meta: { skipAuth: true } })

      expect(await authMiddleware(to, from)).toBe(true)
      expect(navigateTo).not.toHaveBeenCalled()
      expect(fetchUser).not.toHaveBeenCalled()
    })

    it('calls getUser if no user and token invalid', async () => {
      await authMiddleware(to, from)

      expect(getUser).toHaveBeenCalled()
      expect(fetchUser).not.toHaveBeenCalled()
    })

    it('fetches user if token valid', async () => {
      vi.mocked(getUser).mockResolvedValueOnce({ access_token: '123' } as unknown as oidcUser)

      await authMiddleware(to, from)

      expect(fetchUser).toHaveBeenCalled()
    })

    it('does not throw if fetchUser() fails and does not redirect', async () => {
      vi.mocked(getUser).mockResolvedValueOnce({ access_token: '123' } as unknown as oidcUser)

      fetchUser.mockRejectedValueOnce(new Error('fetch failed'))

      await expect(authMiddleware(to, from)).resolves.not.toThrow()
      expect(navigateTo).not.toHaveBeenCalled()
    })
  })

  describe('authorization (roles)', () => {
    it('passes if user has required role', async () => {
      const to = makeRoute({ meta: { roles: ['User'] } })
      mockUser.value = { role: 'User' }

      expect(await authMiddleware(to, from)).toBe(true)
      expect(navigateTo).not.toHaveBeenCalled()
    })

    it('redirects to index if user role insufficient', async () => {
      const to = makeRoute({ meta: { roles: ['Admin'] } })
      mockUser.value = { role: 'User' }

      await authMiddleware(to, from)

      expect(navigateTo).toHaveBeenCalledWith(
        expect.objectContaining({ name: 'index' }),
        expect.objectContaining({ replace: true }),
      )
    })

    it('skips role validation if skipAuth is true', async () => {
      const to = makeRoute({ meta: { skipAuth: true, roles: ['Admin'] } })
      mockUser.value = { role: 'User' }

      expect(await authMiddleware(to, from)).toBe(true)
      expect(navigateTo).not.toHaveBeenCalled()
    })
  })
})

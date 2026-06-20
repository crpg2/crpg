import { describe, expect, it } from 'vitest'

import type { User } from '~/models/user'

import { ROLE } from '~/models/role'
import { isAdmin } from '~/services/role-service'

const userWithRole = (role: User['role']): User => ({ role } as User)

describe('role service', () => {
  describe('isAdmin', () => {
    it('is true for an admin', () => {
      expect(isAdmin(userWithRole(ROLE.Admin))).toBe(true)
    })

    it.each([ROLE.User, ROLE.Moderator, ROLE.GameAdmin])('is false for %s', (role) => {
      expect(isAdmin(userWithRole(role))).toBe(false)
    })

    it('is false when there is no user', () => {
      expect(isAdmin(null)).toBe(false)
      expect(isAdmin(undefined)).toBe(false)
    })
  })
})

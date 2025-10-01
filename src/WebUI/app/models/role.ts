import type { Role as _Role } from '#api'
import type { ValueOf } from 'type-fest'

export const ROLE = {
  User: 'User',
  Moderator: 'Moderator',
  Admin: 'Admin',
  GameAdmin: 'GameAdmin',
} as const satisfies Record<_Role, _Role>

export type Role = ValueOf<typeof ROLE>

export const SomeRole: Role[] = Object.values(ROLE)

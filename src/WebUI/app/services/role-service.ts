import type { User } from '~/models/user'

import { ROLE } from '~/models/role'

// Whether the user is an administrator. Single source of truth for admin-only UI gating.
export const isAdmin = (user: User | null | undefined): boolean => user?.role === ROLE.Admin

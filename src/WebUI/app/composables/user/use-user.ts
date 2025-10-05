import type { User } from '~/models/user'

import { getUser } from '~/services/user-service'

export function useUser() {
  const user = useState<User | null>('user', () => null)

  async function fetchUser() {
    const data = await getUser()
    user.value = data
  }

  const clan = computed(() => user.value?.clanMembership?.clan ?? null)
  const clanMemberRole = computed(() => user.value?.clanMembership?.role ?? null)
  const hasUnreadNotifications = computed(() => Boolean(user.value?.unreadNotificationsCount))

  return {
    user,
    fetchUser,
    clan,
    clanMemberRole,
    hasUnreadNotifications,
  }
}

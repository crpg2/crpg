import { getUser, getUserRestriction } from '~/services/user-service'

// TODO: migrate to useState userProvider
export const useUserStore = defineStore('user', () => {
  const {
    state: user,
    execute: fetchUser,
  } = useAsyncState(
    () => getUser(),
    null,
    { resetOnExecute: false, immediate: false },
  )

  const clan = computed(() => user.value?.clanMembership?.clan ?? null)

  const clanMemberRole = computed(() => user.value?.clanMembership?.role ?? null)

  const hasUnreadNotifications = computed(() => Boolean(user.value?.unreadNotificationsCount))

  // TODO: to dedicate composable
  const {
    state: restriction,
    execute: fetchUserRestriction,
  } = useAsyncState(
    () => getUserRestriction(),
    null,
    { immediate: false },
  )

  return {
    user,
    fetchUser,
    clan,
    clanMemberRole,

    restriction,
    fetchUserRestriction,

    hasUnreadNotifications,
  }
})

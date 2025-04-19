import type { NavigationGuard, RouteLocationNormalized } from 'vue-router/auto'

import { canManageApplicationsValidate, canUpdateClanValidate } from '~/services/clan-service'
import { useUserStore } from '~/stores/user'

export const clanIdParamValidate: NavigationGuard = (to) => {
  // @ts-expect-error TODO:
  if (Number.isNaN(Number(to.params.id as string))) {
    return { name: 'PageNotFound' } as RouteLocationNormalized<'PageNotFound'>
  }

  return true
}

export const clanExistValidate: NavigationGuard = async () => {
  const userStore = useUserStore()

  if (userStore.clan !== null) {
    return {
      name: 'ClansId',
      params: { id: String(userStore.clan.id) },
    } as RouteLocationNormalized<'ClansId'>
  }

  return true
}

export const canUpdateClan: NavigationGuard = async (to) => {
  const userStore = useUserStore()

  if (
    // @ts-expect-error TODO:
    userStore.clan?.id !== Number(to.params.id as string)
    || (userStore.clanMemberRole !== null && !canUpdateClanValidate(userStore.clanMemberRole))
  ) {
    return { name: 'Clans' } as RouteLocationNormalized<'Clans'>
  }

  return true
}

export const canManageApplications: NavigationGuard = async (to) => {
  const userStore = useUserStore()

  if (
    // @ts-expect-error TODO:
    userStore.clan?.id !== Number(to.params.id as string)
    || (userStore.clanMemberRole !== null && !canManageApplicationsValidate(userStore.clanMemberRole))
  ) {
    return { name: 'Clans' } as RouteLocationNormalized<'Clans'>
  }

  return true
}

export const canUseClanArmory: NavigationGuard = async (to) => {
  const userStore = useUserStore()

  // @ts-expect-error TODO:
  if (userStore.clan?.id !== Number(to.params.id as string)) {
    return { name: 'Clans' } as RouteLocationNormalized<'Clans'>
  }

  return true
}

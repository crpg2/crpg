import type { RouteLocationNormalizedLoaded } from 'vue-router'

import { useUser } from '~/composables/user/use-user'

export default defineNuxtRouteMiddleware(async (to) => {
  const { clan } = useUser()
  if (clan.value?.id !== Number((to as RouteLocationNormalizedLoaded<'clans-id'>).params.id)) {
    return navigateTo({ name: 'clans' })
  }
})

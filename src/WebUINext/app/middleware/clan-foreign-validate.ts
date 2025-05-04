import type { RouteLocationNormalizedLoaded } from 'vue-router'

export default defineNuxtRouteMiddleware(async (to, from) => {
  const userStore = useUserStore()

  if (userStore.clan?.id !== Number((to as RouteLocationNormalizedLoaded<'clans-id'>).params.id)) {
    return navigateTo({ name: 'clans' })
  }
})

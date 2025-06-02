import type { RouteLocationNormalizedLoaded } from 'vue-router'

export default defineNuxtRouteMiddleware(async (to) => {
  if (Number.isNaN(Number((to as RouteLocationNormalizedLoaded<'clans-id'>).params.id))) {
    return navigateTo({ name: 'clans' })
  }
})

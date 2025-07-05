import { useStorage } from '@vueuse/core'

import type { Platform } from '~/models/platform'

import { PLATFORM } from '~/models/platform'

export const usePlatform = () => {
  const platform = useStorage<Platform>('user-platform', PLATFORM.Steam) // Steam by default

  const changePlatform = (p: Platform) => {
    platform.value = p
  }

  return {
    changePlatform,
    platform: readonly(platform),
  }
}

import type { Role } from '~/models/role'

declare module 'nuxt/schema' {
  interface PageMeta {
    roles?: Role[]
    skipAuth?: boolean
    layoutOptions?: {
      bg?: string
      noFooter?: boolean
    }
  }

  interface AppConfig {
  }

  interface RuntimeConfig {

  }

  interface PublicRuntimeConfig {
    HH: string
  }
}

export {}

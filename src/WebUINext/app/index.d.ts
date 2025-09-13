import type { Role } from '~/models/role'

declare module '#app' {
  interface PageMeta {
    roles?: Role[]
    skipAuth?: boolean
    layoutOptions?: {
      bg?: string
      noFooter?: boolean
      noStickyHeader?: boolean
    }
  }
}

declare module 'nuxt/schema' {
  interface AppConfig { }

  interface RuntimeConfig { }

  interface PublicRuntimeConfig {
    HH: string
  }
}

export {}

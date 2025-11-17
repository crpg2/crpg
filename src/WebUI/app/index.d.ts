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
  interface AppConfig {
  }

  interface AppConfigInput {
    pollIntevalMs: number
    settings: Settings
    links: {
      tipsTricksHelpThread: string
      buildSupportThread: string
    }
  }

  interface RuntimeConfig { }

  interface PublicRuntimeConfig { }
}

declare module '@tanstack/table-core' {
  interface FilterFns {
    includesSome?: FilterFn<unknown>
  }
}

export {}

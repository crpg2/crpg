import type { Role } from '~/models/role'

declare module '#app' {
  interface PageMeta {
    roles?: Role[]
    skipAuth?: boolean
    layoutOptions?: {
      bg?: string
      noFooter?: boolean
    }
  }

  //   interface AppConfig {
  //   }
}

export {}

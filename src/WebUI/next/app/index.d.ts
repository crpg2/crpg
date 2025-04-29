import type { Role } from '~/models/role'

declare module '#app' {
  interface PageMeta {
    roles?: Role[]
    skipAuth?: boolean
  }

  //   interface AppConfig {
  //   }
}

export {}

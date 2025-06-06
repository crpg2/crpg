import type { InjectionKey } from 'vue'

import { inject } from 'vue'

// Please, use injectStrict instead the regular inject
// ref: https://logaretm.com/blog/type-safe-provide-inject/
export const injectStrict = <T>(key: InjectionKey<T>, fallback?: T): T => {
  const resolved = inject(key, fallback)
  if (resolved === undefined || resolved === null) {
    throw new Error(`Could not resolve ${key.description}`)
  }
  return resolved
}

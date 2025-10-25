import type { InjectionKey } from 'vue'

import { inject } from 'vue'

/**
 * @description Please, use injectStrict instead the regular inject
 * @link https://logaretm.com/blog/type-safe-provide-inject/
 */
export const injectStrict = <T>(key: InjectionKey<T>, fallback?: T): T => {
  const resolved = inject(key, fallback)
  if (resolved === undefined || resolved === null) {
    throw new Error(`Could not resolve ${key.description}`)
  }
  return resolved
}

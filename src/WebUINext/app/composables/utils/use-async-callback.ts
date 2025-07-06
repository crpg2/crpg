// ref https://github.com/vueuse/vueuse/issues/2890
import { makeDestructurable, noop } from '@vueuse/shared'

export type AnyPromiseFn = (...args: any[]) => Promise<any>

export interface UseAsyncCallbackOptions {
  /**
   * Callback when error is caught.
   */
  onError?: (e: unknown) => void

  /**
   * Callback when success is caught.
   */
  onSuccess?: () => void

  /**
   * An error is thrown when executing the execute function
   * @default false
   */
  throwError?: boolean
}

export type UseAsyncCallbackReturn<Fn extends AnyPromiseFn> = readonly [
  Fn,
  Ref<boolean>,
  Ref<any>,
] & {
  execute: Fn
  isLoading: Ref<boolean>
  error: Ref<any>
}

/**
 * Using async functions
 *
 * @see https://vueuse.org/useAsyncCallback
 * @param fn
 */
export function useAsyncCallback<T extends AnyPromiseFn>(fn: T, options?: UseAsyncCallbackOptions): UseAsyncCallbackReturn<T> {
  const {
    onError = noop,
    onSuccess = noop,
    throwError = false,
  } = options ?? {}

  const error = ref()
  const isLoading = ref(false)

  const execute = (async (...args: any[]) => {
    try {
      isLoading.value = true
      await fn(...args)
      isLoading.value = false
      onSuccess()
    }
    catch (e) {
      isLoading.value = false
      error.value = e
      onError(e)
      if (throwError) {
        throw e
      }
    }
  }) as T

  return makeDestructurable(
    { error, execute, isLoading } as const,
    [execute, isLoading, error] as const,
  )
}

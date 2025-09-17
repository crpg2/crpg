// ref https://github.com/vueuse/vueuse/issues/2890
import { makeDestructurable, noop } from '@vueuse/shared'

import { usePageLoading } from '~/composables/app/use-page-loading'

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

  successMessage?: string

  /**
   * An error is thrown when executing the execute function
   * @default false
   */
  throwError?: boolean

  pageLoading?: boolean
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
    pageLoading = false,
    successMessage,
  } = options ?? {}

  const error = ref()
  const isLoading = ref(false)
  const toast = useToast()

  const execute = (async (...args: any[]) => {
    try {
      isLoading.value = true
      await fn(...args)
      isLoading.value = false

      if (successMessage) {
        toast.add({
          title: successMessage,
          close: false,
          color: 'success',
        })
      }
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

  if (pageLoading) {
    usePageLoading([isLoading])
  }

  return makeDestructurable(
    { error, execute, isLoading } as const,
    [execute, isLoading, error] as const,
  )
}

import { makeDestructurable, noop } from '@vueuse/shared'

import { usePageLoading } from '~/composables/app/use-page-loading'

export type AnyPromiseFn = (...args: any[]) => Promise<any>

export interface UseAsyncCallbackOptions {
  onError?: (e: unknown) => void
  onSuccess?: () => void
  throwError?: boolean
  successMessage?: string
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

export function useAsyncCallback<T extends AnyPromiseFn>(fn: T, options?: UseAsyncCallbackOptions): UseAsyncCallbackReturn<T> {
  const {
    onError = noop,
    onSuccess = noop,
    throwError = false,
    pageLoading = true,
    successMessage,
  } = options ?? {}

  const error = shallowRef()
  const isLoading = shallowRef(false)
  const toast = useToast()

  const execute = (async (...args: any[]) => {
    isLoading.value = true

    try {
      await fn(...args)

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
      error.value = e
      onError(e)
      if (throwError) {
        throw e
      }
    }
    finally {
      isLoading.value = false
    }
  }) as T

  if (pageLoading) {
    usePageLoading(isLoading)
  }

  return makeDestructurable(
    { execute, isLoading, error } as const,
    [execute, isLoading, error] as const,
  )
}

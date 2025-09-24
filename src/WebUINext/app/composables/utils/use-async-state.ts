import type { UseAsyncStateOptions, UseAsyncStateReturn } from '@vueuse/core'

import { useAsyncState as _useAsyncState } from '@vueuse/core'

import { usePageLoading } from '~/composables/app/use-page-loading'
import { usePollInterval } from '~/composables/utils/use-poll-interval'

type UseAsyncStateWithPollOptions<Shallow extends boolean, Data = any> = UseAsyncStateOptions<Shallow, Data> & {
  pollKey?: EntryKey
  pageLoading?: boolean
}

export function useAsyncStateWithPoll<Data, Params extends any[] = any[], Shallow extends boolean = true>(
  promise: Promise<Data> | ((...args: Params) => Promise<Data>),
  initialState: MaybeRef<Data>,
  options?: UseAsyncStateWithPollOptions<Shallow, Data>,
): UseAsyncStateReturn<Data, Params, Shallow> {
  const {
    pollKey,
    pageLoading = false,
    ..._useAsyncStateOptions
  } = options ?? {}

  const { state, execute, executeImmediate, isLoading, isReady, error, then } = _useAsyncState(promise, initialState, _useAsyncStateOptions)

  if (pollKey) {
    usePollInterval(
      {
        key: toCacheKey(pollKey),
        fn: executeImmediate,
      },
    )
  }

  if (pageLoading) {
    usePageLoading(isLoading)
  }

  return {
    state,
    execute,
    executeImmediate,
    isLoading,
    isReady,
    error,
    then,
  }
}

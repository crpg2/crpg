import type { AsyncData, AsyncDataOptions, NuxtApp, NuxtError } from 'nuxt/app'

import { refreshNuxtData, useAsyncData } from 'nuxt/app'

import type { KeysOf, PickFrom } from '#app/composables/asyncData'

import { usePageLoading } from '~/composables/app/use-page-loading'
import { usePollInterval } from '~/composables/utils/use-poll-interval'

type AsyncDataCustomOptions<
  ResT,
  DataT = ResT,
  PickKeys extends KeysOf<DataT> = KeysOf<DataT>,
  DefaultT = undefined,
> = Omit<AsyncDataOptions<ResT, DataT, PickKeys, DefaultT>, 'getCachedData'> & {
  /**
   * @default true
   */
  poll?: boolean
  /**
   * @default true
   */
  loadingIndicator?: boolean
}

export function useAsyncDataCustom<
  ResT,
  NuxtErrorDataT = unknown,
  DataT = ResT,
  PickKeys extends KeysOf<DataT> = KeysOf<DataT>,
  DefaultT = undefined,
>(
  key: MaybeRefOrGetter<EntryKey>,
  handler: (app?: NuxtApp) => Promise<ResT>,
  options: AsyncDataCustomOptions<ResT, DataT, PickKeys, DefaultT>,
): AsyncData<PickFrom<DataT, PickKeys>, (NuxtErrorDataT extends Error | NuxtError ? NuxtErrorDataT : NuxtError<NuxtErrorDataT>) | undefined> {
  const {
    loadingIndicator,
    poll,
    ..._options
  } = { poll: true, loadingIndicator: true, ...options }

  const _key = computed(() => toCacheKey(toValue(key)))

  const { refresh, pending, ...rest } = useAsyncData(
    _key,
    handler,
    _options,
  )

  if (poll) {
    usePollInterval({
      fn: refresh,
      key: _key.value,
    })
  }

  if (loadingIndicator) {
    usePageLoading(pending)
  }

  // @ts-expect-error ...
  return {
    refresh,
    pending,
    ...rest,
  }
}

export function getAsyncData<DataT = any>(key: MaybeRefOrGetter<EntryKey>): Ref<DataT> {
  const { data } = useNuxtData<DataT[]>(toCacheKey(toValue(key))) as { data: Ref<DataT> }

  if (!data.value) {
    throw new Error(`Could not resolve ${toValue(key)}`)
  }

  return data
}

export function refreshAsyncData(key: MaybeRefOrGetter<EntryKey>) {
  return () => refreshNuxtData(toCacheKey(toValue(key)))
}

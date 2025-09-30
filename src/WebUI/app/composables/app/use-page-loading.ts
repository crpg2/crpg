import type { MultiWatchSources, Ref, WatchSource } from 'vue'

const pageLoadingKey: InjectionKey<{
  state: Ref<boolean>
  toggle: (value?: boolean) => boolean
}> = Symbol('PageLoading')

export const usePageLoadingProvider = () => {
  const [state, toggle] = useToggle()

  provide(pageLoadingKey, { state, toggle })

  return makeDestructurable(
    { state, toggle } as const,
    [state, toggle] as const,
  )
}

export const usePageLoading = (watchSource: WatchSource | MultiWatchSources) => {
  const { state, toggle } = injectStrict(pageLoadingKey)

  const _watchs = Array.isArray(watchSource) ? watchSource : [watchSource]

  tryOnScopeDispose(watch(_watchs, states => toggle(states.some(Boolean))))

  return makeDestructurable(
    { state, toggle } as const,
    [state, toggle] as const,
  )
}

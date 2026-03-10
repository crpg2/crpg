import type { MultiWatchSources, WatchSource } from 'vue'

export const usePageLoading = (watchSource?: WatchSource | MultiWatchSources) => {
  const state = useState<boolean>('pageLoading', () => false)

  const toggle = (val: boolean) => {
    state.value = val
  }

  if (watchSource) {
    const _watchs = Array.isArray(watchSource) ? watchSource : [watchSource]
    tryOnScopeDispose(watch(_watchs, states => toggle(states.some(Boolean))))
  }

  return makeDestructurable(
    { state, toggle } as const,
    [state, toggle] as const,
  )
}

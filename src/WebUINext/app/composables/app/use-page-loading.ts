import type { MultiWatchSources, Ref } from 'vue'

const pageLoadingKey: InjectionKey<{
  state: Ref<boolean>
  toggle: (value?: boolean) => boolean
}> = Symbol('PageLoading')

export const usePageLoadingProvider = () => {
  const [state, toggle] = useToggle()

  provide(pageLoadingKey, { state, toggle })

  return {
    state,
    toggle,
  }
}

interface PageLoadingOptions {
  watch?: MultiWatchSources
}

export const usePageLoading = (options?: PageLoadingOptions) => {
  const { state, toggle } = injectStrict(pageLoadingKey)

  const hasScope = getCurrentScope()

  const unsubExecute = watch(options?.watch || [], (states) => {
    toggle(states.some(Boolean))
  }, { flush: 'sync' })

  if (hasScope) {
    onScopeDispose(() => {
      unsubExecute()
    })
  }

  return {
    state,
    togglePageLoading: toggle,
  }
}

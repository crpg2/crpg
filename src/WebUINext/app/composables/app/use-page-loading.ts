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

export const usePageLoading = (watches: MultiWatchSources) => {
  const { state, toggle } = injectStrict(pageLoadingKey)

  const unsubscribeExecute = watch(watches, states => toggle(states.some(Boolean)))

  // TODO: scope dispose
  onBeforeUnmount(() => {
    unsubscribeExecute()
  })

  return {
    state,
    togglePageLoading: toggle,
  }
}

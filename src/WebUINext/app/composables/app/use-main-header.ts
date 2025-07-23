const mainHeaderHeightKey: InjectionKey<Ref<number>> = Symbol('MainHeaderHeight')

export const useMainHeaderProvider = (height: Ref<number>) => {
  provide(mainHeaderHeightKey, height)
}

export const useMainHeader = () => {
  const mainHeaderHeight = injectStrict(mainHeaderHeightKey)

  return {
    mainHeaderHeight,
  }
}

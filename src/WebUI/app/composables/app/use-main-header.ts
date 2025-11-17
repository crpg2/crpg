import type { TemplateRef } from 'vue'

const mainHeaderHeightKey: InjectionKey<Ref<number>> = Symbol('MainHeaderHeight')

export const useMainHeaderProvider = (el: TemplateRef<HTMLElement | null>) => {
  const { height } = useElementSize(el, { height: 0, width: 0 }, { box: 'border-box' })
  provide(mainHeaderHeightKey, height)
}

export const useMainHeader = () => {
  const mainHeaderHeight = injectStrict(mainHeaderHeightKey)

  return {
    mainHeaderHeight,
  }
}

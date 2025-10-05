// @vitest-environment jsdom

/**
 * @link https://github.com/verdie-g/crpg/issues/873
 * @link https://user-images.githubusercontent.com/33551334/234285036-e96bd6a2-26c8-4ddc-9310-b2528f3ab70c.png
 */

import { shallowMount } from '@vue/test-utils'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { defineComponent, nextTick, ref } from 'vue'

import { useStickySidebar } from '../use-sticky-sidebar'

describe('useStickySidebar', () => {
  const getComponent = (height: number, offsetTop: number = 0, offsetBottom: number = 0) => {
    return shallowMount(defineComponent({
      setup() {
        const el = document.createElement('div')
        vi.spyOn(el, 'offsetHeight', 'get').mockReturnValue(height)
        const { top } = useStickySidebar(ref(el), offsetTop, offsetBottom)
        return { top }
      },
      template: `<div :style="{ top: \`\${top}px\` }" ></div>`,
    }))
  }

  const spyAddEventListener = vi.spyOn(window, 'addEventListener')
  const spyRemoveEventListener = vi.spyOn(window, 'removeEventListener')

  const mockedObserve = vi.fn()
  const mockedDisconnect = vi.fn()
  const ResizeObserverMock = vi.fn(() => ({
    observe: mockedObserve,
    unobserve: vi.fn(),
    disconnect: mockedDisconnect,
  }))
  vi.stubGlobal('ResizeObserver', ResizeObserverMock)

  vi.spyOn(window, 'requestAnimationFrame').mockImplementation((cb) => {
    cb(0)
    return 0
  })
  vi.spyOn(window, 'cancelAnimationFrame').mockImplementation(() => {})

  const triggerScroll = async (y: number = 0) => {
    Object.defineProperty(window, 'scrollY', { value: y, configurable: true })
    window.dispatchEvent(new Event('scroll'))
    return nextTick()
  }

  const WINDOW_VIEWPORT_HEIGHT = 768
  const mockWindowHeight = vi.spyOn(window, 'innerHeight', 'get').mockImplementation(() => WINDOW_VIEWPORT_HEIGHT)

  const triggerResize = async (height: number) => {
    mockWindowHeight.mockImplementationOnce(() => height)
    window.dispatchEvent(new Event('resize'))
    return nextTick()
  }

  const stickySidebarHeight = 1080
  const maxStickyTop = stickySidebarHeight - WINDOW_VIEWPORT_HEIGHT // 312

  let wrapper: ReturnType<typeof getComponent>

  afterEach(async () => {
    wrapper?.unmount()
    await triggerScroll(0)
    vi.clearAllMocks()
  })

  it('does not add a scroll listener if the element is smaller than the screen height', () => {
    wrapper = getComponent(700)

    expect(spyAddEventListener).toHaveBeenCalledWith('resize', expect.any(Function))
    expect(spyAddEventListener).not.toHaveBeenCalledWith('scroll', expect.any(Function))
  })

  it('start listen scroll event after resize', async () => {
    wrapper = getComponent(700)

    expect(spyAddEventListener).toHaveBeenNthCalledWith(1, 'resize', expect.any(Function))

    await triggerResize(500) // resize window 768px -> 500px

    expect(spyAddEventListener).toHaveBeenCalledTimes(2)
    expect(spyAddEventListener).toHaveBeenNthCalledWith(2, 'scroll', expect.any(Function), expect.anything())
  })

  describe('sticky el h >= viewport h', () => {
    it('listen scroll evt', () => {
      wrapper = getComponent(stickySidebarHeight)

      expect(spyAddEventListener).toHaveBeenCalledTimes(2)
      expect(spyAddEventListener).toHaveBeenNthCalledWith(1, 'resize', expect.any(Function))
      expect(spyAddEventListener).toHaveBeenNthCalledWith(2, 'scroll', expect.any(Function), expect.anything())
    })

    it('remove scroll evt listener, after resize window', async () => {
      wrapper = getComponent(stickySidebarHeight)

      expect(spyAddEventListener).toHaveBeenCalledTimes(2)
      expect(spyAddEventListener).toHaveBeenNthCalledWith(1, 'resize', expect.any(Function))
      expect(spyAddEventListener).toHaveBeenNthCalledWith(2, 'scroll', expect.any(Function), expect.anything())

      await triggerResize(1080) // resize window 768px ->1 080px

      expect(spyRemoveEventListener).toHaveBeenCalledTimes(1)
      expect(spyRemoveEventListener).toHaveBeenNthCalledWith(1, 'scroll', expect.any(Function))
    })
  })

  it('remove listeners when cmp is unmounted', () => {
    wrapper = getComponent(stickySidebarHeight)
    wrapper.unmount()

    expect(spyRemoveEventListener).toHaveBeenCalledTimes(2)
    expect(spyRemoveEventListener).toHaveBeenNthCalledWith(1, 'resize', expect.any(Function))
    expect(spyRemoveEventListener).toHaveBeenNthCalledWith(2, 'scroll', expect.any(Function))
  })

  describe('el ResizeObserver', () => {
    it('observe', () => {
      wrapper = getComponent(stickySidebarHeight)

      expect(mockedObserve).toHaveBeenCalledTimes(1)
    })

    it('unobserve', () => {
      wrapper = getComponent(stickySidebarHeight)
      wrapper.unmount()

      expect(mockedDisconnect).toHaveBeenCalledTimes(1)
    })
  })

  describe('scroll DOWN', () => {
    it('scrollY:0px', () => {
      wrapper = getComponent(stickySidebarHeight)
      expect(wrapper.attributes('style')).toEqual('top: 0px;')
    })

    it('scrollY:50px', async () => {
      wrapper = getComponent(stickySidebarHeight, 0, 0)
      await triggerScroll(50)
      expect(wrapper.attributes('style')).toEqual('top: -50px;')
    })
  })

  it('scroll UP', async () => {
    wrapper = getComponent(stickySidebarHeight, 0, 0)

    // scrolling DOWN
    await triggerScroll(maxStickyTop)
    await triggerScroll(1400)

    // then UP
    await triggerScroll(1200)
    expect(wrapper.attributes('style')).toEqual(`top: -112px;`)

    await triggerScroll(1100)
    expect(wrapper.attributes('style')).toEqual(`top: -12px;`)

    await triggerScroll(maxStickyTop)
    await triggerScroll(maxStickyTop - 1)
    expect(wrapper.attributes('style')).toEqual(`top: 0px;`)
  })
})

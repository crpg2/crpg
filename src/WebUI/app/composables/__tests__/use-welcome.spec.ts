import { beforeEach, describe, expect, it, vi } from 'vitest'
import { ref } from 'vue'

import { useWelcome } from '../use-welcome'

const {
  useStorageMock,
  useUserMock,
  useOverlayMock,
} = vi.hoisted(() => ({
  useStorageMock: vi.fn(),
  useUserMock: vi.fn(),
  useOverlayMock: vi.fn(),
}))

vi.mock('@vueuse/core', () => ({
  useStorage: useStorageMock,
}))

vi.mock('~/composables/user/use-user', () => ({
  useUser: useUserMock,
}))

vi.mock('#imports', () => ({
  useOverlay: useOverlayMock,
}))

vi.mock('#components', () => ({
  LazyAppWelcomeModal: {},
}))

describe('useWelcome', () => {
  let storageRef: any
  let openMock: any

  beforeEach(() => {
    storageRef = ref(false)
    useStorageMock.mockReturnValue(storageRef)

    useUserMock.mockReturnValue({ user: ref({ isRecent: true }) })

    openMock = vi.fn()
    useOverlayMock.mockReturnValue({
      create: vi.fn().mockReturnValue({ open: openMock }),
    })
  })

  it('opens modal for new user if not showed before', () => {
    useWelcome()
    expect(openMock).toHaveBeenCalledTimes(1)
  })

  it('does not open modal if user is not new', () => {
    useUserMock.mockReturnValue({ user: ref({ isRecent: false }) })

    useWelcome()
    expect(openMock).not.toHaveBeenCalled()
  })

  it('does not open modal if welcome message already showed', () => {
    storageRef.value = true

    useWelcome()
    expect(openMock).not.toHaveBeenCalled()
  })

  it('sets showedWelcomeMessage to true when modal closes', () => {
    const overlayCreateMock = vi.fn().mockImplementation((_component, options) => {
      options.props.onClose() // Simulate onClose call
      return { open: openMock }
    })
    useOverlayMock.mockReturnValue({ create: overlayCreateMock })

    useWelcome()
    expect(storageRef.value).toBe(true)
  })

  it('returns showWelcomeMessage function', () => {
    const { showWelcomeMessage } = useWelcome()
    expect(typeof showWelcomeMessage).toBe('function')
  })
})

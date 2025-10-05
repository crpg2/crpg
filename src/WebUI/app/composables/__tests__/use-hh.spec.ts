import type { PartialDeep } from 'type-fest'

import { describe, expect, it, vi } from 'vitest'
import { ref } from 'vue'

import { useHappyHours } from '../use-hh'

const {
  mockedUseAppConfig,
  mockedUseToastAdd,
  mockedUseUSer,
  mockedGetHHEventByRegion,
  mockedGetHHEventRemainingSeconds,
  mockedUseLocalStorage,
  mockedIseIntervalFn,
} = vi.hoisted(() => ({
  mockedUseAppConfig: vi.fn<() => PartialDeep<ReturnType<typeof import('#app')['useAppConfig']>>>()
    .mockReturnValue({ settings: { happyHours: '' } }),
  mockedUseToastAdd: vi.fn(() => {}),

  mockedUseUSer: vi.fn<() => PartialDeep<ReturnType<typeof import('~/composables/user/use-user')['useUser']>>>().mockReturnValue({ user: { value: { region: 'Eu' } } }),

  mockedGetHHEventByRegion: vi.fn<() => ReturnType<typeof import('~/services/hh-service')['getHHEventByRegion']>>()
    .mockReturnValue({ start: new Date('2000-02-01T00:00:00.000Z'), end: new Date('2000-02-02T00:00:00.000Z') }),
  mockedGetHHEventRemainingSeconds: vi.fn().mockReturnValue(1),

  mockedUseLocalStorage: vi.fn((key, defaultValue) => ref(defaultValue)),
  mockedIseIntervalFn: vi.fn((cb) => {
    cb()
    return {
      pause: vi.fn(),
    }
  }),
}))

vi.mock('#app', () => ({
  useAppConfig: mockedUseAppConfig,
}))

vi.mock('#imports', () => ({
  useI18n: vi.fn(() => ({ t: (key: string) => key })),
  useToast: vi.fn(() => ({
    add: mockedUseToastAdd,
  })),
}))

vi.mock('@vueuse/core', async importActual => ({
  ...(await importActual()),
  useIntervalFn: mockedIseIntervalFn,
  useLocalStorage: mockedUseLocalStorage,
  tryOnScopeDispose: vi.fn(),
}))

vi.mock('~/services/hh-service', () => ({
  getHHEventByRegion: mockedGetHHEventByRegion,
  getHHEventRemainingSeconds: mockedGetHHEventRemainingSeconds,
}))

vi.mock('~/composables/user/use-user', () => ({
  useUser: mockedUseUSer,
}))

describe('useHappyHours', () => {
  it('init is correct', () => {
    const { hHEvent, isHhEventActive } = useHappyHours()

    expect(hHEvent.value).toEqual({
      start: new Date('2000-02-01T00:00:00.000Z'),
      end: new Date('2000-02-02T00:00:00.000Z'),
    })

    expect(isHhEventActive.value).toBe(true)
  })

  it('notify about the start of Happy Hours, if it has not been shown yet', () => {
    const storageRef = ref(false)
    mockedUseLocalStorage.mockReturnValue(storageRef)

    useHappyHours()

    expect(mockedUseToastAdd).toHaveBeenCalledWith(expect.objectContaining({ title: 'hh.notify.started' }))
    expect(storageRef.value).toBe(true)
  })

  it('does not show the notification about the start again if it has already been shown', () => {
    const storageRef = ref(true)
    mockedUseLocalStorage.mockReturnValue(storageRef)

    useHappyHours()

    expect(mockedUseToastAdd).not.toHaveBeenCalled()
  })

  it('displays a toast when Happy Hours ends', () => {
    const { onEndHH } = useHappyHours()
    onEndHH()
    expect(mockedUseToastAdd).toHaveBeenCalledWith(expect.objectContaining({ title: 'hh.notify.ended' }))
  })
})

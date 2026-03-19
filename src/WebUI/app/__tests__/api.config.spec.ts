import type { FetchResponse } from 'ofetch'

import { beforeEach, describe, expect, it, vi } from 'vitest'

import type { CrpgApiResult, SwaggerValidationApiResult } from '../api.config'

import { onResponseError } from '../api.config'
import { PLATFORM } from '../models/platform'

const { toastAdd, loggerError, mockLogin, mockDelay, routeMeta } = vi.hoisted(() => ({
  toastAdd: vi.fn(),
  loggerError: vi.fn(),
  mockLogin: vi.fn(),
  mockDelay: vi.fn(),
  routeMeta: {} as { roles?: string[] },
}))

vi.mock('#app', () => ({
  useNuxtApp: () => ({ $logger: { error: loggerError } }),
  useRoute: () => ({ meta: routeMeta }),
}))

vi.mock('#imports', () => ({
  useToast: () => ({ add: toastAdd }),
}))

vi.mock('~/services/auth-service', () => ({
  getToken: vi.fn(),
  login: mockLogin,
}))

vi.mock('es-toolkit', () => ({
  delay: mockDelay,
}))

type ApiErrorResponse = CrpgApiResult<unknown> | SwaggerValidationApiResult

const makeResponse = (status: number, data: unknown): FetchResponse<ApiErrorResponse> => ({
  status,
  _data: data as ApiErrorResponse,
}) as FetchResponse<ApiErrorResponse>

describe('api.config onResponseError', () => {
  beforeEach(() => {
    delete routeMeta.roles
    mockLogin.mockResolvedValue(undefined)
    mockDelay.mockResolvedValue(undefined)
  })

  it('handles 401 on protected route and triggers login', async () => {
    routeMeta.roles = ['User']

    await onResponseError({ response: makeResponse(401, null) })

    expect(toastAdd).toHaveBeenCalledWith(expect.objectContaining({ title: 'Session expired', color: 'error' }))
    expect(mockDelay).toHaveBeenCalledWith(1000)
    expect(mockLogin).toHaveBeenCalledWith(PLATFORM.Steam)
    expect(loggerError).not.toHaveBeenCalled()
  })

  it('handles swagger validation error', async () => {
    await onResponseError({
      response: makeResponse(400, {
        title: 'Validation failed',
        errors: {
          name: ['is required'],
          age: ['must be positive'],
        },
      }),
    })

    expect(toastAdd).toHaveBeenCalledWith(expect.objectContaining({
      title: 'Validation failed',
      description: 'is required, must be positive',
      color: 'error',
    }))
    expect(loggerError).toHaveBeenCalledWith('Swagger Validation Api Error', expect.any(Object))
  })

  it('handles crpg api error payload', async () => {
    const error = {
      code: 'Forbidden',
      type: 'Forbidden' as const,
      title: 'No access',
      detail: 'You are not allowed to do this',
      traceId: null,
      stackTrace: null,
    }

    await onResponseError({
      response: makeResponse(403, {
        data: null,
        errors: [error],
      }),
    })

    expect(toastAdd).toHaveBeenCalledWith(expect.objectContaining({
      title: 'No access',
      description: 'You are not allowed to do this',
      color: 'error',
    }))
    expect(loggerError).toHaveBeenCalledWith('Crpg Api Error', error)
  })

  it('shows generic toast when crpg result has no errors', async () => {
    const payload = { data: null, errors: [] }

    await onResponseError({ response: makeResponse(500, payload) })

    expect(toastAdd).toHaveBeenCalledWith(expect.objectContaining({ title: 'Some Error', color: 'error' }))
    expect(loggerError).toHaveBeenCalledWith('Crpg Api Error', payload)
  })

  it('shows generic toast for unknown payload (e.g. HTML error page)', async () => {
    const payload = '<html>Internal Server Error</html>'

    await onResponseError({ response: makeResponse(500, payload) })

    expect(toastAdd).toHaveBeenCalledWith(expect.objectContaining({ title: 'Some Error', color: 'error' }))
    expect(loggerError).toHaveBeenCalledWith('Unknown Api Error', payload)
  })
})

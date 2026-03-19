import type { FetchResponse } from 'ofetch'

import { describe, expect, it, vi } from 'vitest'

import type { CrpgApiResult, SwaggerValidationApiResult } from '../api.config'

import { onResponseError } from '../api.config'
import { PLATFORM } from '../models/platform'

vi.mock('#app', () => ({
  useNuxtApp: () => ({ $logger: undefined }),
  useRoute: () => ({ meta: {} }),
}))

vi.mock('#imports', () => ({
  useToast: () => ({ add: vi.fn() }),
}))

type ApiErrorResponse = CrpgApiResult<unknown> | SwaggerValidationApiResult

const makeResponse = (status: number, data: unknown): FetchResponse<ApiErrorResponse> => ({
  status,
  _data: data as ApiErrorResponse,
}) as FetchResponse<ApiErrorResponse>

describe('api.config onResponseError', () => {
  it('handles 401 on protected route and triggers login', async () => {
    const toastAdd = vi.fn()
    const loggerError = vi.fn()
    const loginFn = vi.fn().mockResolvedValue(undefined)
    const delayFn = vi.fn().mockResolvedValue(undefined)

    await onResponseError(
      {
        response: makeResponse(401, null),
      },
      {
        roles: ['User'],
        toast: { add: toastAdd },
        logger: { error: loggerError },
        loginFn,
        delayFn,
        getStoredPlatform: () => PLATFORM.EpicGames,
      },
    )

    expect(toastAdd).toHaveBeenCalledWith(expect.objectContaining({ title: 'Session expired', color: 'error' }))
    expect(delayFn).toHaveBeenCalledWith(1000)
    expect(loginFn).toHaveBeenCalledWith(PLATFORM.EpicGames)
    expect(loggerError).not.toHaveBeenCalled()
  })

  it('handles swagger validation error', async () => {
    const toastAdd = vi.fn()
    const loggerError = vi.fn()

    await onResponseError(
      {
        response: makeResponse(400, {
          title: 'Validation failed',
          errors: {
            name: ['is required'],
            age: ['must be positive'],
          },
        }),
      },
      {
        toast: { add: toastAdd },
        logger: { error: loggerError },
      },
    )

    expect(toastAdd).toHaveBeenCalledWith(expect.objectContaining({
      title: 'Validation failed',
      description: 'is required, must be positive',
      color: 'error',
    }))
    expect(loggerError).toHaveBeenCalledWith('Swagger Validation Api Error', expect.any(Object))
  })

  it('handles crpg api error payload', async () => {
    const toastAdd = vi.fn()
    const loggerError = vi.fn()

    const error = {
      code: 'Forbidden',
      type: 'Forbidden' as const,
      title: 'No access',
      detail: 'You are not allowed to do this',
      traceId: null,
      stackTrace: null,
    }

    await onResponseError(
      {
        response: makeResponse(403, {
          data: null,
          errors: [error],
        }),
      },
      {
        toast: { add: toastAdd },
        logger: { error: loggerError },
      },
    )

    expect(toastAdd).toHaveBeenCalledWith(expect.objectContaining({
      title: 'No access',
      description: 'You are not allowed to do this',
      color: 'error',
    }))
    expect(loggerError).toHaveBeenCalledWith('Crpg Api Error', error)
  })

  it('shows generic toast when crpg result has no errors', async () => {
    const toastAdd = vi.fn()
    const loggerError = vi.fn()
    const payload = {
      data: null,
      errors: [],
    }

    await onResponseError(
      {
        response: makeResponse(500, payload),
      },
      {
        toast: { add: toastAdd },
        logger: { error: loggerError },
      },
    )

    expect(toastAdd).toHaveBeenCalledWith(expect.objectContaining({ title: 'Some Error', color: 'error' }))
    expect(loggerError).toHaveBeenCalledWith('Crpg Api Error', payload)
  })
})

import type { CreateClientConfig } from '#api/client.gen'
import type { FetchResponse } from 'ofetch'

import { useNuxtApp, useRoute } from '#app'
import { useToast } from '#imports'
import { delay } from 'es-toolkit'

import { getToken, login } from '~/services/auth-service'

import type { Platform } from './models/platform'

import { PLATFORM } from './models/platform'

interface CrpgApiError {
  code: string
  type: 'InternalError' | 'Forbidden' | 'Conflict' | 'NotFound' | 'Validation'
  title: string | null
  detail: string | null
  traceId: string | null
  stackTrace: string | null
}

export interface CrpgApiResult<T> {
  data: T | null
  errors: CrpgApiError[] | null
}

export interface SwaggerValidationApiResult {
  errors: Record<string, string[]>
  status: number
  title: string | null
  traceId: string | null
  type: string | null
}

export const unwrapData = <T>(result: CrpgApiResult<T>): T => {
  if (result.data === null) {
    throw new Error('Response contains no data')
  }

  return result.data
}

const isObjectRecord = (value: unknown): value is Record<string, unknown> => {
  return typeof value === 'object' && value !== null
}

const isCrpgApiResult = (result: unknown): result is CrpgApiResult<unknown> => {
  if (!isObjectRecord(result)) {
    return false
  }

  return 'data' in result && 'errors' in result
}

const isSwaggerValidationApiResult = (result: unknown): result is SwaggerValidationApiResult => {
  if (!isObjectRecord(result)) {
    return false
  }

  return !('data' in result) && 'errors' in result && isObjectRecord(result.errors)
}

export const onResponseError = async (
  { response }: { response: FetchResponse<CrpgApiResult<unknown> | SwaggerValidationApiResult> },
) => {
  const roles = useRoute().meta.roles
  const toast = useToast()
  const logger = useNuxtApp().$logger

  const showErrorToast = (title?: string | null, description?: string | null) => {
    toast.add({
      title: title || 'Some Error',
      ...(description ? { description } : {}),
      color: 'error',
      duration: 5000,
      icon: 'crpg:error',
      close: false,
    })
  }

  if (roles?.length && response.status === 401) {
    showErrorToast('Session expired')
    await delay(1000)
    await login((globalThis.localStorage?.getItem('user-platform') as Platform) ?? PLATFORM.Steam)
    return
  }

  const responseData = response._data

  // Crpg api error
  if (isCrpgApiResult(responseData)) {
    const [error] = responseData.errors ?? []

    if (error) {
      showErrorToast(error.title, error.detail)
      logger?.error?.('Crpg Api Error', error)
      return
    }

    showErrorToast()
    logger?.error?.('Crpg Api Error', responseData)
    return
  }

  // Swagger validation error
  if (isSwaggerValidationApiResult(responseData)) {
    const validationDescription = Object.values(responseData.errors)
      .flatMap(errorMessages => Array.isArray(errorMessages) ? errorMessages : [])
      .join(', ')

    showErrorToast(responseData.title, validationDescription)
    logger?.error?.('Swagger Validation Api Error', responseData)
    return
  }

  // Unknown payload
  showErrorToast()
  logger?.error?.('Unknown Api Error', responseData)
}

export const createClientConfig: CreateClientConfig = config => ({
  ...config,
  baseURL: import.meta.env.NUXT_PUBLIC_API_BASE_URL,
  auth: getToken,
  onResponseError,
})

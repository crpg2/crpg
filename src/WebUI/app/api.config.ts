import type { CreateClientConfig } from '#api/client.gen'
import type { FetchResponse } from 'ofetch'

import { useNuxtApp, useRoute } from '#app'
import { useToast } from '#imports'
import { delay } from 'es-toolkit'

import { getToken, login } from '~/services/auth-service'

import type { Platform } from './models/platform'
import type { Role } from './models/role'

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

interface ErrorToast {
  title: string
  description?: string
  color: 'error'
  duration: number
  icon: 'crpg:error'
  close: false
}

interface ResponseErrorHandlerDependencies {
  roles?: Role[]
  toast?: {
    add: (toast: ErrorToast) => void
  }
  logger?: {
    error?: (message: string, payload?: unknown) => void
  }
  loginFn?: (platform: Platform) => Promise<unknown>
  delayFn?: (timeout: number) => Promise<unknown>
  getStoredPlatform?: () => Platform
}

const needSomeRole = (roles?: Role[]): boolean => {
  return Array.isArray(roles) ? roles.length > 0 : Boolean(roles)
}

const makeErrorToast = (title?: string | null, description?: string | null): ErrorToast => {
  return {
    title: title || 'Some Error',
    ...(description ? { description } : {}),
    color: 'error',
    duration: 5000,
    icon: 'crpg:error',
    close: false,
  }
}

export const onResponseError = async (
  { response }: { response: FetchResponse<CrpgApiResult<unknown> | SwaggerValidationApiResult> },
  deps: ResponseErrorHandlerDependencies = {},
) => {
  const roles = deps.roles ?? useRoute().meta.roles
  const toast = deps.toast ?? useToast()
  const logger = deps.logger ?? useNuxtApp().$logger
  const loginFn = deps.loginFn ?? login
  const delayFn = deps.delayFn ?? delay
  const getStoredPlatform = deps.getStoredPlatform
    ?? (() => (globalThis.localStorage?.getItem('user-platform') as Platform) ?? PLATFORM.Steam)

  const showErrorToast = (title?: string | null, description?: string | null) => {
    toast.add(makeErrorToast(title, description))
  }

  if (needSomeRole(roles) && response.status === 401) {
    showErrorToast('Session expired')
    await delayFn(1000)
    await loginFn(getStoredPlatform())
    return
  }

  const responseData = response._data

  // Swagger validation error
  if (!isCrpgApiResult(responseData)) {
    const validationDescription = Object.values(responseData?.errors || [])
      .flatMap(errorMessages => Array.isArray(errorMessages) ? errorMessages : [])
      .join(', ')

    showErrorToast(responseData?.title, validationDescription)
    logger?.error?.('Swagger Validation Api Error', responseData)
    return
  }

  // Crpg api error
  const [error] = responseData.errors ?? []

  if (error) {
    showErrorToast(error.title, error.detail)
    logger?.error?.('Crpg Api Error', error)
    return
  }

  showErrorToast()
  logger?.error?.('Crpg Api Error', responseData)
}

export const createClientConfig: CreateClientConfig = config => ({
  ...config,
  baseURL: import.meta.env.NUXT_PUBLIC_API_BASE_URL,
  auth: getToken,
  onResponseError,
})

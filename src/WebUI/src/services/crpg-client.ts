import { StatusCodes } from 'http-status-codes'

import type { Result } from '~/models/crpg-client-result'

import { ErrorType } from '~/models/crpg-client-result'
import { Platform } from '~/models/platform'
import { getToken, login } from '~/services/auth-service'
import { NotificationType, notify } from '~/services/notification-service'
import { sleep } from '~/utils/promise'

export const API_BASE_URL = import.meta.env.VITE_API_BASE_URL

async function trySend<T = any>(
  method: string,
  path: string,
  body?: any,
): Promise<Result<T> | null> {
  const token = await getToken()

  const response = await fetch(API_BASE_URL + path, {
    body: body != null ? JSON.stringify(body) : undefined,
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json',
    },
    method,
  })

  if (response.status === StatusCodes.UNAUTHORIZED) {
    notify('Session expired', NotificationType.Warning)
    await sleep(1000)
    await login((localStorage.getItem('user-platform') as Platform) || Platform.Steam)
    return null!
  }

  return response.status !== StatusCodes.NO_CONTENT
    ? (JSONDateToJs(await response.json()) as Result<T>)
    : null
}

async function send(method: string, path: string, body?: any): Promise<any> {
  const result = await trySend(method, path, body)

  if (result === null) {
    return null
  }

  if (result.errors === null) {
    return result.data
  }

  const [error] = result.errors || []

  if (error?.type === ErrorType.InternalError) {
    notify(error.title!, NotificationType.Danger)
    throw new Error('Server error')
  }
  else {
    notify(error.title!, NotificationType.Warning)
    throw new Error('Bad request')
  }
}

export function tryGet<T = any>(path: string): Promise<Result<T> | null> {
  return trySend<T>('GET', path)
}

export function get<T = any>(path: string): Promise<T> {
  return send('GET', path)
}

export function post<T = any>(path: string, body?: any): Promise<T> {
  return send('POST', path, body)
}

export function put<T = any>(path: string, body?: any): Promise<T> {
  return send('PUT', path, body)
}

export function patch<T = any>(path: string, body?: any): Promise<T> {
  return send('PATCH', path, body)
}

export function del(path: string): Promise<any> {
  return send('DELETE', path)
}

// https://medium.com/@vladkens/automatic-parsing-of-date-strings-in-rest-protocol-with-typescript-cf43554bd157
export const JSONDateToJs = (data: unknown) => {
  if (isIsoDateString(data)) {
    return new Date(data)
  }

  if (data === null || data === undefined || typeof data !== 'object') {
    return data
  }

  for (const [key, val] of Object.entries(data)) {
    if (isIsoDateString(val)) {
    // @ts-expect-error this is a hack to make the type checker happy
      data[key] = new Date(val)
    }
    else if (typeof val === 'object') {
      JSONDateToJs(val)
    }
  }
  return data
}

const ISODateFormat = /^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(?:\.\d*)?(?:[-+]\d{2}:?\d{2}|Z)?$/

const isIsoDateString = (value: unknown): value is string =>
  typeof value === 'string' && ISODateFormat.test(value)

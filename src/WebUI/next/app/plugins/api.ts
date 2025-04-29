// import type { NitroFetchRequest, NitroFetchOptions } from 'nitropack'
// import { client } from '~/api/client.gen'
import { Platform } from '~/models/platform'
import { getToken, login } from '~/services/auth-service'

export default defineNuxtPlugin((nuxtApp) => {
//   client.setConfig({
//     baseURL: 'https://localhost:8000',
//   })

  //
  //
  // const { session } = useUserSession()
  const api = $fetch.create({
    baseURL: 'https://localhost:8000', // TODO: FIXME:
    retry: false,
    async onRequest({ request, options, error }) {
      options.headers.set('Authorization', `Bearer ${await getToken()}`)
    },
    onResponse({ response }) {
      if (response.ok && response.status !== 204) {
        response._data = JSONDateToJs(response._data)
        // activeProject.value = projects.value.find(
        //   proj => proj.id === projIdOfReport,
        // )
      }
    },
    async onResponseError({ response }) {
      // UNAUTHORIZED
      if (response.status === 401) {
        // await sleep(1000)
        // retry
        // await login((localStorage.getItem('user-platform') as Platform) || Platform.Steam)
        // notify('Session expired', NotificationType.Warning)
        // TODO:
        // await nuxtApp.runWithContext(() => navigateTo('/login'))
      }
    },
  })

  //   fetcher()
  //   const api = async (
  //     request: NitroFetchRequest,
  //     options: NitroFetchOptions<NitroFetchRequest>,
  //   ) => {
  //     const response = await fetcher(request, options)

  //     //  NO_CONTENT
  //     // if (response.status !== 204) {
  //     //   response = JSONDateToJs(response.body)
  //     // }

  //     return response
  //   }

  // Expose to useNuxtApp().$api
  return {
    provide: {
      api,
    },
  }
})

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

export interface Result<TData> {
  data: TData | null
  errors: Error[] | null
}

export interface Error {
  code: string
  type: ErrorType
  title: string | null
  detail: string | null
  traceId: string | null
  // TODO: errorSource
  stackTrace: string | null
}

export enum ErrorType {
  InternalError = 'InternalError',
  Forbidden = 'Forbidden',
  Conflict = 'Conflict',
  NotFound = 'NotFound',
  Validation = 'Validation',
}

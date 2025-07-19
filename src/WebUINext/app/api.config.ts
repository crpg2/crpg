import { delay } from 'es-toolkit'

import type { CreateClientConfig } from '~/api/client.gen'

import { getToken, login } from '~/services/auth-service'

import type { Platform } from './models/platform'

import { PLATFORM } from './models/platform'

interface CrpgApiError {
  code: string
  type: CrpgApiErrorType
  title: string | null
  detail: string | null
  traceId: string | null
  stackTrace: string | null
}

enum CrpgApiErrorType {
  InternalError = 'InternalError',
  Forbidden = 'Forbidden',
  Conflict = 'Conflict',
  NotFound = 'NotFound',
  Validation = 'Validation',
}

export const createClientConfig: CreateClientConfig = (config) => {
  return ({
    ...config,
    baseURL: import.meta.env.NUXT_PUBLIC_API_BASE_URL,
    async onRequest({ options }) {
      options.headers.set('Authorization', `Bearer ${await getToken()}`)
    },
    async onResponseError({ response }) {
      const toast = useToast()
      const route = useRoute()

      if (route.meta.roles) {
        if (response.status === 401) {
          toast.add({
            title: 'Session expired',
            color: 'error',
            duration: 3000,
            icon: 'crpg:error',
            close: false,
          })
          await delay(1000)
          await login(globalThis.localStorage.getItem('user-platform') as Platform ?? PLATFORM.Steam)
          return
        }
      }

      const [error] = response._data?.errors as CrpgApiError[]

      if (error) {
        toast.add({
          title: error?.title || 'Some Error',
          ...(error.detail && { description: error.detail }),
          color: 'error',
          duration: 5000,
          icon: 'crpg:error',
          close: false,
        })
      }
    },
  })
}

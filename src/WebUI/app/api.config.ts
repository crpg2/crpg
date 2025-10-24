import type { CreateClientConfig } from '#api/client.gen'
import type { FetchResponse } from 'ofetch'

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

export const createClientConfig: CreateClientConfig = (config) => {
  return ({
    ...config,
    baseURL: import.meta.env.NUXT_PUBLIC_API_BASE_URL,
    auth: () => getToken(),
    // TODO: spec
    async onResponseError({ response }) {
      const route = useRoute()
      const toast = useToast()

      if (route.meta.roles && response.status === 401) {
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

      const [error] = (response as FetchResponse<{ errors: CrpgApiError[] }>)._data?.errors ?? []

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

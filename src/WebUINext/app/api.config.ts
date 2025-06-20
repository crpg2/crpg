import type { CreateClientConfig } from '~/api/client.gen'

import { getToken } from '~/services/auth-service'

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

export const createClientConfig: CreateClientConfig = config => ({
  ...config,
  baseURL: 'https://localhost:8000', // TODO: FIXME: from env
  async onRequest({ request, options, error }) {
    // TODO: FIXME: auto refresh token
    options.headers.set('Authorization', `Bearer ${await getToken()}`)
  },
  onResponseError({ response }) {
    const [error] = response._data?.errors as CrpgApiError[]

    if (error) {
      const toast = useToast()
      toast.add({
        title: error?.title || 'Some Error',
        ...(error.detail && { description: error.detail }),
        color: 'error',
        duration: 5000,
        icon: 'crpg:error',
      })
    }

    console.log('error', response._data?.errors as CrpgApiError[])
  },
})

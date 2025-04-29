import { getToken } from '~/services/auth-service'
import type { CreateClientConfig } from '~/api/client.gen'

export const createClientConfig: CreateClientConfig = config => ({
  ...config,
  baseURL: 'https://localhost:8000', // TODO: FIXME:
  async onRequest({ request, options, error }) {
    options.headers.set('Authorization', `Bearer ${await getToken()}`)
  },
})

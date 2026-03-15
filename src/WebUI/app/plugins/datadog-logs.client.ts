import type { Logger } from '@datadog/browser-logs'

export default defineNuxtPlugin<{ logger: Logger }>(async (nuxtApp) => {
  const {
    public: {
      datadog: {
        logs,
      },
    },
  } = useRuntimeConfig()

  if (!logs?.enabled) {
    return
  }

  if (!logs.clientToken) {
    console.warn('[datadog-logs] Missing client token, browser log collection disabled')
    return
  }

  const { datadogLogs } = await import('@datadog/browser-logs')

  datadogLogs.init({
    clientToken: logs.clientToken,
    service: logs.service,
    // version: logs.version, // TODO: from github patches
    forwardConsoleLogs: ['error'],
  })

  nuxtApp.provide('logger', datadogLogs.logger)
})

import type { ConsoleApiName } from '@datadog/browser-core'
import type { LogsInitConfiguration } from '@datadog/browser-logs'

import { datadogLogs } from '@datadog/browser-logs'

const ALLOWED_CONSOLE_LOG_LEVELS = new Set<ConsoleApiName>(['debug', 'error', 'info', 'log', 'warn'])

function parseForwardConsoleLogs(value: unknown): LogsInitConfiguration['forwardConsoleLogs'] {
  if (typeof value !== 'string' || value.trim() === '') {
    return undefined
  }

  if (value === 'all') {
    return 'all'
  }

  const levels = value
    .split(',')
    .map(v => v.trim().toLowerCase())
    .filter((v): v is ConsoleApiName => ALLOWED_CONSOLE_LOG_LEVELS.has(v as ConsoleApiName))

  return levels.length > 0 ? levels : undefined
}

export default defineNuxtPlugin(() => {
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
    console.warn('[datadog-logs] Missing client token or site, browser log collection disabled')
    return
  }

  datadogLogs.init({
    clientToken: logs.clientToken,
    service: logs.service,
    // version: logs.version, // TODO: from github patches
    forwardErrorsToLogs: true,
    forwardConsoleLogs: parseForwardConsoleLogs(logs.forwardConsoleLogs),
  })

  return {
    provide: {
      datadogLogs,
    },
  }
})

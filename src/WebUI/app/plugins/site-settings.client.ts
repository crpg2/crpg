import { updateAppConfig } from '#app'

import { getSettings } from '~/services/settings-service'

export default defineNuxtPlugin(async () => {
  updateAppConfig({ settings: await getSettings() })
})

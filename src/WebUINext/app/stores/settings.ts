import { getSettings } from '~/services/settings-service'

export const useSettingsStore = defineStore('settings', () => {
  const {
    execute: loadSettings,
    state: settings,
    isLoading: isLoadingSettings,
  } = useAsyncState(
    () => getSettings(),
    {
      discord: '',
      steam: '',
      patreon: '',
      github: '',
      reddit: '',
      modDb: '',
    },
    {
      immediate: false,
    },
  )

  return {
    settings,
    loadSettings,
    isLoadingSettings,
  }
})

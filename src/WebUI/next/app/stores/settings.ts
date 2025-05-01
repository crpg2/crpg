import { getSettings } from '#hey-api/sdk.gen'

export const useSettingsStore = defineStore('settings', () => {
  const {
    execute: loadSettings,
    state: settings,
    isLoading: isLoadingSettings,
  } = useAsyncState(
    async () => {
      const { data } = await getSettings({ composable: '$fetch' })
      return data
    },
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

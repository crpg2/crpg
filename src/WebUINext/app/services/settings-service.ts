import { getSettings as _getSettings, patchSettings } from '#hey-api/sdk.gen'

import type { Settings } from '~/models/setting'

export const getSettings = async (): Promise<Settings> => {
  const { data } = await _getSettings({ composable: '$fetch' })
  return data
}

export const editSettings = (setting: Partial<Settings>) => patchSettings({ composable: '$fetch', body: setting })

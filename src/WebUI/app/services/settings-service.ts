import { getSettings as _getSettings, patchSettings } from '#api/sdk.gen'

import type { Settings } from '~/models/setting'

export const getSettings = async (): Promise<Settings> =>
  (await _getSettings({})).data

export const editSettings = (setting: Partial<Settings>) =>
  patchSettings({ body: setting })

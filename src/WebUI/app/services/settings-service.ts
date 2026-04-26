import type { Settings } from '~/models/setting'

import { getSettings as _getSettings, patchSettings } from '#api/sdk.gen'

export const getSettings = async (): Promise<Settings> =>
  (await _getSettings({})).data!

export const editSettings = (setting: Partial<Settings>) =>
  patchSettings({ body: setting })

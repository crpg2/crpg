import type { $Fetch } from 'nitropack'
import type { Settings } from '~/models/setting'

export const getSettings = (fetch: $Fetch) => fetch<Settings>('/settings', { method: 'GET' })

export const editSettings = (fetch: $Fetch, setting: Partial<Settings>) => fetch('/settings', { method: 'PATCH', body: setting })

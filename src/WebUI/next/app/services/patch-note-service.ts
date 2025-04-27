import type { NitroFetchRequest, $Fetch } from 'nitropack'
import type { PatchNote } from '~/models/patch-note'

export const getPatchNotes = <T>(fetch: $Fetch<T, NitroFetchRequest>) => fetch<PatchNote[]>('/patch-notes', { method: 'GET' })

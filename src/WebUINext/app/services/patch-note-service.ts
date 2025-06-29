import { getPatchNotes as _getPatchNotes } from '#hey-api/sdk.gen'

import type { PatchNote } from '~/models/patch-note'

export const getPatchNotes = async (): Promise<PatchNote[]> => {
  const { data } = await _getPatchNotes({ composable: '$fetch' })
  return data!
}

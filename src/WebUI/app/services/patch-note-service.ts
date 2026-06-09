import type { PatchNote } from '~/models/patch-note'

import { getPatchNotes as _getPatchNotes } from '#api/sdk.gen'

export const getPatchNotes = async (): Promise<PatchNote[]> =>
  (await _getPatchNotes({ })).data!

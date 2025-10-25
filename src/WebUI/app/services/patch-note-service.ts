import { getPatchNotes as _getPatchNotes } from '#api/sdk.gen'

import type { PatchNote } from '~/models/patch-note'

export const getPatchNotes = async (): Promise<PatchNote[]> =>
  (await _getPatchNotes({ })).data!

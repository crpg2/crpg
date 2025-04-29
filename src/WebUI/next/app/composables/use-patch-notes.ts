// import { getPatchNotes } from '~/services/patch-note-service'
import { getPatchNotes } from '#hey-api/sdk.gen'

export const usePatchNotes = () => {
  const { $api } = useNuxtApp()

  const {
    execute: loadPatchNotes,
    state: patchNotes,
  } = useAsyncState(async () => {
    const res = await getPatchNotes({ composable: '$fetch' },
    )
    return res.data
  },
  [], {
    immediate: false,
  })

  return {
    loadPatchNotes,
    patchNotes,
  }
}

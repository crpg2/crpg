import { getPatchNotes } from '~/services/patch-note-service'

export const usePatchNotes = (immediate = true) => {
  const {
    execute: loadPatchNotes,
    state: patchNotes,
  } = useAsyncState(() => getPatchNotes(), [], { immediate })

  return {
    loadPatchNotes,
    patchNotes,
  }
}

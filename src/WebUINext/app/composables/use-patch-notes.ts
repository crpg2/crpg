import { getPatchNotes } from '~/services/patch-note-service'

export const usePatchNotes = () => {
  const {
    execute: loadPatchNotes,
    state: patchNotes,
  } = useAsyncState(() => getPatchNotes(), [], { immediate: false })

  return {
    loadPatchNotes,
    patchNotes,
  }
}

import { getPatchNotes } from '~/services/patch-note-service'

export const usePatchNotes = () => {
  return useAsyncDataCustom(
    ['patchNotes'],
    () => getPatchNotes(),
    {
      default: () => [],
      loadingIndicator: false,
      poll: false,
    },
  )
}

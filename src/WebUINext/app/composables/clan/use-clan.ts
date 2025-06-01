import { useAsyncState } from '@vueuse/core'

import { getClan } from '~/services/clan-service'

export const useClan = () => {
  const { state: clan, execute: loadClan, isLoading: loadingClan } = useAsyncState(
    ({ id }: { id: number }) => getClan(id),
    null,
    {
      immediate: false,
    },
  )

  return {
    clan,
    loadClan,
    loadingClan,
  }
}

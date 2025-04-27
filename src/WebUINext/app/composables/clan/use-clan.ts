import { useAsyncState } from '@vueuse/core'

import type { ClanUpdate } from '~/models/clan'

import {
  updateClan as _updateClan,
  getClan,
} from '~/services/clan-service'

export const useClan = (clanId: MaybeRefOrGetter<number>) => {
  const { state: clan, execute: loadClan, isLoading: loadingClan } = useAsyncState(
    () => getClan(toValue(clanId)),
    null,
    {
      immediate: false,
    },
  )

  const updateClan = (clan: ClanUpdate) => _updateClan(toValue(clanId), clan)

  return {
    clan,
    loadClan,
    loadingClan,
    updateClan,
  }
}

import { getAsyncData, refreshAsyncData, useRoute } from '#imports'

import type { Clan, ClanUpdate } from '~/models/clan'

import { CLAN_QUERY_KEYS } from '~/queries'
import { updateClan as _updateClan } from '~/services/clan-service'

export const useClan = () => {
  const route = useRoute('clans-id')
  const _key = CLAN_QUERY_KEYS.byId(Number(route.params.id))

  const clan = getAsyncData<Clan>(_key)
  const refreshClan = refreshAsyncData(_key)

  const updateClan = (data: ClanUpdate) => _updateClan(clan.value.id, data)

  return {
    clan,
    refreshClan,
    updateClan,
  }
}

import type { LMap } from '@vue-leaflet/vue-leaflet'
import type { Map } from 'leaflet'

import { useIntervalFn, useToggle } from '@vueuse/core'

import type { Party, PartyCommon, StrategusUpdate, UpdatePartyStatus } from '~/models/strategus/party'

import { useAsyncCallback } from '~/composables/utils/use-async-callback'
import { PARTY_STATUS } from '~/models/strategus/party'
import { PARTY_QUERY_KEYS } from '~/queries'
import { getSelfUpdate, IN_SETTLEMENT_PARTY_STATUSES, updatePartyStatus } from '~/services/strategus/party-service'

// const INTERVAL = 1000 * 60 ; // 1 min
const INTERVAL = 10000 // TODO:

// велосипед?
export const usePartyState = (data?: StrategusUpdate): Ref<StrategusUpdate> => {
  const state = useState<StrategusUpdate | null>('party')

  if (data) {
    state.value = data
  }

  if (state.value === null) {
    throw createError({ statusMessage: 'PartyInfo not provided' })
  }

  return state as Ref<StrategusUpdate>
}

export const useParty = (
  // TODO: provide map?
  // map: Ref<typeof LMap | null>,
) => {
  const partyInfo = usePartyState()
  const route = useRoute()

  const updateParty = async () => {
    partyInfo.value = (await getSelfUpdate()).data!

    const { party } = partyInfo.value

    // вынести в функции
    if (party.targetedSettlement
      && IN_SETTLEMENT_PARTY_STATUSES.includes(party.status)
      && route.name !== 'strategus-settlement-id'
    ) {
      await navigateTo({
        name: 'strategus-settlement-id',
        params: { id: party.targetedSettlement.id },
      })
    }

    // вынести в функции
    if (party.targetedBattle
      && party.status === PARTY_STATUS.InBattle
      && route.name !== 'strategus-battle-id'
    ) {
      await navigateTo({
        name: 'strategus-battle-id',
        params: { id: party.targetedBattle.id },
      })
    }
  }

  const { resume: startUpdatePartyInterval } = useIntervalFn(updateParty, INTERVAL, { immediate: false })

  const partySpawn = () => {
    startUpdatePartyInterval()
  }

  const moveParty = async (updateRequest: Partial<UpdatePartyStatus>) => {
    partyInfo.value.party = await updatePartyStatus({
      status: PARTY_STATUS.MovingToPoint,
      targetedPartyId: 0,
      targetedSettlementId: 0,
      targetedBattletId: 0,
      battleJoinIntents: [],
      waypoints: { coordinates: [], type: 'MultiPoint' },
      ...updateRequest,
    })
  }

  // TODO: move to party action
  const [toggleRecruitTroops, isTogglingRecruitTroops] = useAsyncCallback(
    async () => {
      // if (party.value === null || party.value.targetedSettlement === null) {
      //   return
      // }

      // party.value = await updatePartyStatus({
      //   status:
      //     party.value.status !== PARTY_STATUS.RecruitingInSettlement
      //       ? PARTY_STATUS.RecruitingInSettlement
      //       : PARTY_STATUS.IdleInSettlement,
      //   targetedPartyId: 0,
      //   targetedSettlementId: party.value.targetedSettlement.id,
      //   waypoints: { coordinates: [], type: 'MultiPoint' },
      // })
    },
  )

  return {
    partyInfo,
    updateParty,
    moveParty,
    partySpawn,

    toggleRecruitTroops,
    isTogglingRecruitTroops,

  }
}

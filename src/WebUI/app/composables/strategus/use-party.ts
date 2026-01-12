import type { LMap } from '@vue-leaflet/vue-leaflet'
import type { Map } from 'leaflet'

import { useIntervalFn, useToggle } from '@vueuse/core'

import type { Party, PartyCommon, StrategusUpdate } from '~/models/strategus/party'

import { useAsyncCallback } from '~/composables/utils/use-async-callback'
import { PARTY_STATUS } from '~/models/strategus/party'
import { PARTY_QUERY_KEYS } from '~/queries'
import { getSelfUpdate, updatePartyStatus } from '~/services/strategus/party-service'

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
// export const usePartyProvider = (data: StrategusUpdate) => {

//   usePartyState().value = data

//   // return useState<StrategusUpdate>(toCacheKey(PARTY_QUERY_KEYS.root), () => data)
//   // return {
//   //   party: toRef(() => partyInfo.value.party),
//   //   visibleParties: toRef(() => partyInfo.value.visibleParties),
//   //   visibleSettlements: toRef(() => partyInfo.value.visibleSettlements),
//   //   visibleBattles: toRef(() => partyInfo.value.visibleBattles),
//   // }
// }

export const useParty = (
  map: Ref<typeof LMap | null>,
) => {
  const partyInfo = usePartyState()

  // const party = getAsyncData<Character[]>(PARTY_QUERY_KEYS.root)
  // const updateParty = refreshAsyncData(PARTY_QUERY_KEYS.root)
  const updateParty = async () => {
    partyInfo.value = (await getSelfUpdate()).data!
  }

  // const party = ref<Party | null>(null)

  const [isRegistered, toggleRegistered] = useToggle(true)
  // const visibleParties = ref<PartyCommon[]>([])

  // const updateParty = async () => {
  //   const res = await getSelfUpdate()

  //   // TODO: Not registered to Strategus.
  //   if (res?.errors !== null) {
  //     toggleRegistered(false)
  //     return
  //   }

  //   if (res.data === null) {
  //     return
  //   }

  //   party.value = res.data.party
  //   visibleParties.value = res.data.visibleParties
  // }

  const { resume: startUpdatePartyInterval } = useIntervalFn(updateParty, INTERVAL, { immediate: false })

  const partySpawn = async () => {
    await updateParty()
    if (partyInfo.value.party === null) {
      return
    }
    startUpdatePartyInterval()
  }

  const onRegistered = () => {
    toggleRegistered(true)
    partySpawn()
  }

  const moveParty = async (
    // updateRequest: Partial<PartyStatusUpdateRequest>
  ) => {
    // if (party.value === null) {
    //   return
    // }
    // party.value = await updatePartyStatus({
    //   status: PARTY_STATUS.MovingToPoint,
    //   targetedPartyId: 0,
    //   targetedSettlementId: 0,
    //   waypoints: { coordinates: [], type: 'MultiPoint' },
    //   // ...updateRequest,
    // })
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

  const flyToSelfParty = () => {
    if (!partyInfo.value.party || !map.value) {
      return
    }
    (map.value.leafletObject as Map).flyTo(positionToLatLng(partyInfo.value.party.position.coordinates), 5, {
      animate: false,
    })
  }

  return {
    isRegistered,

    partyInfo,
    moveParty,
    onRegistered,
    partySpawn,
    flyToSelfParty,

    toggleRecruitTroops,
    isTogglingRecruitTroops,

    updateParty,
    // visibleParties,
  }
}

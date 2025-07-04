import { useIntervalFn, useToggle } from '@vueuse/core'

import type {
  Party,
  PartyCommon,
  PartyStatusUpdateRequest,
} from '~/models/strategus/party'

import { useAsyncCallback } from '~/composables/utils/use-async-callback'
import { PartyStatus } from '~/models/strategus/party'
import { getUpdate, updatePartyStatus } from '~/services/strategus-service'

// Shared state
// TODO: FIXME: refactoring
// TODO: https://vueuse.org/shared/createGlobalState/
const party = ref<Party | null>(null)

// const INTERVAL = 1000 * 60 ; // 1 min
const INTERVAL = 10000 // TODO:

export const useParty = () => {
  const [isRegistered, toggleRegistered] = useToggle(true)
  const visibleParties = ref<PartyCommon[]>([])

  const updateParty = async () => {
    const res = await getUpdate()

    // TODO: Not registered to Strategus.
    if (res?.errors !== null) {
      toggleRegistered(false)
      return
    }

    if (res.data === null) {
      return
    }

    party.value = res.data.party
    visibleParties.value = res.data.visibleParties
  }

  const { resume: startUpdatePartyInterval } = useIntervalFn(updateParty, INTERVAL, {
    immediate: false,
  })

  const moveParty = async (updateRequest: Partial<PartyStatusUpdateRequest>) => {
    if (party.value === null) { return }

    party.value = await updatePartyStatus({
      status: PartyStatus.MovingToPoint,
      targetedPartyId: 0,
      targetedSettlementId: 0,
      waypoints: { coordinates: [], type: 'MultiPoint' },
      ...updateRequest,
    })
  }

  const { execute: toggleRecruitTroops, loading: isTogglingRecruitTroops } = useAsyncCallback(
    async () => {
      if (party.value === null || party.value.targetedSettlement === null) { return }

      party.value = await updatePartyStatus({
        status:
          party.value.status !== PartyStatus.RecruitingInSettlement
            ? PartyStatus.RecruitingInSettlement
            : PartyStatus.IdleInSettlement,
        targetedPartyId: 0,
        targetedSettlementId: party.value.targetedSettlement.id,
        waypoints: { coordinates: [], type: 'MultiPoint' },
      })
    },
  )

  const onRegistered = () => {
    toggleRegistered(true)
    partySpawn()
  }

  const partySpawn = async () => {
    await updateParty()
    if (party.value === null) { return }
    startUpdatePartyInterval()
  }

  return {
    isRegistered,

    party,
    moveParty,
    onRegistered,
    partySpawn,

    toggleRecruitTroops,
    isTogglingRecruitTroops,

    updateParty,
    visibleParties,
  }
}

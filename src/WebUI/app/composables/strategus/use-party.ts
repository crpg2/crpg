import { useIntervalFn } from '@vueuse/core'

import type { PartyOrder, StrategusUpdate, UpdatePartyOrder } from '~/models/strategus/party'

import { useAsyncCallback } from '~/composables/utils/use-async-callback'
import { BATTLE_SIDE } from '~/models/strategus/battle'
import { PARTY_ORDER_TYPE, PARTY_STATUS } from '~/models/strategus/party'
import {
  getSelfUpdate,
  mapPartyOrderToUpdateOrder,
  shouldPartyBeInBattle,
  shouldPartyBeInSettlement,
  UNMOVABLE_PARTY_STATUSES,
  updatePartyOrders,
} from '~/services/strategus/party-service'

// const INTERVAL = 1000 * 60 ; // 1 min // TODO: to env
const INTERVAL = 10_000 // TODO:

export const usePartyState = (strict: boolean = true): {
  partyState: Ref<StrategusUpdate>
  setPartyState: (data: StrategusUpdate) => void
} => {
  const state = useState<StrategusUpdate | null>('party')

  if (strict && state.value === null) {
    throw createError({ statusMessage: 'PartyInfo not provided' })
  }

  const setPartyState = (data: StrategusUpdate) => {
    state.value = data
  }

  return {
    partyState: state as Ref<StrategusUpdate>,
    setPartyState,
  }
}

export const useParty = (
  // TODO: provide map?
  // map: Ref<typeof LMap | null>,
) => {
  const { partyState, setPartyState } = usePartyState()
  const route = useRoute()

  const updateParty = async () => {
    setPartyState((await getSelfUpdate()).data!)

    const { party } = partyState.value

    const _shouldPartyBeInSettlement = shouldPartyBeInSettlement(party)
    const _shouldPartyBeInBattle = shouldPartyBeInBattle(party)

    if (_shouldPartyBeInSettlement && !route.meta.groups?.includes('strategussettlement')
    ) {
      await navigateTo({
        name: 'strategus-settlement-id',
        params: { id: party.currentSettlement!.id },
      })
    }

    if (_shouldPartyBeInBattle && !route.meta.groups?.includes('strategusbattle')
    ) {
      await navigateTo({
        name: 'strategus-battle-id',
        params: { id: party.currentBattle!.id },
      })
    }

    // TODO: подумать еще, но вроде ок
    // don't let users go where they shouldn't
    if (!_shouldPartyBeInSettlement && !_shouldPartyBeInBattle && route.name !== 'strategus') {
      await navigateTo({ name: 'strategus' })
    }
  }

  const { resume: startUpdatePartyInterval } = useIntervalFn(updateParty, INTERVAL, { immediate: false })

  const partySpawn = () => {
    startUpdatePartyInterval()
  }

  const setPartyOrder = async (updateRequest: Partial<UpdatePartyOrder>, chain: boolean = false) => {
    const order: UpdatePartyOrder = {
      type: PARTY_ORDER_TYPE.MoveToPoint,
      orderIndex: 0,
      waypoints: { coordinates: [], type: 'MultiPoint' },
      targetedPartyId: 0,
      targetedSettlementId: 0,
      targetedBattleId: 0,
      battleJoinIntents: [],
      transferOfferPartyIntent: null,
      ...updateRequest,
    }

    const newPartyState = await updatePartyOrders(
      chain
        ? [
            ...partyState.value.party.orders.map(mapPartyOrderToUpdateOrder),
            {
              ...order,
              orderIndex: partyState.value.party.orders.length,
            },
          ]
        : [order],
    )

    setPartyState(newPartyState)
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

  const toast = useToast()

  const validateCanMove = () => {
    if (UNMOVABLE_PARTY_STATUSES.includes(partyState.value.party.status)) {
      toast.add({
        description: 'Вы не можете двигаться TODO:',
        color: 'warning',
      })
      return false
    }

    return true
  }

  return {
    partyState,
    updateParty,
    setPartyOrder,
    partySpawn,
    validateCanMove,

    toggleRecruitTroops,
    isTogglingRecruitTroops,
  }
}

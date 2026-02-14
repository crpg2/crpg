import {
  getPartiesByPartyIdItems,
  getPartiesSelfItems,
  getPartiesSelfUpdate,
  postParties,
  putPartiesSelfOrders,
  putPartiesSelfTransferOffersByTransferOfferId,
  // putPartiesSelfStatus,
} from '#api/sdk.gen'

import type { CrpgApiResult } from '~/api.config'
import type { ItemStack, Party, PartyOrder, PartyStatus, StrategusUpdate, TransferOfferPartyUpdate, UpdatePartyOrder } from '~/models/strategus/party'

import { PARTY_STATUS } from '~/models/strategus/party'

export const getSelfUpdate = (): Promise<CrpgApiResult<StrategusUpdate>> => getPartiesSelfUpdate({})

// TODO:
// export const updatePartyStatus = async (payload: UpdatePartyStatus): Promise<Party> => (await putPartiesSelfStatus({ body: payload })).data!

export const updatePartyOrders = async (orders: UpdatePartyOrder[]): Promise<StrategusUpdate> => (await putPartiesSelfOrders({ body: { orders } })).data!

export const registerParty = () => postParties({ body: {} })

export const getSelfPartyItems = async (): Promise<ItemStack[]> => (await getPartiesSelfItems({ })).data!

export const getPartyItems = async (
  partyId: number,
): Promise<ItemStack[]> => (await getPartiesByPartyIdItems({ path: { partyId } })).data!

export const respondToPartyTransferOffer = async (
  transferOfferId: number,
  accept: boolean,
  accepted?: TransferOfferPartyUpdate,
): Promise<void> => {
  await putPartiesSelfTransferOffersByTransferOfferId({ path: { transferOfferId }, body: { accept, accepted } })
}

export const IN_SETTLEMENT_PARTY_STATUSES: PartyStatus[] = [
  PARTY_STATUS.IdleInSettlement,
  PARTY_STATUS.RecruitingInSettlement,
]

export const IN_BATTLE_PARTY_STATUSES: PartyStatus[] = [
  PARTY_STATUS.InBattle,
  PARTY_STATUS.AwaitingBattleJoinDecision, // You can attack at a party in this status.
]

export const UNMOVABLE_PARTY_STATUSES: PartyStatus[] = [
  PARTY_STATUS.IdleInSettlement,
  PARTY_STATUS.RecruitingInSettlement,
  PARTY_STATUS.AwaitingBattleJoinDecision,
  PARTY_STATUS.InBattle,
  PARTY_STATUS.AwaitingPartyOfferDecision,
]

export const shouldPartyBeInSettlement = (party: Party) => party.currentSettlement && IN_SETTLEMENT_PARTY_STATUSES.includes(party.status)

export const shouldPartyBeInBattle = (party: Party) => party.currentBattle && IN_BATTLE_PARTY_STATUSES.includes(party.status)

export function mapPartyOrderToUpdateOrder(order: PartyOrder): UpdatePartyOrder {
  const { type, orderIndex, waypoints, targetedParty, targetedSettlement, targetedBattle, battleJoinIntents } = order
  return {
    type,
    orderIndex,
    waypoints,
    targetedPartyId: targetedParty?.id || 0,
    targetedSettlementId: targetedSettlement?.id || 0,
    targetedBattleId: targetedBattle?.id || 0,
    battleJoinIntents,
    transferOfferPartyIntent: order.transferOfferPartyIntent
      ? {
          gold: order.transferOfferPartyIntent.gold,
          troops: order.transferOfferPartyIntent.troops,
          items: order.transferOfferPartyIntent.items.map(itemStack => ({
            count: itemStack.count,
            itemId: itemStack.item.id,
          })),
        }
      : null,
  }
}

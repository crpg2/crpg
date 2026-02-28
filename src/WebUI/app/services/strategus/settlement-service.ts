import {
  getSettlements as _getSettlements,
  getSettlementsBySettlementId,
  getSettlementsBySettlementIdItems,
  getSettlementsBySettlementIdShopItems,
  putSettlementsBySettlementId,
  putSettlementsBySettlementIdItems,
} from '#api/sdk.gen'

import type { ItemStack, ItemStackUpdate } from '~/models/strategus/party'
import type { SettlementPublic, SettlementType } from '~/models/strategus/settlement'

import { SETTLEMENT_TYPE } from '~/models/strategus/settlement'

export const settlementIconByType: Record<SettlementType, string> = {
  [SETTLEMENT_TYPE.Town]: 'settlement-type-town',
  [SETTLEMENT_TYPE.Castle]: 'settlement-type-castle',
  [SETTLEMENT_TYPE.Village]: 'settlement-type-village',
}

export const getSettlements = async () => (await _getSettlements({})).data!

export const getSettlement = async (settlementId: number): Promise<SettlementPublic> => (await getSettlementsBySettlementId({ path: { settlementId } })).data!

export const getSettlementItems = async (settlementId: number): Promise<ItemStack[]> => (await getSettlementsBySettlementIdItems({ path: { settlementId } })).data!

export const updateSettlementResources = async (settlementId: number, troops: number): Promise<SettlementPublic> => (await putSettlementsBySettlementId({
  path: { settlementId },
  body: { troops },
})).data!

export const updateSettlementItems = async (settlementId: number, items: ItemStackUpdate[]) => putSettlementsBySettlementIdItems({
  path: { settlementId },
  body: { items },
})

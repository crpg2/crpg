import {
  getSettlements as _getSettlements,
  getSettlementsBySettlementId,
} from '#api/sdk.gen'

import type { SettlementPublic, SettlementType } from '~/models/strategus/settlement'

import { SETTLEMENT_TYPE } from '~/models/strategus/settlement'

export const settlementIconByType: Record<SettlementType, string> = {
  [SETTLEMENT_TYPE.Town]: 'settlement-type-town',
  [SETTLEMENT_TYPE.Castle]: 'settlement-type-castle',
  [SETTLEMENT_TYPE.Village]: 'settlement-type-village',
}

export const getSettlements = async () => (await _getSettlements({})).data!

export const getSettlement = async (settlementId: number): Promise<SettlementPublic> => (await getSettlementsBySettlementId({ path: { settlementId } })).data!

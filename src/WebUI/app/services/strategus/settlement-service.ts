import type { SettlementType } from '~/models/strategus/settlement'

import { SETTLEMENT_TYPE } from '~/models/strategus/settlement'

export const settlementIconByType: Record<SettlementType, string> = {
  [SETTLEMENT_TYPE.Town]: 'settlement-type-town',
  [SETTLEMENT_TYPE.Castle]: 'settlement-type-castle',
  [SETTLEMENT_TYPE.Village]: 'settlement-type-village',
}

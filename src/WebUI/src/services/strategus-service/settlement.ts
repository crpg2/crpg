import type { SettlementItem, SettlementPublic } from '~/models/strategus/settlement'

import { SettlementType } from '~/models/strategus/settlement'
import { get, post } from '~/services/crpg-client'

export const settlementIconByType: Record<
  SettlementType,
  {
    icon: string
    iconSize: string
  }
> = {
  [SettlementType.Castle]: {
    icon: 'castle',
    iconSize: 'sm',
  },
  [SettlementType.Town]: {
    icon: 'town',
    iconSize: 'lg',
  },
  [SettlementType.Village]: {
    icon: 'village',
    iconSize: 'sm',
  },
}

export const getSettlements = () => get<SettlementPublic[]>('/settlements')

export const getSettlement = (id: number) => get<SettlementPublic>(`/settlements/${id}`)

export const getSettlementGarrisonItems = (id: number) => get<SettlementItem[]>(`/settlements/${id}/items`)

export interface SettlementGarrisonItemsUpdate {
  itemId: string
  count: number
}

export const updateSettlementGarrisonItems = (id: number, payload: SettlementGarrisonItemsUpdate) =>
  post<SettlementItem>(`/settlements/${id}/items`, payload)

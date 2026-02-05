import type { LatLngBounds } from 'leaflet'

import type { SettlementPublic } from '~/models/strategus/settlement'

import { SETTLEMENT_TYPE } from '~/models/strategus/settlement'

export const shouldDisplaySettlement = (
  settlement: SettlementPublic,
  mapBounds: LatLngBounds,
  zoom: number,
) => {
  if (!mapBounds.contains(positionToLatLng(settlement.position.coordinates))) {
    return false
  }

  //   TODO: to constant
  return (
    zoom > 4
    || (zoom > 3 && settlement.type === SETTLEMENT_TYPE.Castle)
    || settlement.type === SETTLEMENT_TYPE.Town
  )
}

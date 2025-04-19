import type { LatLngBounds } from 'leaflet'

import type { SettlementPublic } from '~/models/strategus/settlement'

import { SettlementType } from '~/models/strategus/settlement'
import { positionToLatLng } from '~/utils/geometry'

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
    || (zoom > 3 && settlement.type === SettlementType.Castle)
    || settlement.type === SettlementType.Town
  )
}

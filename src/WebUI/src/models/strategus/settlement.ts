import type { Point } from 'geojson'

import type { PartyCommon } from '~/models/strategus/party'

export enum SettlementType {
  Village = 'Village',
  Castle = 'Castle',
  Town = 'Town',
}

export interface SettlementPublic {
  id: number
  name: string
  scene: string
  region: string
  culture: string
  position: Point
  type: SettlementType
  owner: PartyCommon | null
}

import type {
  SettlementType as _SettlementType,
} from '#api'
import type { Point } from 'geojson'
import type { ValueOf } from 'type-fest'

import type { Culture } from '~/models/culture'
import type { Item } from '~/models/item'
import type { Region } from '~/models/region'
import type { UserPublic } from '~/models/user'

export const SETTLEMENT_TYPE = {
  Village: 'Village',
  Castle: 'Castle',
  Town: 'Town',
} as const satisfies Record<_SettlementType, _SettlementType>

export type SettlementType = ValueOf<typeof SETTLEMENT_TYPE>

export interface SettlementPublic {
  id: number
  name: string
  type: SettlementType
  culture: Culture
  position: Point
  region: Region
  troops: number
  owner: UserPublic | null
}

export interface SettlementItem {
  item: Item
  count: number
}

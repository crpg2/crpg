import type { ValueOf } from 'type-fest'

import type { Region as _Region } from '~/api'

export const REGION = {
  Eu: 'Eu',
  Na: 'Na',
  As: 'As',
  Oc: 'Oc',
} as const satisfies Record<_Region, _Region>

export type Region = ValueOf<typeof REGION>

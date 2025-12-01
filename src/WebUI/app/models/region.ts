import type { Region as _Region } from '#api'
import type { ValueOf } from 'type-fest'

export const REGION = {
  Eu: 'Eu',
  Na: 'Na',
  As: 'As',
  Oc: 'Oc',
} as const satisfies Record<_Region, _Region>

export type Region = ValueOf<typeof REGION>

import type { Culture as _Culture } from '#api'
import type { ValueOf } from 'type-fest'

export const CULTURE = {
  Neutral: 'Neutral',
  Aserai: 'Aserai',
  Battania: 'Battania',
  Empire: 'Empire',
  Khuzait: 'Khuzait',
  Looters: 'Looters',
  Sturgia: 'Sturgia',
  Vlandia: 'Vlandia',
} as const satisfies Record<_Culture, _Culture>

export type Culture = ValueOf<typeof CULTURE>

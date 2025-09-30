import type { Culture } from '~/models/culture'

import { CULTURE } from '~/models/culture'

export const cultureToIcon: Record<Culture, string> = {
  [CULTURE.Aserai]: 'culture-aserai',
  [CULTURE.Battania]: 'culture-battania',
  [CULTURE.Empire]: 'culture-empire',
  [CULTURE.Khuzait]: 'culture-khuzait',
  [CULTURE.Looters]: 'culture-looters',
  [CULTURE.Neutral]: 'culture-neutrals',
  [CULTURE.Sturgia]: 'culture-sturgia',
  [CULTURE.Vlandia]: 'culture-vlandia',
}

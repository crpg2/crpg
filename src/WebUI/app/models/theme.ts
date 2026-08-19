import type { ValueOf } from 'type-fest'

import type { ThemeEquipmentSlot as _ThemeEquipmentSlot } from '#api'

export type { ThemeEventViewModel, ThemeViewModel } from '#api'

export const THEME_EQUIPMENT_SLOT = {
  Head: 'Head',
  Shoulder: 'Shoulder',
  Body: 'Body',
  Hand: 'Hand',
  Leg: 'Leg',
  MountHarness: 'MountHarness',
  Mount: 'Mount',
  Weapon: 'Weapon',
} as const satisfies Record<_ThemeEquipmentSlot, _ThemeEquipmentSlot>

export type ThemeEquipmentSlot = ValueOf<typeof THEME_EQUIPMENT_SLOT>

export const THEME_EQUIPMENT_SLOTS: ThemeEquipmentSlot[] = Object.values(THEME_EQUIPMENT_SLOT)

export interface ThemeEventFormData {
  name: string
  themeId: number
  goldMultiplier: number
  expMultiplier: number
  activeFromUtc: Date
  activeUntilUtc: Date | null
  requiredEquipmentSlotsMatchingTheme: ThemeEquipmentSlot[]
  minimumThemedItemsEquipped: number | null
}

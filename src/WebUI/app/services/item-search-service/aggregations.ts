import type { FilterFnOption } from '@tanstack/vue-table'
import type { ValueOf } from 'type-fest'

import type { ItemFieldCompareRule, ItemFieldFormat, ItemFlat, ItemType, WeaponClass } from '~/models/item'

import {
  ITEM_FIELD_COMPARE_RULE,
  ITEM_FIELD_FORMAT,
  ITEM_TYPE,
  WEAPON_CLASS,
} from '~/models/item'

export interface AggregationOptions {
  view: AggregationView
  format?: ItemFieldFormat
  hidden?: boolean
  compareRule?: ItemFieldCompareRule
  width?: number // px
}

export type AggregationConfig = Partial<Record<keyof ItemFlat, AggregationOptions>>

export const AGGREGATION_VIEW = {
  Range: 'Range',
  Checkbox: 'Checkbox',
  Toggle: 'Toggle',
} as const

export type AggregationView = ValueOf<typeof AGGREGATION_VIEW>

export const aggregationsConfig: AggregationConfig = {
  culture: {
    view: AGGREGATION_VIEW.Checkbox,
  },
  flags: {
    format: ITEM_FIELD_FORMAT.List,
    view: AGGREGATION_VIEW.Checkbox,
    width: 160,
  },
  id: {
    view: AGGREGATION_VIEW.Checkbox,
    hidden: true,
  },
  modId: {
    view: AGGREGATION_VIEW.Checkbox,
    hidden: true,
  },
  isNew: {
    view: AGGREGATION_VIEW.Toggle,
    hidden: true,
  },
  price: {
    compareRule: ITEM_FIELD_COMPARE_RULE.Less,
    format: ITEM_FIELD_FORMAT.Number,
    view: AGGREGATION_VIEW.Range,
    width: 200,
  },
  requirement: {
    compareRule: ITEM_FIELD_COMPARE_RULE.Less,
    format: ITEM_FIELD_FORMAT.Requirement,
    view: AGGREGATION_VIEW.Range,
  },
  tier: {
    compareRule: ITEM_FIELD_COMPARE_RULE.Bigger,
    format: ITEM_FIELD_FORMAT.Number,
    view: AGGREGATION_VIEW.Range,
  },
  type: {
    view: AGGREGATION_VIEW.Toggle,
    format: ITEM_FIELD_FORMAT.String,
    hidden: true,
  },
  upkeep: {
    compareRule: ITEM_FIELD_COMPARE_RULE.Less,
    format: ITEM_FIELD_FORMAT.Number,
    view: AGGREGATION_VIEW.Range,
  },
  weight: {
    compareRule: ITEM_FIELD_COMPARE_RULE.Less,
    format: ITEM_FIELD_FORMAT.Number,
    view: AGGREGATION_VIEW.Range,
  },

  // Armor
  armArmor: {
    compareRule: ITEM_FIELD_COMPARE_RULE.Bigger,
    format: ITEM_FIELD_FORMAT.Number,
    view: AGGREGATION_VIEW.Range,
  },
  armorFamilyType: {
    format: ITEM_FIELD_FORMAT.Number,
    view: AGGREGATION_VIEW.Checkbox,
  },
  armorMaterialType: {
    format: ITEM_FIELD_FORMAT.List,
    view: AGGREGATION_VIEW.Checkbox,
  },
  bodyArmor: {
    compareRule: ITEM_FIELD_COMPARE_RULE.Bigger,
    format: ITEM_FIELD_FORMAT.Number,
    view: AGGREGATION_VIEW.Range,
  },
  headArmor: {
    compareRule: ITEM_FIELD_COMPARE_RULE.Bigger,
    format: ITEM_FIELD_FORMAT.Number,
    view: AGGREGATION_VIEW.Range,
  },
  legArmor: {
    compareRule: ITEM_FIELD_COMPARE_RULE.Bigger,
    format: ITEM_FIELD_FORMAT.Number,
    view: AGGREGATION_VIEW.Range,
  },

  // Mount
  bodyLength: {
    format: ITEM_FIELD_FORMAT.Number,
    view: AGGREGATION_VIEW.Range,
  },
  chargeDamage: {
    compareRule: ITEM_FIELD_COMPARE_RULE.Bigger,
    format: ITEM_FIELD_FORMAT.Number,
    view: AGGREGATION_VIEW.Range,
  },
  hitPoints: {
    compareRule: ITEM_FIELD_COMPARE_RULE.Bigger,
    format: ITEM_FIELD_FORMAT.Number,
    view: AGGREGATION_VIEW.Range,
  },
  maneuver: {
    compareRule: ITEM_FIELD_COMPARE_RULE.Bigger,
    format: ITEM_FIELD_FORMAT.Number,
    view: AGGREGATION_VIEW.Range,
  },
  mountFamilyType: {
    format: ITEM_FIELD_FORMAT.Number,
    view: AGGREGATION_VIEW.Checkbox,
  },
  speed: {
    compareRule: ITEM_FIELD_COMPARE_RULE.Bigger,
    format: ITEM_FIELD_FORMAT.Number,
    view: AGGREGATION_VIEW.Range,
  },

  // Mount armor
  mountArmor: {
    compareRule: ITEM_FIELD_COMPARE_RULE.Bigger,
    format: ITEM_FIELD_FORMAT.Number,
    view: AGGREGATION_VIEW.Range,
  },
  mountArmorFamilyType: {
    view: AGGREGATION_VIEW.Checkbox,
  },

  // Weapon
  handling: {
    compareRule: ITEM_FIELD_COMPARE_RULE.Bigger,
    format: ITEM_FIELD_FORMAT.Number,
    view: AGGREGATION_VIEW.Range,
  },
  length: {
    compareRule: ITEM_FIELD_COMPARE_RULE.Bigger,
    format: ITEM_FIELD_FORMAT.Number,
    view: AGGREGATION_VIEW.Range,
  },
  swingDamage: {
    compareRule: ITEM_FIELD_COMPARE_RULE.Bigger,
    format: ITEM_FIELD_FORMAT.Damage,
    view: AGGREGATION_VIEW.Range,
  },
  swingDamageType: {
    view: AGGREGATION_VIEW.Checkbox,
  },
  swingSpeed: {
    compareRule: ITEM_FIELD_COMPARE_RULE.Bigger,
    format: ITEM_FIELD_FORMAT.Number,
    view: AGGREGATION_VIEW.Range,
  },
  thrustDamage: {
    compareRule: ITEM_FIELD_COMPARE_RULE.Bigger,
    format: ITEM_FIELD_FORMAT.Damage,
    view: AGGREGATION_VIEW.Range,
  },
  thrustDamageType: {
    view: AGGREGATION_VIEW.Checkbox,
  },
  thrustSpeed: {
    compareRule: ITEM_FIELD_COMPARE_RULE.Bigger,
    format: ITEM_FIELD_FORMAT.Number,
    view: AGGREGATION_VIEW.Range,
  },
  weaponClass: {
    view: AGGREGATION_VIEW.Toggle,
    format: ITEM_FIELD_FORMAT.String,
    hidden: true,
  },
  weaponUsage: {
    view: AGGREGATION_VIEW.Checkbox,
    // hidden: true, // TODO: FIXME:
  },

  // Throw/Bow/Xbow
  accuracy: {
    compareRule: ITEM_FIELD_COMPARE_RULE.Bigger,
    format: ITEM_FIELD_FORMAT.Number,
    view: AGGREGATION_VIEW.Range,
  },
  missileSpeed: {
    compareRule: ITEM_FIELD_COMPARE_RULE.Bigger,
    format: ITEM_FIELD_FORMAT.Number,
    view: AGGREGATION_VIEW.Range,
  },

  // Bow/Xbow
  aimSpeed: {
    compareRule: ITEM_FIELD_COMPARE_RULE.Bigger,
    format: ITEM_FIELD_FORMAT.Number,
    view: AGGREGATION_VIEW.Range,
  },
  reloadSpeed: {
    compareRule: ITEM_FIELD_COMPARE_RULE.Bigger,
    format: ITEM_FIELD_FORMAT.Number,
    view: AGGREGATION_VIEW.Range,
  },

  // Arrows/Bolts/Thrown
  damage: {
    compareRule: ITEM_FIELD_COMPARE_RULE.Bigger,
    format: ITEM_FIELD_FORMAT.Damage,
    view: AGGREGATION_VIEW.Range,
  },
  damageType: {
    view: AGGREGATION_VIEW.Checkbox,
  },
  stackAmount: {
    compareRule: ITEM_FIELD_COMPARE_RULE.Bigger,
    format: ITEM_FIELD_FORMAT.Number,
    view: AGGREGATION_VIEW.Range,
  },
  stackWeight: {
    compareRule: ITEM_FIELD_COMPARE_RULE.Less,
    format: ITEM_FIELD_FORMAT.Number,
    view: AGGREGATION_VIEW.Range,
  },

  // SHIELD
  shieldArmor: {
    compareRule: ITEM_FIELD_COMPARE_RULE.Bigger,
    format: ITEM_FIELD_FORMAT.Number,
    view: AGGREGATION_VIEW.Range,
  },
  shieldDurability: {
    compareRule: ITEM_FIELD_COMPARE_RULE.Bigger,
    format: ITEM_FIELD_FORMAT.Number,
    view: AGGREGATION_VIEW.Range,
  },
  shieldSpeed: {
    compareRule: ITEM_FIELD_COMPARE_RULE.Bigger,
    format: ITEM_FIELD_FORMAT.Number,
    view: AGGREGATION_VIEW.Range,
  },
}

const _tail: Array<keyof ItemFlat> = [
  'upkeep',
  'price',
]

export const aggregationsKeysByItemType: Partial<Record<ItemType, Array<keyof ItemFlat>>> = {
  [ITEM_TYPE.BodyArmor]: [
    'armorFamilyType',
    'culture',
    'flags',
    'armorMaterialType',
    'weight',
    'bodyArmor',
    'armArmor',
    'legArmor',
    ..._tail,
  ],
  [ITEM_TYPE.HandArmor]: [
    'culture',
    'flags',
    'armorMaterialType',
    'weight',
    'armArmor',
    ..._tail,
  ],
  [ITEM_TYPE.HeadArmor]: [
    'culture',
    'flags',
    'armorMaterialType',
    'weight',
    'headArmor',
    ..._tail,
  ],
  [ITEM_TYPE.LegArmor]: [
    'armorFamilyType',
    'culture',
    'armorMaterialType',
    'weight',
    'legArmor',
    ..._tail,
  ],
  [ITEM_TYPE.Mount]: [
    'culture',
    'mountFamilyType',
    'bodyLength',
    'chargeDamage',
    'maneuver',
    'speed',
    'hitPoints',
    ..._tail,
  ],
  [ITEM_TYPE.MountHarness]: [
    'culture',
    'mountArmorFamilyType',
    'armorMaterialType',
    'weight',
    'mountArmor',
    ..._tail,
  ],
  [ITEM_TYPE.OneHandedWeapon]: [
    'weaponUsage',
    'flags',
    'weight',
    'length',
    'handling',
    'thrustDamage',
    'thrustSpeed',
    'swingDamage',
    'swingSpeed',
    ..._tail,
  ],
  [ITEM_TYPE.Polearm]: [
    'weaponUsage',
    'flags',
    'weight',
    'length',
    'handling',
    'thrustDamage',
    'thrustSpeed',
    'swingDamage',
    'swingSpeed',
    ..._tail,
  ],
  [ITEM_TYPE.Shield]: [
    'flags',
    'weight',
    'length',
    'shieldSpeed',
    'shieldDurability',
    'shieldArmor',
    ..._tail,
  ],
  [ITEM_TYPE.ShoulderArmor]: [
    'culture',
    'flags',
    'armorMaterialType',
    'weight',
    'headArmor',
    'bodyArmor',
    'armArmor',
    ..._tail,
  ],
  [ITEM_TYPE.Thrown]: [
    'damage',
    'missileSpeed',
    'stackWeight',
    'stackAmount',
    ..._tail,
  ],
  [ITEM_TYPE.TwoHandedWeapon]: [
    'flags',
    'weight',
    'length',
    'handling',
    'thrustDamage',
    'thrustSpeed',
    'swingDamage',
    'swingSpeed',
    ..._tail,
  ],
  [ITEM_TYPE.Ammo]: [
    'damageType',
    'damage',
    'stackWeight',
    'stackAmount',
    ..._tail,
  ],
  // banners are all the same, no need for aggregation
  [ITEM_TYPE.Banner]: [
    'flags',
    'weight',
    'culture',
    ..._tail,
  ],
}

export const aggregationsKeysByWeaponClass: Partial<Record<WeaponClass, Array<keyof ItemFlat>>> = {
  [WEAPON_CLASS.Bow]: [
    'flags',
    'weight',
    'damage',
    'accuracy',
    'missileSpeed',
    'reloadSpeed',
    'aimSpeed',
    ..._tail,
  ],
  [WEAPON_CLASS.Crossbow]: [
    'flags',
    'weight',
    'damage',
    'accuracy',
    'missileSpeed',
    'reloadSpeed',
    'aimSpeed',
    'requirement',
    ..._tail,
  ],
  [WEAPON_CLASS.Musket]: [
    'flags',
    'weight',
    'damage',
    'accuracy',
    'missileSpeed',
    'reloadSpeed',
    'aimSpeed',
    'requirement',
    ..._tail,
  ],
  [WEAPON_CLASS.Pistol]: [
    'flags',
    'weight',
    'damage',
    'accuracy',
    'missileSpeed',
    'reloadSpeed',
    'aimSpeed',
    'requirement',
    ..._tail,
  ],
  [WEAPON_CLASS.Dagger]: [
    'length',
    'weight',
    'handling',
    'thrustDamage',
    'thrustSpeed',
    'swingDamage',
    'swingSpeed',
    ..._tail,
  ],
  [WEAPON_CLASS.Javelin]: [
    'flags',
    'damage',
    'missileSpeed',
    'stackWeight',
    'stackAmount',
    ..._tail,
  ],
  [WEAPON_CLASS.Mace]: [
    'flags',
    'weight',
    'length',
    'handling',
    'thrustDamage',
    'thrustSpeed',
    'swingDamage',
    'swingSpeed',
    ..._tail,
  ],
  [WEAPON_CLASS.OneHandedAxe]: [
    'flags',
    'weight',
    'weaponUsage',
    'length',
    'handling',
    'swingDamage',
    'swingSpeed',
    'upkeep',
    'price',
  ],
  [WEAPON_CLASS.OneHandedSword]: [
    'weaponUsage',
    'weight',
    'length',
    'handling',
    'thrustDamage',
    'thrustSpeed',
    'swingDamage',
    'swingSpeed',
    ..._tail,
  ],
  [WEAPON_CLASS.ThrowingAxe]: [
    'flags',
    'damage',
    'missileSpeed',
    'stackWeight',
    ..._tail,
  ],
  [WEAPON_CLASS.ThrowingKnife]: [
    'damage',
    'weaponUsage',
    'missileSpeed',
    'stackWeight',
    'stackAmount',
    ..._tail,
  ],
  [WEAPON_CLASS.TwoHandedAxe]: [
    'flags',
    'weight',
    'length',
    'handling',
    'swingDamage',
    'swingSpeed',
    ..._tail,
  ],
  [WEAPON_CLASS.TwoHandedMace]: [
    'flags',
    'weight',
    'length',
    'handling',
    'swingDamage',
    'swingSpeed',
    ..._tail,
  ],
}

export function getFilterFn(options: AggregationOptions): FilterFnOption<any> {
  if (options.view === AGGREGATION_VIEW.Toggle && options.format === ITEM_FIELD_FORMAT.String) {
    return 'equalsString'
  }

  if (options.view === AGGREGATION_VIEW.Range) {
    return 'inNumberRange'
  }

  if (options.view === AGGREGATION_VIEW.Checkbox) {
    if (options.format === ITEM_FIELD_FORMAT.List) {
      return 'arrIncludesSome'
    }

    return includesSome
  }

  return 'auto'
}

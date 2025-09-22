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
}

export type AggregationConfig = Partial<Record<keyof ItemFlat, AggregationOptions>>

export enum AggregationView {
  Range = 'Range',
  Checkbox = 'Checkbox',
  Radio = 'Radio',
}

export const aggregationsConfig: AggregationConfig = {
  culture: {
    view: AggregationView.Checkbox,
  },
  flags: {
    format: ITEM_FIELD_FORMAT.List,
    view: AggregationView.Checkbox,
  },
  id: {
    view: AggregationView.Checkbox,
    hidden: true,
  },
  modId: {
    view: AggregationView.Checkbox,
    hidden: true,
  },
  isNew: {
    view: AggregationView.Checkbox,
    hidden: true,
  },
  price: {
    compareRule: ITEM_FIELD_COMPARE_RULE.Less,
    format: ITEM_FIELD_FORMAT.Number,
    view: AggregationView.Range,
  },
  requirement: {
    compareRule: ITEM_FIELD_COMPARE_RULE.Less,
    format: ITEM_FIELD_FORMAT.Requirement,
    view: AggregationView.Range,
  },
  tier: {
    compareRule: ITEM_FIELD_COMPARE_RULE.Bigger,
    format: ITEM_FIELD_FORMAT.Number,
    view: AggregationView.Range,
  },
  type: {
    view: AggregationView.Radio,
    hidden: true,
  },
  upkeep: {
    compareRule: ITEM_FIELD_COMPARE_RULE.Less,
    format: ITEM_FIELD_FORMAT.Number,
    view: AggregationView.Range,
  },
  weight: {
    compareRule: ITEM_FIELD_COMPARE_RULE.Less,
    format: ITEM_FIELD_FORMAT.Number,
    view: AggregationView.Range,
  },

  // Armor
  armArmor: {
    compareRule: ITEM_FIELD_COMPARE_RULE.Bigger,
    format: ITEM_FIELD_FORMAT.Number,
    view: AggregationView.Range,
  },
  armorFamilyType: {
    format: ITEM_FIELD_FORMAT.List,
    view: AggregationView.Checkbox,
  },
  armorMaterialType: {
    format: ITEM_FIELD_FORMAT.List,
    view: AggregationView.Checkbox,
  },
  bodyArmor: {
    compareRule: ITEM_FIELD_COMPARE_RULE.Bigger,
    format: ITEM_FIELD_FORMAT.Number,
    view: AggregationView.Range,
  },
  headArmor: {
    compareRule: ITEM_FIELD_COMPARE_RULE.Bigger,
    format: ITEM_FIELD_FORMAT.Number,
    view: AggregationView.Range,
  },
  legArmor: {
    compareRule: ITEM_FIELD_COMPARE_RULE.Bigger,
    format: ITEM_FIELD_FORMAT.Number,
    view: AggregationView.Range,
  },

  // Mount
  bodyLength: {
    format: ITEM_FIELD_FORMAT.Number,
    view: AggregationView.Range,
  },
  chargeDamage: {
    compareRule: ITEM_FIELD_COMPARE_RULE.Bigger,
    format: ITEM_FIELD_FORMAT.Number,
    view: AggregationView.Range,
  },
  hitPoints: {
    compareRule: ITEM_FIELD_COMPARE_RULE.Bigger,
    format: ITEM_FIELD_FORMAT.Number,
    view: AggregationView.Range,
  },
  maneuver: {
    compareRule: ITEM_FIELD_COMPARE_RULE.Bigger,
    format: ITEM_FIELD_FORMAT.Number,
    view: AggregationView.Range,
  },
  mountFamilyType: {
    view: AggregationView.Checkbox,
  },
  speed: {
    compareRule: ITEM_FIELD_COMPARE_RULE.Bigger,
    format: ITEM_FIELD_FORMAT.Number,
    view: AggregationView.Range,
  },

  // Mount armor
  mountArmor: {
    compareRule: ITEM_FIELD_COMPARE_RULE.Bigger,
    format: ITEM_FIELD_FORMAT.Number,
    view: AggregationView.Range,
  },
  mountArmorFamilyType: {
    view: AggregationView.Checkbox,
  },

  // Weapon
  handling: {
    compareRule: ITEM_FIELD_COMPARE_RULE.Bigger,
    format: ITEM_FIELD_FORMAT.Number,
    view: AggregationView.Range,
  },
  length: {
    compareRule: ITEM_FIELD_COMPARE_RULE.Bigger,
    format: ITEM_FIELD_FORMAT.Number,
    view: AggregationView.Range,
  },
  swingDamage: {
    compareRule: ITEM_FIELD_COMPARE_RULE.Bigger,
    format: ITEM_FIELD_FORMAT.Damage,
    view: AggregationView.Range,
  },
  swingDamageType: {
    view: AggregationView.Checkbox,
  },
  swingSpeed: {
    compareRule: ITEM_FIELD_COMPARE_RULE.Bigger,
    format: ITEM_FIELD_FORMAT.Number,
    view: AggregationView.Range,
  },
  thrustDamage: {
    compareRule: ITEM_FIELD_COMPARE_RULE.Bigger,
    format: ITEM_FIELD_FORMAT.Damage,
    view: AggregationView.Range,
  },
  thrustDamageType: {
    view: AggregationView.Checkbox,
  },
  thrustSpeed: {
    compareRule: ITEM_FIELD_COMPARE_RULE.Bigger,
    format: ITEM_FIELD_FORMAT.Number,
    view: AggregationView.Range,
  },
  weaponClass: {
    view: AggregationView.Radio,
    hidden: true,
  },
  weaponUsage: {
    view: AggregationView.Checkbox,
    hidden: true,
  },

  // Throw/Bow/Xbow
  accuracy: {
    compareRule: ITEM_FIELD_COMPARE_RULE.Bigger,
    format: ITEM_FIELD_FORMAT.Number,
    view: AggregationView.Range,
  },
  missileSpeed: {
    compareRule: ITEM_FIELD_COMPARE_RULE.Bigger,
    format: ITEM_FIELD_FORMAT.Number,
    view: AggregationView.Range,
  },

  // Bow/Xbow
  aimSpeed: {
    compareRule: ITEM_FIELD_COMPARE_RULE.Bigger,
    format: ITEM_FIELD_FORMAT.Number,
    view: AggregationView.Range,
  },
  reloadSpeed: {
    compareRule: ITEM_FIELD_COMPARE_RULE.Bigger,
    format: ITEM_FIELD_FORMAT.Number,
    view: AggregationView.Range,
  },

  // Arrows/Bolts/Thrown
  damage: {
    compareRule: ITEM_FIELD_COMPARE_RULE.Bigger,
    format: ITEM_FIELD_FORMAT.Damage,
    view: AggregationView.Range,
  },
  damageType: {
    view: AggregationView.Checkbox,
  },
  stackAmount: {
    compareRule: ITEM_FIELD_COMPARE_RULE.Bigger,
    format: ITEM_FIELD_FORMAT.Number,
    view: AggregationView.Range,
  },
  stackWeight: {
    compareRule: ITEM_FIELD_COMPARE_RULE.Less,
    format: ITEM_FIELD_FORMAT.Number,
    view: AggregationView.Range,
  },

  // SHIELD
  shieldArmor: {
    compareRule: ITEM_FIELD_COMPARE_RULE.Bigger,
    format: ITEM_FIELD_FORMAT.Number,
    view: AggregationView.Range,
  },
  shieldDurability: {
    compareRule: ITEM_FIELD_COMPARE_RULE.Bigger,
    format: ITEM_FIELD_FORMAT.Number,
    view: AggregationView.Range,
  },
  shieldSpeed: {
    compareRule: ITEM_FIELD_COMPARE_RULE.Bigger,
    format: ITEM_FIELD_FORMAT.Number,
    view: AggregationView.Range,
  },
}

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
    'upkeep',
    'price',
  ],
  [ITEM_TYPE.HandArmor]: [
    'culture',
    'flags',
    'armorMaterialType',
    'weight',
    'armArmor',
    'upkeep',
    'price',
  ],
  [ITEM_TYPE.HeadArmor]: [
    'culture',
    'flags',
    'armorMaterialType',
    'weight',
    'headArmor',
    'upkeep',
    'price',
  ],
  [ITEM_TYPE.LegArmor]: [
    'armorFamilyType',
    'culture',
    'armorMaterialType',
    'weight',
    'legArmor',
    'upkeep',
    'price',
  ],
  [ITEM_TYPE.Mount]: [
    'culture',
    'mountFamilyType',
    'bodyLength',
    'chargeDamage',
    'maneuver',
    'speed',
    'hitPoints',
    'upkeep',
    'price',
  ],
  [ITEM_TYPE.MountHarness]: [
    'culture',
    'mountArmorFamilyType',
    'armorMaterialType',
    'weight',
    'mountArmor',
    'upkeep',
    'price',
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
    'upkeep',
    'price',
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
    'upkeep',
    'price',
  ],
  [ITEM_TYPE.Shield]: [
    'flags',
    'weight',
    'length',
    'shieldSpeed',
    'shieldDurability',
    'shieldArmor',
    'upkeep',
    'price',
  ],
  [ITEM_TYPE.ShoulderArmor]: [
    'culture',
    'flags',
    'armorMaterialType',
    'weight',
    'headArmor',
    'bodyArmor',
    'armArmor',
    'upkeep',
    'price',
  ],
  [ITEM_TYPE.Thrown]: [
    'damage',
    'missileSpeed',
    'stackWeight',
    'stackAmount',
    'upkeep',
    'price',
  ],
  [ITEM_TYPE.TwoHandedWeapon]: [
    // 'flags',
    // 'weight',
    // 'length',
    'handling',
    // 'thrustDamage',
    // 'thrustSpeed',
    // 'swingDamage',
    // 'swingSpeed',
    // 'upkeep',
    'price',
  ],
  // banners are all the same, no need for aggregation
  [ITEM_TYPE.Banner]: [
    'flags',
    'weight',
    'culture',
    'upkeep',
    'price',
  ],
}

export const aggregationsKeysByWeaponClass: Partial<Record<WeaponClass, Array<keyof ItemFlat>>> = {
  [WEAPON_CLASS.Arrow]: [
    'damageType',
    'damage',
    'stackWeight',
    'stackAmount',
    'upkeep',
    'price',
  ],
  [WEAPON_CLASS.Bolt]: [
    'damageType',
    'damage',
    'stackWeight',
    'stackAmount',
    'upkeep',
    'price',
  ],
  [WEAPON_CLASS.Cartridge]: [
    'damageType',
    'damage',
    'weight',
    'stackAmount',
    'upkeep',
    'price',
  ],
  [WEAPON_CLASS.Bow]: [
    'flags',
    'weight',
    'damage',
    'accuracy',
    'missileSpeed',
    'reloadSpeed',
    'aimSpeed',
    'upkeep',
    'price',
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
    'upkeep',
    'price',
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
    'upkeep',
    'price',
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
    'upkeep',
    'price',
  ],
  [WEAPON_CLASS.Dagger]: [
    'length',
    'weight',
    'handling',
    'thrustDamage',
    'thrustSpeed',
    'swingDamage',
    'swingSpeed',
    'upkeep',
    'price',
  ],
  [WEAPON_CLASS.Javelin]: [
    'flags',
    'damage',
    'missileSpeed',
    'stackWeight',
    'stackAmount',
    'upkeep',
    'price',
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
    'upkeep',
    'price',
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
    'upkeep',
    'price',
  ],
  [WEAPON_CLASS.ThrowingAxe]: [
    'flags',
    'damage',
    'missileSpeed',
    'stackWeight',
    'stackAmount',
    'upkeep',
    'price',
  ],
  [WEAPON_CLASS.ThrowingKnife]: [
    'damage',
    'weaponUsage',
    'missileSpeed',
    'stackWeight',
    'stackAmount',
    'upkeep',
    'price',
  ],
  [WEAPON_CLASS.TwoHandedAxe]: [
    'flags',
    'weight',
    'length',
    'handling',
    'swingDamage',
    'swingSpeed',
    'upkeep',
    'price',
  ],
  [WEAPON_CLASS.TwoHandedMace]: [
    'flags',
    'weight',
    'length',
    'handling',
    'swingDamage',
    'swingSpeed',
    'upkeep',
    'price',
  ],
}

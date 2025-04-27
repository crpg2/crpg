import type { ItemFlat } from '~/models/item'

import {
  ItemFieldCompareRule,
  ItemFieldFormat,
  ItemType,
  WeaponClass,
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
    format: ItemFieldFormat.List,
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
  new: {
    view: AggregationView.Checkbox,
    hidden: true,
  },
  price: {
    compareRule: ItemFieldCompareRule.Less,
    format: ItemFieldFormat.Number,
    view: AggregationView.Range,
  },
  requirement: {
    compareRule: ItemFieldCompareRule.Less,
    format: ItemFieldFormat.Requirement,
    view: AggregationView.Range,
  },
  tier: {
    compareRule: ItemFieldCompareRule.Bigger,
    format: ItemFieldFormat.Number,
    view: AggregationView.Range,
  },
  type: {
    view: AggregationView.Radio,
    hidden: true,
  },
  upkeep: {
    compareRule: ItemFieldCompareRule.Less,
    format: ItemFieldFormat.Number,
    view: AggregationView.Range,
  },
  weight: {
    compareRule: ItemFieldCompareRule.Less,
    format: ItemFieldFormat.Number,
    view: AggregationView.Range,
  },

  // Armor
  armArmor: {
    compareRule: ItemFieldCompareRule.Bigger,
    format: ItemFieldFormat.Number,
    view: AggregationView.Range,
  },
  armorFamilyType: {
    format: ItemFieldFormat.List,
    view: AggregationView.Checkbox,
  },
  armorMaterialType: {
    format: ItemFieldFormat.List,
    view: AggregationView.Checkbox,
  },
  bodyArmor: {
    compareRule: ItemFieldCompareRule.Bigger,
    format: ItemFieldFormat.Number,
    view: AggregationView.Range,
  },
  headArmor: {
    compareRule: ItemFieldCompareRule.Bigger,
    format: ItemFieldFormat.Number,
    view: AggregationView.Range,
  },
  legArmor: {
    compareRule: ItemFieldCompareRule.Bigger,
    format: ItemFieldFormat.Number,
    view: AggregationView.Range,
  },

  // Mount
  bodyLength: {
    format: ItemFieldFormat.Number,
    view: AggregationView.Range,
  },
  chargeDamage: {
    compareRule: ItemFieldCompareRule.Bigger,
    format: ItemFieldFormat.Number,
    view: AggregationView.Range,
  },
  hitPoints: {
    compareRule: ItemFieldCompareRule.Bigger,
    format: ItemFieldFormat.Number,
    view: AggregationView.Range,
  },
  maneuver: {
    compareRule: ItemFieldCompareRule.Bigger,
    format: ItemFieldFormat.Number,
    view: AggregationView.Range,
  },
  mountFamilyType: {
    view: AggregationView.Checkbox,
  },
  speed: {
    compareRule: ItemFieldCompareRule.Bigger,
    format: ItemFieldFormat.Number,
    view: AggregationView.Range,
  },

  // Mount armor
  mountArmor: {
    compareRule: ItemFieldCompareRule.Bigger,
    format: ItemFieldFormat.Number,
    view: AggregationView.Range,
  },
  mountArmorFamilyType: {
    view: AggregationView.Checkbox,
  },

  // Weapon
  handling: {
    compareRule: ItemFieldCompareRule.Bigger,
    format: ItemFieldFormat.Number,
    view: AggregationView.Range,
  },
  length: {
    compareRule: ItemFieldCompareRule.Bigger,
    format: ItemFieldFormat.Number,
    view: AggregationView.Range,
  },
  swingDamage: {
    compareRule: ItemFieldCompareRule.Bigger,
    format: ItemFieldFormat.Damage,
    view: AggregationView.Range,
  },
  swingDamageType: {
    view: AggregationView.Checkbox,
  },
  swingSpeed: {
    compareRule: ItemFieldCompareRule.Bigger,
    format: ItemFieldFormat.Number,
    view: AggregationView.Range,
  },
  thrustDamage: {
    compareRule: ItemFieldCompareRule.Bigger,
    format: ItemFieldFormat.Damage,
    view: AggregationView.Range,
  },
  thrustDamageType: {
    view: AggregationView.Checkbox,
  },
  thrustSpeed: {
    compareRule: ItemFieldCompareRule.Bigger,
    format: ItemFieldFormat.Number,
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
    compareRule: ItemFieldCompareRule.Bigger,
    format: ItemFieldFormat.Number,
    view: AggregationView.Range,
  },
  missileSpeed: {
    compareRule: ItemFieldCompareRule.Bigger,
    format: ItemFieldFormat.Number,
    view: AggregationView.Range,
  },

  // Bow/Xbow
  aimSpeed: {
    compareRule: ItemFieldCompareRule.Bigger,
    format: ItemFieldFormat.Number,
    view: AggregationView.Range,
  },
  reloadSpeed: {
    compareRule: ItemFieldCompareRule.Bigger,
    format: ItemFieldFormat.Number,
    view: AggregationView.Range,
  },

  // Arrows/Bolts/Thrown
  damage: {
    compareRule: ItemFieldCompareRule.Bigger,
    format: ItemFieldFormat.Damage,
    view: AggregationView.Range,
  },
  damageType: {
    view: AggregationView.Checkbox,
  },
  stackAmount: {
    compareRule: ItemFieldCompareRule.Bigger,
    format: ItemFieldFormat.Number,
    view: AggregationView.Range,
  },
  stackWeight: {
    compareRule: ItemFieldCompareRule.Less,
    format: ItemFieldFormat.Number,
    view: AggregationView.Range,
  },

  // SHIELD
  shieldArmor: {
    compareRule: ItemFieldCompareRule.Bigger,
    format: ItemFieldFormat.Number,
    view: AggregationView.Range,
  },
  shieldDurability: {
    compareRule: ItemFieldCompareRule.Bigger,
    format: ItemFieldFormat.Number,
    view: AggregationView.Range,
  },
  shieldSpeed: {
    compareRule: ItemFieldCompareRule.Bigger,
    format: ItemFieldFormat.Number,
    view: AggregationView.Range,
  },
}

export const aggregationsKeysByItemType: Partial<Record<ItemType, Array<keyof ItemFlat>>> = {
  [ItemType.BodyArmor]: [
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
  [ItemType.HandArmor]: [
    'culture',
    'flags',
    'armorMaterialType',
    'weight',
    'armArmor',
    'upkeep',
    'price',
  ],
  [ItemType.HeadArmor]: [
    'culture',
    'flags',
    'armorMaterialType',
    'weight',
    'headArmor',
    'upkeep',
    'price',
  ],
  [ItemType.LegArmor]: [
    'armorFamilyType',
    'culture',
    'armorMaterialType',
    'weight',
    'legArmor',
    'upkeep',
    'price',
  ],
  [ItemType.Mount]: [
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
  [ItemType.MountHarness]: [
    'culture',
    'mountArmorFamilyType',
    'armorMaterialType',
    'weight',
    'mountArmor',
    'upkeep',
    'price',
  ],
  [ItemType.OneHandedWeapon]: [
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
  [ItemType.Polearm]: [
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
  [ItemType.Shield]: [
    'flags',
    'weight',
    'length',
    'shieldSpeed',
    'shieldDurability',
    'shieldArmor',
    'upkeep',
    'price',
  ],
  [ItemType.ShoulderArmor]: [
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
  [ItemType.Thrown]: [
    'damage',
    'missileSpeed',
    'stackWeight',
    'stackAmount',
    'upkeep',
    'price',
  ],
  [ItemType.TwoHandedWeapon]: [
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
  // banners are all the same, no need for aggregation
  [ItemType.Banner]: [
    'flags',
    'weight',
    'culture',
    'upkeep',
    'price',
  ],
}

export const aggregationsKeysByWeaponClass: Partial<Record<WeaponClass, Array<keyof ItemFlat>>> = {
  [WeaponClass.Arrow]: [
    'damageType',
    'damage',
    'stackWeight',
    'stackAmount',
    'upkeep',
    'price',
  ],
  [WeaponClass.Bolt]: [
    'damageType',
    'damage',
    'stackWeight',
    'stackAmount',
    'upkeep',
    'price',
  ],
  [WeaponClass.Cartridge]: [
    'damageType',
    'damage',
    'weight',
    'stackAmount',
    'upkeep',
    'price',
  ],
  [WeaponClass.Bow]: [
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
  [WeaponClass.Crossbow]: [
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
  [WeaponClass.Musket]: [
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
  [WeaponClass.Pistol]: [
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
  [WeaponClass.Dagger]: [
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
  [WeaponClass.Javelin]: [
    'flags',
    'damage',
    'missileSpeed',
    'stackWeight',
    'stackAmount',
    'upkeep',
    'price',
  ],
  [WeaponClass.Mace]: [
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
  [WeaponClass.OneHandedAxe]: [
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
  [WeaponClass.OneHandedSword]: [
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
  [WeaponClass.ThrowingAxe]: [
    'flags',
    'damage',
    'missileSpeed',
    'stackWeight',
    'stackAmount',
    'upkeep',
    'price',
  ],
  [WeaponClass.ThrowingKnife]: [
    'damage',
    'weaponUsage',
    'missileSpeed',
    'stackWeight',
    'stackAmount',
    'upkeep',
    'price',
  ],
  [WeaponClass.TwoHandedAxe]: [
    'flags',
    'weight',
    'length',
    'handling',
    'swingDamage',
    'swingSpeed',
    'upkeep',
    'price',
  ],
  [WeaponClass.TwoHandedMace]: [
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

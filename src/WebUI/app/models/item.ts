import type {
  ArmorMaterialType as _ArmorMaterialType,
  DamageType as _DamageType,
  ItemFlags as _ItemFlags,
  ItemSlot as _ItemSlot,
  ItemType as _ItemType,
  WeaponClass as _WeaponClass,
  WeaponFlags as _WeaponFlags,
} from '#api'
import type { ValueOf } from 'type-fest'

import type { Culture } from '~/models/culture'

export const ARMOR_MATERIAL_TYPE = {
  Undefined: 'Undefined',
  Cloth: 'Cloth',
  Leather: 'Leather',
  Chainmail: 'Chainmail',
  Plate: 'Plate',
} as const satisfies Record<_ArmorMaterialType, _ArmorMaterialType>

export type ArmorMaterialType = ValueOf<typeof ARMOR_MATERIAL_TYPE>

export interface ItemArmorComponent {
  armArmor: number
  legArmor: number
  headArmor: number
  bodyArmor: number
  familyType: ItemFamilyType
  materialType: ArmorMaterialType
}

export interface ItemMountComponent {
  speed: number
  maneuver: number
  hitPoints: number
  bodyLength: number
  chargeDamage: number
  familyType: ItemFamilyType
}

export const WEAPON_CLASS = {
  Undefined: 'Undefined',
  OneHandedSword: 'OneHandedSword',
  TwoHandedSword: 'TwoHandedSword',
  OneHandedAxe: 'OneHandedAxe',
  TwoHandedAxe: 'TwoHandedAxe',
  Mace: 'Mace',
  Dagger: 'Dagger',
  Pick: 'Pick',
  TwoHandedMace: 'TwoHandedMace',
  TwoHandedPolearm: 'TwoHandedPolearm',
  OneHandedPolearm: 'OneHandedPolearm',
  LowGripPolearm: 'LowGripPolearm',
  Arrow: 'Arrow',
  Bolt: 'Bolt',
  Cartridge: 'Cartridge',
  Bullets: 'Bullets',
  Bow: 'Bow',
  Crossbow: 'Crossbow',
  Boulder: 'Boulder',
  Javelin: 'Javelin',
  ThrowingAxe: 'ThrowingAxe',
  ThrowingKnife: 'ThrowingKnife',
  Stone: 'Stone',
  Pistol: 'Pistol',
  Musket: 'Musket',
  SmallShield: 'SmallShield',
  LargeShield: 'LargeShield',
  Banner: 'Banner',
} as const satisfies Record<_WeaponClass, _WeaponClass>

export type WeaponClass = ValueOf<typeof WEAPON_CLASS>

export const WEAPON_FLAG = {
  MeleeWeapon: 'MeleeWeapon',
  RangedWeapon: 'RangedWeapon',
  FirearmAmmo: 'FirearmAmmo',
  NotUsableWithOneHand: 'NotUsableWithOneHand',
  NotUsableWithTwoHand: 'NotUsableWithTwoHand',
  WideGrip: 'WideGrip',
  AttachAmmoToVisual: 'AttachAmmoToVisual',
  Consumable: 'Consumable',
  HasHitPoints: 'HasHitPoints',
  HasString: 'HasString',
  StringHeldByHand: 'StringHeldByHand',
  UnloadWhenSheathed: 'UnloadWhenSheathed',
  AffectsArea: 'AffectsArea',
  AffectsAreaBig: 'AffectsAreaBig',
  Burning: 'Burning',
  BonusAgainstShield: 'BonusAgainstShield',
  CanPenetrateShield: 'CanPenetrateShield',
  CantReloadOnHorseback: 'CantReloadOnHorseback',
  CanReloadOnHorseback: 'CanReloadOnHorseback', // custom flag
  CantUseOnHorseback: 'CantUseOnHorseback', // custom flag
  AutoReload: 'AutoReload',
  TwoHandIdleOnMount: 'TwoHandIdleOnMount',
  NoBlood: 'NoBlood',
  PenaltyWithShield: 'PenaltyWithShield',
  CanDismount: 'CanDismount',
  CanHook: 'CanHook',
  MissileWithPhysics: 'MissileWithPhysics',
  MultiplePenetration: 'MultiplePenetration',
  CanKnockDown: 'CanKnockDown',
  CanBlockRanged: 'CanBlockRanged',
  LeavesTrail: 'LeavesTrail',
  CanCrushThrough: 'CanCrushThrough',
  UseHandAsThrowBase: 'UseHandAsThrowBase',
  AmmoBreaksOnBounceBack: 'AmmoBreaksOnBounceBack',
  AmmoCanBreakOnBounceBack: 'AmmoCanBreakOnBounceBack',
  AmmoSticksWhenShot: 'AmmoSticksWhenShot',
} as const satisfies Record<_WeaponFlags, _WeaponFlags>

export type WeaponFlag = ValueOf<typeof WEAPON_FLAG>

export const DAMAGE_TYPE = {
  Undefined: 'Undefined',
  Cut: 'Cut',
  Pierce: 'Pierce',
  Blunt: 'Blunt',
} as const satisfies Record<_DamageType, _DamageType>

export type DamageType = ValueOf<typeof DAMAGE_TYPE>

export interface ItemWeaponComponent {
  length: number
  accuracy: number
  balance: number
  handling: number
  bodyArmor: number
  class: WeaponClass
  swingSpeed: number
  stackAmount: number
  thrustSpeed: number
  swingDamage: number
  itemUsage: ItemUsage
  missileSpeed: number
  flags: WeaponFlag[]
  thrustDamage: number
  swingDamageType: DamageType
  thrustDamageType: DamageType
}

export const ITEM_FLAG = {
  ForceAttachOffHandPrimaryItemBone: 'ForceAttachOffHandPrimaryItemBone',
  ForceAttachOffHandSecondaryItemBone: 'ForceAttachOffHandSecondaryItemBone',
  NotUsableByFemale: 'NotUsableByFemale',
  NotUsableByMale: 'NotUsableByMale',
  DropOnWeaponChange: 'DropOnWeaponChange',
  DropOnAnyAction: 'DropOnAnyAction',
  CannotBePickedUp: 'CannotBePickedUp',
  CanBePickedUpFromCorpse: 'CanBePickedUpFromCorpse',
  QuickFadeOut: 'QuickFadeOut',
  WoodenAttack: 'WoodenAttack',
  WoodenParry: 'WoodenParry',
  HeldInOffHand: 'HeldInOffHand',
  HasToBeHeldUp: 'HasToBeHeldUp',
  UseTeamColor: 'UseTeamColor',
  Civilian: 'Civilian',
  DoNotScaleBodyAccordingToWeaponLength: 'DoNotScaleBodyAccordingToWeaponLength',
  DoesNotHideChest: 'DoesNotHideChest',
  NotStackable: 'NotStackable',
} as const satisfies Record<_ItemFlags, _ItemFlags>

export type ItemFlag = ValueOf<typeof ITEM_FLAG>

export const ITEM_TYPE = {
  Undefined: 'Undefined',
  OneHandedWeapon: 'OneHandedWeapon',
  TwoHandedWeapon: 'TwoHandedWeapon',
  Polearm: 'Polearm',
  Thrown: 'Thrown',
  Ranged: 'Ranged', // virtual
  Ammo: 'Ammo', // virtual
  Bow: 'Bow',
  Crossbow: 'Crossbow',
  Pistol: 'Pistol',
  Musket: 'Musket',
  Arrows: 'Arrows',
  Bolts: 'Bolts',
  Bullets: 'Bullets',
  Shield: 'Shield',
  HeadArmor: 'HeadArmor',
  ShoulderArmor: 'ShoulderArmor',
  BodyArmor: 'BodyArmor',
  HandArmor: 'HandArmor',
  LegArmor: 'LegArmor',
  Mount: 'Mount',
  MountHarness: 'MountHarness',
  Banner: 'Banner',
} as const satisfies Record<_ItemType, _ItemType>

export type ItemType = ValueOf<typeof ITEM_TYPE>

export const ITEM_USAGE = {
  LongBow: 'long_bow',
  Bow: 'bow',
  Crossbow: 'crossbow',
  CrossbowLight: 'crossbow_light',
  PolearmCouch: 'polearm_couch',
  PolearmBracing: 'polearm_bracing',
  PolearmPike: 'polearm_pike',
  Polearm: 'polearm', // jousting lance
} as const

export type ItemUsage = ValueOf<typeof ITEM_USAGE>

export const ITEM_FAMILY_TYPE = {
  Undefined: 0,
  Horse: 1,
  Camel: 2,
  EBA: 3,
} as const

export type ItemFamilyType = ValueOf<typeof ITEM_FAMILY_TYPE>

export interface Item {
  id: string
  name: string
  tier: number
  price: number
  baseId: string
  rank: number
  type: ItemType
  weight: number
  createdAt: Date
  culture: Culture
  flags: ItemFlag[]
  requirement: number
  weapons: ItemWeaponComponent[]
  armor: ItemArmorComponent | null
  mount: ItemMountComponent | null
}

export const WEAPON_USAGE = {
  Primary: 'Primary',
  Secondary: 'Secondary',
} as const

export type WeaponUsage = ValueOf<typeof WEAPON_USAGE>

export interface ItemFlat {
  id: string
  isNew: boolean
  name: string
  tier: number
  modId: string
  price: number
  baseId: string
  rank: number
  upkeep: number
  type: ItemType
  culture: Culture
  requirement: number
  weight: number | null
  flags: Array<ItemFlag | WeaponFlag | ItemUsage>
  // Armor
  armArmor: number | null
  legArmor: number | null
  headArmor: number | null
  bodyArmor: number | null
  armorMaterialType: ArmorMaterialType | null
  armorFamilyType?: ItemFamilyType | null
  // weapons
  length: number | null
  itemUsage: ItemUsage[] // TODO: delete?
  accuracy: number | null
  handling: number | null
  weaponFlags: WeaponFlag[] // TODO: delete?

  weaponUsage: WeaponUsage[]
  stackAmount: number | null
  missileSpeed: number | null
  weaponClass: WeaponClass | null
  swingSpeed?: number | null
  weaponPrimaryClass: WeaponClass | null
  thrustSpeed?: number | null
  swingDamage?: number | null
  thrustDamage?: number | null
  swingDamageType?: DamageType | null
  thrustDamageType?: DamageType | null

  // Shield
  shieldSpeed: number | null
  shieldArmor: number | null
  shieldDurability: number | null
  // Mount
  speed: number | null
  maneuver: number | null
  hitPoints: number | null
  bodyLength: number | null
  chargeDamage: number | null
  mountFamilyType: ItemFamilyType | null
  // MountHarness
  mountArmor: number | null
  mountArmorFamilyType: ItemFamilyType | null
  // Bow/XBow
  aimSpeed: number | null
  reloadSpeed: number | null
  // Arrows/Bolts
  damage: number | null
  stackWeight: number | null
  damageType: DamageType | null | undefined
}

export type ItemDescriptorField = [string, string | number]

export interface ItemDescriptor {
  flags: string[]
  modes: ItemMode[]
  fields: ItemDescriptorField[]
}

export interface ItemMode {
  name: string
  flags: string[]
  fields: ItemDescriptorField[]
}

export const ITEM_SLOT = {
  Head: 'Head',
  Shoulder: 'Shoulder',
  Body: 'Body',
  Hand: 'Hand',
  Leg: 'Leg',
  MountHarness: 'MountHarness',
  Mount: 'Mount',
  Weapon0: 'Weapon0',
  Weapon1: 'Weapon1',
  Weapon2: 'Weapon2',
  Weapon3: 'Weapon3',
  WeaponExtra: 'WeaponExtra',
} as const satisfies Record<_ItemSlot, _ItemSlot>

export type ItemSlot = ValueOf<typeof ITEM_SLOT>

export const ITEM_FIELD_FORMAT = {
  List: 'List',
  Damage: 'Damage',
  Requirement: 'Requirement',
  Number: 'Number',
  String: 'String',
} as const

export type ItemFieldFormat = ValueOf<typeof ITEM_FIELD_FORMAT>

export const ITEM_FIELD_COMPARE_RULE = {
  Bigger: 'Bigger',
  Less: 'Less',
} as const

export type ItemFieldCompareRule = ValueOf<typeof ITEM_FIELD_COMPARE_RULE>

export type CompareItemsResult = Partial<Record<keyof ItemFlat, number>>

export const ITEM_COMPARE_MODE = {
  Absolute: 'Absolute', // The items compared to each other, and the best one is chosen.
  Relative: 'Relative', // The items compared relative to the selected
} as const

export type ItemCompareMode = ValueOf<typeof ITEM_COMPARE_MODE>

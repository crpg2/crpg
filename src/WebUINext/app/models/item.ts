import type { Culture } from '~/models/culture'

export enum ArmorMaterialType {
  Undefined = 'Undefined',
  Cloth = 'Cloth',
  Leather = 'Leather',
  Chainmail = 'Chainmail',
  Plate = 'Plate',
}

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

export enum WeaponClass {
  Undefined = 'Undefined',
  OneHandedSword = 'OneHandedSword',
  TwoHandedSword = 'TwoHandedSword',
  OneHandedAxe = 'OneHandedAxe',
  TwoHandedAxe = 'TwoHandedAxe',
  Mace = 'Mace',
  Dagger = 'Dagger',
  Pick = 'Pick',
  TwoHandedMace = 'TwoHandedMace',
  TwoHandedPolearm = 'TwoHandedPolearm',
  OneHandedPolearm = 'OneHandedPolearm',
  LowGripPolearm = 'LowGripPolearm',
  Arrow = 'Arrow',
  Bolt = 'Bolt',
  Cartridge = 'Cartridge',
  Bow = 'Bow',
  Crossbow = 'Crossbow',
  Boulder = 'Boulder',
  Javelin = 'Javelin',
  ThrowingAxe = 'ThrowingAxe',
  ThrowingKnife = 'ThrowingKnife',
  Stone = 'Stone',
  Pistol = 'Pistol',
  Musket = 'Musket',
  SmallShield = 'SmallShield',
  LargeShield = 'LargeShield',
  Banner = 'Banner',
}

export enum WeaponFlags {
  MeleeWeapon = 'MeleeWeapon',
  RangedWeapon = 'RangedWeapon',
  FirearmAmmo = 'FirearmAmmo',
  NotUsableWithOneHand = 'NotUsableWithOneHand',
  NotUsableWithTwoHand = 'NotUsableWithTwoHand',
  WideGrip = 'WideGrip',
  AttachAmmoToVisual = 'AttachAmmoToVisual',
  Consumable = 'Consumable',
  HasHitPoints = 'HasHitPoints',
  HasString = 'HasString',
  StringHeldByHand = 'StringHeldByHand',
  UnloadWhenSheathed = 'UnloadWhenSheathed',
  AffectsArea = 'AffectsArea',
  AffectsAreaBig = 'AffectsAreaBig',
  Burning = 'Burning',
  BonusAgainstShield = 'BonusAgainstShield',
  CanPenetrateShield = 'CanPenetrateShield',
  CantReloadOnHorseback = 'CantReloadOnHorseback',
  CanReloadOnHorseback = 'CanReloadOnHorseback', // TODO: custom flag
  CantUseOnHorseback = 'CantUseOnHorseback', // TODO: custom flag
  AutoReload = 'AutoReload',
  TwoHandIdleOnMount = 'TwoHandIdleOnMount',
  NoBlood = 'NoBlood',
  PenaltyWithShield = 'PenaltyWithShield',
  CanDismount = 'CanDismount',
  CanHook = 'CanHook',
  MissileWithPhysics = 'MissileWithPhysics',
  MultiplePenetration = 'MultiplePenetration',
  CanKnockDown = 'CanKnockDown',
  CanBlockRanged = 'CanBlockRanged',
  LeavesTrail = 'LeavesTrail',
  CanCrushThrough = 'CanCrushThrough',
  UseHandAsThrowBase = 'UseHandAsThrowBase',
  AmmoBreaksOnBounceBack = 'AmmoBreaksOnBounceBack',
  AmmoCanBreakOnBounceBack = 'AmmoCanBreakOnBounceBack',
  AmmoSticksWhenShot = 'AmmoSticksWhenShot',
}

export enum DamageType {
  Undefined = 'Undefined',
  Cut = 'Cut',
  Pierce = 'Pierce',
  Blunt = 'Blunt',
}

export interface ItemWeaponComponent {
  length: number
  accuracy: number
  handling: number
  bodyArmor: number
  class: WeaponClass
  swingSpeed: number
  stackAmount: number
  thrustSpeed: number
  swingDamage: number

  itemUsage: ItemUsage
  missileSpeed: number
  flags: WeaponFlags[]

  thrustDamage: number
  swingDamageType: DamageType
  thrustDamageType: DamageType
}

export enum ItemFlags {
  ForceAttachOffHandPrimaryItemBone = 'ForceAttachOffHandPrimaryItemBone',
  ForceAttachOffHandSecondaryItemBone = 'ForceAttachOffHandSecondaryItemBone',
  NotUsableByFemale = 'NotUsableByFemale',
  NotUsableByMale = 'NotUsableByMale',
  DropOnWeaponChange = 'DropOnWeaponChange',
  DropOnAnyAction = 'DropOnAnyAction',
  CannotBePickedUp = 'CannotBePickedUp',
  CanBePickedUpFromCorpse = 'CanBePickedUpFromCorpse',
  QuickFadeOut = 'QuickFadeOut',
  WoodenAttack = 'WoodenAttack',
  WoodenParry = 'WoodenParry',
  HeldInOffHand = 'HeldInOffHand',
  HasToBeHeldUp = 'HasToBeHeldUp',
  UseTeamColor = 'UseTeamColor',
  Civilian = 'Civilian',
  DoNotScaleBodyAccordingToWeaponLength = 'DoNotScaleBodyAccordingToWeaponLength',
  DoesNotHideChest = 'DoesNotHideChest',
  NotStackable = 'NotStackable',
}

export enum ItemType {
  Undefined = 'Undefined',
  OneHandedWeapon = 'OneHandedWeapon',
  TwoHandedWeapon = 'TwoHandedWeapon',
  Polearm = 'Polearm',
  Thrown = 'Thrown',

  Ranged = 'Ranged', // virtual
  Ammo = 'Ammo', // virtual

  Bow = 'Bow',
  Crossbow = 'Crossbow',
  Pistol = 'Pistol',
  Musket = 'Musket',
  Arrows = 'Arrows',
  Bolts = 'Bolts',
  Bullets = 'Bullets',

  Shield = 'Shield',

  HeadArmor = 'HeadArmor',
  ShoulderArmor = 'ShoulderArmor',
  BodyArmor = 'BodyArmor',
  HandArmor = 'HandArmor',
  LegArmor = 'LegArmor',

  Mount = 'Mount',
  MountHarness = 'MountHarness',

  Banner = 'Banner',
}

export enum ItemUsage {
  LongBow = 'long_bow',
  Bow = 'bow',
  Crossbow = 'crossbow',
  CrossbowLight = 'crossbow_light',
  PolearmCouch = 'polearm_couch',
  PolearmBracing = 'polearm_bracing',
  PolearmPike = 'polearm_pike',
  Polearm = 'polearm', // jousting lance
}

export enum ItemFamilyType {
  Undefined = 0,
  Horse = 1,
  Camel = 2,
  EBA = 3,
}

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
  flags: ItemFlags[]
  requirement: number
  weapons: ItemWeaponComponent[]
  armor: ItemArmorComponent | null
  mount: ItemMountComponent | null
}

export enum WeaponUsage {
  Primary = 'Primary',
  Secondary = 'Secondary',
}

export interface ItemFlat {
  id: string
  new: number
  name: string
  tier: number
  modId: string
  price: number
  baseId: string
  rank: ItemRank
  upkeep: number
  type: ItemType
  culture: Culture
  requirement: number
  weight: number | null
  flags: Array<ItemFlags | WeaponFlags | ItemUsage>
  // Armor
  armArmor: number | null
  legArmor: number | null
  headArmor: number | null
  bodyArmor: number | null
  armorMaterialType: ArmorMaterialType | null
  armorFamilyType: ItemFamilyType | null | undefined
  // weapons
  length: number | null
  itemUsage: ItemUsage[]
  accuracy: number | null
  handling: number | null
  weaponFlags: WeaponFlags[]

  weaponUsage: WeaponUsage[]
  stackAmount: number | null
  missileSpeed: number | null
  weaponClass: WeaponClass | null
  swingSpeed: number | null | undefined
  weaponPrimaryClass: WeaponClass | null
  thrustSpeed: number | null | undefined
  swingDamage: number | null | undefined
  thrustDamage: number | null | undefined
  swingDamageType: DamageType | null | undefined
  thrustDamageType: DamageType | null | undefined

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

export enum ItemSlot {
  Head = 'Head',
  Shoulder = 'Shoulder',
  Body = 'Body',
  Hand = 'Hand',
  Leg = 'Leg',
  MountHarness = 'MountHarness',
  Mount = 'Mount',
  Weapon0 = 'Weapon0',
  Weapon1 = 'Weapon1',
  Weapon2 = 'Weapon2',
  Weapon3 = 'Weapon3',
  WeaponExtra = 'WeaponExtra',
}

export enum ItemFieldFormat {
  List = 'List',
  Damage = 'Damage',
  Requirement = 'Requirement',
  Number = 'Number',
}

export enum ItemFieldCompareRule {
  Bigger = 'Bigger',
  Less = 'Less',
}

export type CompareItemsResult = Partial<Record<keyof ItemFlat, number>>

export enum ItemCompareMode {
  Absolute = 'Absolute', // The items compared to each other, and the best one is chosen.
  Relative = 'Relative', // The items compared relative to the selected
}

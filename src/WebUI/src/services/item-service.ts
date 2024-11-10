import {
  brokenItemRepairPenaltySeconds,
  itemBreakChance,
  itemReforgeCostPerRank,
  itemRepairCostPerSecond,
  itemSellCostPenalty,
} from '~root/data/constants.json'
import { omitBy } from 'es-toolkit'

import type { EquippedItemsBySlot } from '~/models/character'
import type {
  ArmorMaterialType,
  CompareItemsResult,
  Item,
  ItemFlat,
  ItemRank,
} from '~/models/item'
import type { AggregationConfig } from '~/models/item-search'
import type { UserItem } from '~/models/user'

import { Culture } from '~/models/culture'
import {
  DamageType,
  ItemFamilyType,
  ItemFieldCompareRule,
  ItemFieldFormat,
  ItemFlags,
  ItemSlot,
  ItemType,
  ItemUsage,
  WeaponClass,
  WeaponFlags,
} from '~/models/item'
import { get } from '~/services/crpg-client'
import { aggregationsConfig } from '~/services/item-search-service/aggregations'
import { createItemIndex } from '~/services/item-search-service/indexator'
import { NotificationType, notify } from '~/services/notification-service'
import { n, t } from '~/services/translate-service'
import { roundFLoat } from '~/utils/math'

import { getAggregationsConfig, getVisibleAggregationsConfig } from './item-search-service'

export const getItems = () => get<Item[]>('/items')

export const getItemImage = (baseId: string) => `/items/${baseId}.webp`

export const getItemUpgrades = async (item: ItemFlat) =>
  createItemIndex(await get<Item[]>(`/items/upgrades/${item.baseId}`))
    // TODO: hotfix, avoid duplicate items with multiply weaponClass
    .filter(el => el?.weaponClass === item?.weaponClass)

export const armorTypes: ItemType[] = [
  ItemType.HeadArmor,
  ItemType.ShoulderArmor,
  ItemType.BodyArmor,
  ItemType.HandArmor,
  ItemType.LegArmor,
]

export const itemTypeByWeaponClass: Record<WeaponClass, ItemType> = {
  [WeaponClass.Arrow]: ItemType.Arrows,
  [WeaponClass.Banner]: ItemType.Banner,
  [WeaponClass.Bolt]: ItemType.Bolts,
  [WeaponClass.Boulder]: ItemType.Thrown,
  [WeaponClass.Bow]: ItemType.Bow,
  [WeaponClass.Cartridge]: ItemType.Bullets,
  [WeaponClass.Crossbow]: ItemType.Crossbow,
  [WeaponClass.Dagger]: ItemType.OneHandedWeapon,
  [WeaponClass.Javelin]: ItemType.Thrown,
  [WeaponClass.LargeShield]: ItemType.Shield,
  [WeaponClass.LowGripPolearm]: ItemType.Polearm,
  [WeaponClass.Mace]: ItemType.OneHandedWeapon,
  [WeaponClass.Musket]: ItemType.Musket,
  [WeaponClass.OneHandedAxe]: ItemType.OneHandedWeapon,
  [WeaponClass.OneHandedPolearm]: ItemType.Polearm,
  [WeaponClass.OneHandedSword]: ItemType.OneHandedWeapon,
  [WeaponClass.Pick]: ItemType.TwoHandedWeapon,
  [WeaponClass.Pistol]: ItemType.Pistol,
  [WeaponClass.SmallShield]: ItemType.Shield,
  [WeaponClass.Stone]: ItemType.Thrown,
  [WeaponClass.ThrowingAxe]: ItemType.Thrown,
  [WeaponClass.ThrowingKnife]: ItemType.Thrown,
  [WeaponClass.TwoHandedAxe]: ItemType.TwoHandedWeapon,
  [WeaponClass.TwoHandedMace]: ItemType.TwoHandedWeapon,
  [WeaponClass.TwoHandedPolearm]: ItemType.Polearm,
  [WeaponClass.TwoHandedSword]: ItemType.TwoHandedWeapon,
  [WeaponClass.Undefined]: ItemType.Undefined,
}

export const WeaponClassByItemUsage: Partial<Record<ItemUsage, WeaponClass>> = {
  [ItemUsage.Polearm]: WeaponClass.OneHandedPolearm, // jousting lances
  [ItemUsage.PolearmCouch]: WeaponClass.OneHandedPolearm,
}

const WeaponClassByItemType: Partial<Record<ItemType, WeaponClass[]>> = {
  [ItemType.OneHandedWeapon]: [
    WeaponClass.OneHandedSword,
    WeaponClass.OneHandedAxe,
    WeaponClass.Mace,
    WeaponClass.Dagger,
  ],
  [ItemType.Polearm]: [WeaponClass.TwoHandedPolearm, WeaponClass.OneHandedPolearm],
  [ItemType.Thrown]: [
    WeaponClass.Javelin,
    WeaponClass.ThrowingAxe,
    WeaponClass.ThrowingKnife,
    WeaponClass.Stone,
  ],
  [ItemType.TwoHandedWeapon]: [
    WeaponClass.TwoHandedSword,
    WeaponClass.TwoHandedAxe,
    WeaponClass.TwoHandedMace,
  ],
}

export const hasWeaponClassesByItemType = (type: ItemType) =>
  Object.keys(WeaponClassByItemType).includes(type)

export const getWeaponClassesByItemType = (type: ItemType): WeaponClass[] => {
  const weaponClasses = WeaponClassByItemType[type]
  return weaponClasses === undefined ? [] : weaponClasses
}

const weaponTypes: ItemType[] = [
  ItemType.Shield,
  ItemType.Bow,
  ItemType.Crossbow,
  ItemType.OneHandedWeapon,
  ItemType.TwoHandedWeapon,
  ItemType.Polearm,
  ItemType.Thrown,
  ItemType.Arrows,
  ItemType.Bolts,
]

export const itemTypesBySlot: Record<ItemSlot, ItemType[]> = {
  [ItemSlot.Body]: [ItemType.BodyArmor],
  [ItemSlot.Hand]: [ItemType.HandArmor],
  [ItemSlot.Head]: [ItemType.HeadArmor],
  [ItemSlot.Leg]: [ItemType.LegArmor],
  [ItemSlot.Mount]: [ItemType.Mount],
  [ItemSlot.MountHarness]: [ItemType.MountHarness],
  [ItemSlot.Shoulder]: [ItemType.ShoulderArmor],
  [ItemSlot.Weapon0]: weaponTypes,
  [ItemSlot.Weapon1]: weaponTypes,
  [ItemSlot.Weapon2]: weaponTypes,
  [ItemSlot.Weapon3]: weaponTypes,
  [ItemSlot.WeaponExtra]: [ItemType.Banner],
}

const weaponSlots: ItemSlot[] = [
  ItemSlot.Weapon0,
  ItemSlot.Weapon1,
  ItemSlot.Weapon2,
  ItemSlot.Weapon3,
]

export const itemSlotsByType: Partial<Record<ItemType, ItemSlot[]>> = {
  [ItemType.BodyArmor]: [ItemSlot.Body],
  [ItemType.HandArmor]: [ItemSlot.Hand],
  [ItemType.HeadArmor]: [ItemSlot.Head],
  [ItemType.LegArmor]: [ItemSlot.Leg],
  [ItemType.Mount]: [ItemSlot.Mount],
  [ItemType.MountHarness]: [ItemSlot.MountHarness],
  [ItemType.ShoulderArmor]: [ItemSlot.Shoulder],
  //
  [ItemType.Arrows]: weaponSlots,
  [ItemType.Bolts]: weaponSlots,
  [ItemType.Bow]: weaponSlots,
  [ItemType.Crossbow]: weaponSlots,
  [ItemType.OneHandedWeapon]: weaponSlots,
  [ItemType.Polearm]: weaponSlots,
  [ItemType.Shield]: weaponSlots,
  [ItemType.Thrown]: weaponSlots,
  [ItemType.TwoHandedWeapon]: weaponSlots,
  //
  [ItemType.Banner]: [ItemSlot.WeaponExtra],
}

export const isLargeShield = (item: Item) =>
  item.type === ItemType.Shield && item.weapons[0].class === WeaponClass.LargeShield

export const getAvailableSlotsByItem = (
  item: Item,
  equippedItems: EquippedItemsBySlot,
): ItemSlot[] => {
  // family type: compatibility with mount and mountHarness
  if (
    item.type === ItemType.MountHarness
    && ItemSlot.Mount in equippedItems
    && item.armor!.familyType !== equippedItems[ItemSlot.Mount].item.mount!.familyType
  ) {
    return []
  }

  if (
    item.type === ItemType.Mount
    && ItemSlot.MountHarness in equippedItems
    && item.mount!.familyType !== equippedItems[ItemSlot.MountHarness].item.armor!.familyType
  ) {
    return []
  }

  // Pikes
  if (item.flags.includes(ItemFlags.DropOnWeaponChange)) {
    return [ItemSlot.WeaponExtra]
  }

  // Banning the use of large shields on horseback
  if (
    (ItemSlot.Mount in equippedItems && isLargeShield(item))
    || (item.type === ItemType.Mount
      && Object.values(equippedItems).some(item => isLargeShield(item.item)))
  ) {
    notify(
      t('character.inventory.item.cantUseOnHorseback.notify.warning'),
      NotificationType.Warning,
    )
    return []
  }

  // family type: compatibility with EBA BodyArmor and EBA LegArmor
  if (
    (item.type === ItemType.BodyArmor
      && item.armor!.familyType === ItemFamilyType.EBA
      && (!(ItemSlot.Leg in equippedItems)
        || (ItemSlot.Leg in equippedItems
          && item.armor!.familyType !== equippedItems[ItemSlot.Leg].item.armor!.familyType)))
    //
        || (item.type === ItemType.LegArmor
          && ItemSlot.Body in equippedItems
          && equippedItems[ItemSlot.Body].item.armor!.familyType === ItemFamilyType.EBA
          && item.armor!.familyType !== equippedItems[ItemSlot.Body].item.armor!.familyType)
  ) {
    notify(
      t('character.inventory.item.EBAArmorCompatible.notify.warning'),
      NotificationType.Warning,
    )
    return []
  }

  return itemSlotsByType[item.type]!
}

export const getLinkedSlots = (slot: ItemSlot, equippedItems: EquippedItemsBySlot): ItemSlot[] => {
  // family type: compatibility with EBA BodyArmor and EBA LegArmor
  if (
    slot === ItemSlot.Leg
    && ItemSlot.Body in equippedItems
    && equippedItems[ItemSlot.Body].item.armor!.familyType === ItemFamilyType.EBA
  ) {
    return [ItemSlot.Body]
  }

  return []
}

export const visibleItemFlags: ItemFlags[] = [
  ItemFlags.DropOnWeaponChange,
  ItemFlags.DropOnAnyAction,
  ItemFlags.UseTeamColor,
]

export const visibleWeaponFlags: WeaponFlags[] = [
  // WeaponFlags.AffectsAreaBig,
  // WeaponFlags.AffectsArea,
  // WeaponFlags.AutoReload,
  WeaponFlags.BonusAgainstShield,
  // WeaponFlags.Burning,
  // WeaponFlags.CanBlockRanged,
  WeaponFlags.CanCrushThrough,
  WeaponFlags.CanDismount,
  WeaponFlags.CanHook,
  WeaponFlags.CanKnockDown,
  WeaponFlags.CanPenetrateShield,
  WeaponFlags.CantReloadOnHorseback,
  WeaponFlags.CanReloadOnHorseback, // TODO:
  WeaponFlags.MultiplePenetration,
  WeaponFlags.CantUseOnHorseback,
  // WeaponFlags.TwoHandIdleOnMount, TODO:
]

export const visibleItemUsage: ItemUsage[] = [
  ItemUsage.LongBow,
  ItemUsage.Bow,
  ItemUsage.Crossbow,
  ItemUsage.CrossbowLight,
  ItemUsage.PolearmCouch,
  ItemUsage.PolearmBracing,
  ItemUsage.PolearmPike,
  ItemUsage.Polearm,
]

export const itemTypeToIcon: Record<ItemType, string> = {
  [ItemType.Arrows]: 'item-type-arrow',
  [ItemType.Banner]: 'item-type-banner',
  [ItemType.BodyArmor]: 'item-type-body-armor',
  [ItemType.Bolts]: 'item-type-bolt',
  [ItemType.Bow]: 'item-type-bow',
  [ItemType.Bullets]: '',
  [ItemType.Crossbow]: 'item-type-crossbow',
  [ItemType.HandArmor]: 'item-type-hand-armor',
  [ItemType.HeadArmor]: 'item-type-head-armor',
  [ItemType.LegArmor]: 'item-type-leg-armor',
  [ItemType.Mount]: 'item-type-mount',
  [ItemType.MountHarness]: 'item-type-mount-harness',
  [ItemType.Musket]: '',
  [ItemType.OneHandedWeapon]: 'item-type-one-handed-weapon',
  [ItemType.Pistol]: '',
  [ItemType.Polearm]: 'item-type-polearm',
  [ItemType.Shield]: 'item-type-shield',
  [ItemType.ShoulderArmor]: 'item-type-shoulder-armor',

  [ItemType.Thrown]: 'item-type-throwing-weapon',
  [ItemType.TwoHandedWeapon]: 'item-type-two-handed-weapon',
  [ItemType.Undefined]: '',
}

export const weaponClassToIcon: Record<WeaponClass, string> = {
  [WeaponClass.Arrow]: '',
  [WeaponClass.Banner]: '',
  [WeaponClass.Bolt]: '',
  [WeaponClass.Boulder]: '',
  [WeaponClass.Bow]: '',
  [WeaponClass.Cartridge]: '',
  [WeaponClass.Crossbow]: '',
  [WeaponClass.Dagger]: 'weapon-class-one-handed-dagger',
  [WeaponClass.Javelin]: 'weapon-class-throwing-spear',
  [WeaponClass.LargeShield]: 'weapon-class-shield-large',
  [WeaponClass.LowGripPolearm]: '',
  [WeaponClass.Mace]: 'weapon-class-one-handed-mace',
  [WeaponClass.Musket]: '',
  [WeaponClass.OneHandedAxe]: 'weapon-class-one-handed-axe',
  [WeaponClass.OneHandedPolearm]: 'weapon-class-one-handed-polearm',
  [WeaponClass.OneHandedSword]: 'weapon-class-one-handed-sword',
  [WeaponClass.Pick]: '',
  [WeaponClass.Pistol]: '',
  [WeaponClass.SmallShield]: 'weapon-class-shield-small',
  [WeaponClass.Stone]: 'weapon-class-throwing-stone',
  [WeaponClass.ThrowingAxe]: 'weapon-class-throwing-axe',
  [WeaponClass.ThrowingKnife]: 'weapon-class-throwing-knife',
  [WeaponClass.TwoHandedAxe]: 'weapon-class-two-handed-axe',
  [WeaponClass.TwoHandedMace]: 'weapon-class-two-handed-mace',
  [WeaponClass.TwoHandedPolearm]: 'weapon-class-two-handed-polearm',
  [WeaponClass.TwoHandedSword]: 'weapon-class-two-handed-sword',
  [WeaponClass.Undefined]: '',
}

export const itemFlagsToIcon: Record<ItemFlags, string | null> = {
  [ItemFlags.CanBePickedUpFromCorpse]: null,
  [ItemFlags.CannotBePickedUp]: null,
  [ItemFlags.Civilian]: null,
  [ItemFlags.DoesNotHideChest]: null,
  [ItemFlags.DoNotScaleBodyAccordingToWeaponLength]: null,
  [ItemFlags.DropOnAnyAction]: null,
  [ItemFlags.DropOnWeaponChange]: 'item-flag-drop-on-change',
  [ItemFlags.ForceAttachOffHandPrimaryItemBone]: null,
  [ItemFlags.ForceAttachOffHandSecondaryItemBone]: null,
  [ItemFlags.HasToBeHeldUp]: null,
  [ItemFlags.HeldInOffHand]: null,
  [ItemFlags.NotStackable]: null,
  [ItemFlags.NotUsableByFemale]: null,
  [ItemFlags.NotUsableByMale]: null,
  [ItemFlags.QuickFadeOut]: null,
  [ItemFlags.UseTeamColor]: 'item-flag-use-team-color',
  [ItemFlags.WoodenAttack]: null,
  [ItemFlags.WoodenParry]: null,
}

export const weaponFlagsToIcon: Partial<Record<WeaponFlags, string | null>> = {
  [WeaponFlags.AutoReload]: 'item-flag-auto-reload',
  [WeaponFlags.BonusAgainstShield]: 'item-flag-bonus-against-shield',
  [WeaponFlags.CanDismount]: 'item-flag-can-dismount',
  [WeaponFlags.CanKnockDown]: 'item-flag-can-knock-down',
  [WeaponFlags.CanPenetrateShield]: 'item-flag-can-penetrate-shield',
  [WeaponFlags.CanReloadOnHorseback]: 'item-flag-can-reload-on-horseback',
  [WeaponFlags.CantReloadOnHorseback]: 'item-flag-cant-reload-on-horseback',
  [WeaponFlags.CantUseOnHorseback]: 'item-flag-cant-reload-on-horseback',
  [WeaponFlags.MultiplePenetration]: 'item-flag-multiply-penetration',
  [WeaponFlags.TwoHandIdleOnMount]: 'item-flag-two-hand-idle',
}

export const itemUsageToIcon: Partial<Record<ItemUsage, string | null>> = {
  [ItemUsage.Bow]: 'item-flag-short-bow',
  [ItemUsage.Crossbow]: 'item-flag-heavy-crossbow',
  [ItemUsage.CrossbowLight]: 'item-flag-light-crossbow',
  [ItemUsage.LongBow]: 'item-flag-longbow',
  [ItemUsage.Polearm]: 'item-flag-jousting',
  [ItemUsage.PolearmBracing]: 'item-flag-brace',
  [ItemUsage.PolearmCouch]: 'item-flag-couch',
  [ItemUsage.PolearmPike]: 'item-flag-pike',
}

export const itemFamilyTypeToIcon: Record<ItemFamilyType, string | null> = {
  [ItemFamilyType.Camel]: 'mount-type-camel',
  [ItemFamilyType.EBA]: null,
  [ItemFamilyType.Horse]: 'mount-type-horse',
  [ItemFamilyType.Undefined]: null,
}

export const damageTypeToIcon: Record<DamageType, string | null> = {
  [DamageType.Blunt]: 'damage-type-blunt',
  [DamageType.Cut]: 'damage-type-cut',
  [DamageType.Pierce]: 'damage-type-pierce',
  [DamageType.Undefined]: null,
}

export const itemCultureToIcon: Record<Culture, string | null> = {
  [Culture.Aserai]: 'culture-aserai',
  [Culture.Battania]: 'culture-battania',
  [Culture.Empire]: 'culture-empire',
  [Culture.Khuzait]: 'culture-khuzait',
  [Culture.Looters]: 'culture-looters',
  [Culture.Neutral]: 'culture-neutrals',
  [Culture.Sturgia]: 'culture-sturgia',
  [Culture.Vlandia]: 'culture-vlandia',
}

// TO MODEL:
export enum IconBucketType {
  Asset = 'Asset',
  Svg = 'Svg',
}

export interface IconedBucket {
  name: string
  type: IconBucketType
}

export interface HumanBucket {
  label: string
  icon: IconedBucket | null
  tooltip: {
    title: string
    description: string | null
  } | null
}

type ItemFlatDamageField = keyof Pick<ItemFlat, 'damage' | 'thrustDamage' | 'swingDamage'>
type ItemFlatDamageType = keyof Pick<ItemFlat, 'thrustDamageType' | 'swingDamageType'>

const damageTypeFieldByDamageField: Record<ItemFlatDamageField, ItemFlatDamageType> = {
  damage: 'thrustDamageType', // arrow/bolt
  swingDamage: 'swingDamageType',
  thrustDamage: 'thrustDamageType',
}

export const getDamageType = (aggregationKey: keyof ItemFlat, item: ItemFlat) => {
  return item[damageTypeFieldByDamageField[aggregationKey as ItemFlatDamageField]]
}

const createHumanBucket = (
  label: string,
  icon: IconedBucket | null,
  tooltip: {
    title: string
    description: string | null
  } | null,
): HumanBucket => ({
  icon,
  label,
  tooltip,
})

const createIcon = (type: IconBucketType, name: string | null | undefined): IconedBucket | null =>
  name === null || name === undefined
    ? null
    : {
        name,
        type,
      }

export const humanizeBucket = (
  aggregationKey: keyof ItemFlat,
  bucket: any,
  item?: ItemFlat,
): HumanBucket => {
  if (bucket === null || bucket === undefined) {
    return createHumanBucket('', null, null)
  }

  const format = aggregationsConfig[aggregationKey]?.format

  if (aggregationKey === 'type') {
    return createHumanBucket(
      t(`item.type.${bucket as ItemType}`),
      createIcon(IconBucketType.Svg, itemTypeToIcon[bucket as ItemType]),
      null,
    )
  }

  if (aggregationKey === 'weaponClass') {
    return createHumanBucket(
      t(`item.weaponClass.${bucket as WeaponClass}`),
      createIcon(IconBucketType.Svg, weaponClassToIcon[bucket as WeaponClass]),
      null,
    )
  }

  if (aggregationKey === 'damageType') {
    return createHumanBucket(
      t(`item.damageType.${bucket}.long`),
      createIcon(IconBucketType.Svg, damageTypeToIcon[bucket as DamageType]),
      {
        description: t(`item.damageType.${bucket}.description`),
        title: t(`item.damageType.${bucket}.title`),
      },
    )
  }

  if (aggregationKey === 'culture') {
    return createHumanBucket(
      t(`item.culture.${bucket}`),
      createIcon(IconBucketType.Asset, itemCultureToIcon[bucket as Culture]),
      {
        description: null,
        title: t(`item.culture.${bucket}`),
      },
    )
  }

  if (['mountArmorFamilyType', 'mountFamilyType', 'armorFamilyType'].includes(aggregationKey)) {
    return createHumanBucket(
      t(`item.familyType.${bucket}.title`),
      createIcon(IconBucketType.Svg, itemFamilyTypeToIcon[bucket as ItemFamilyType]),
      {
        description: t(`item.familyType.${bucket}.description`),
        title: t(`item.familyType.${bucket}.title`),
      },
    )
  }

  if (['armorMaterialType'].includes(aggregationKey)) {
    return createHumanBucket(
      t(`item.armorMaterialType.${bucket as ArmorMaterialType}.title`),
      null,
      null,
    )
  }

  if (aggregationKey === 'flags') {
    if (Object.values(ItemFlags).includes(bucket as ItemFlags)) {
      return createHumanBucket(
        t(`item.flags.${bucket}`),
        createIcon(IconBucketType.Svg, itemFlagsToIcon[bucket as ItemFlags]),
        {
          description: null,
          title: t(`item.flags.${bucket}`),
        },
      )
    }

    if (Object.values(WeaponFlags).includes(bucket as WeaponFlags)) {
      return createHumanBucket(
        t(`item.weaponFlags.${bucket}`),
        createIcon(IconBucketType.Svg, weaponFlagsToIcon[bucket as WeaponFlags]),
        {
          description: null,
          title: t(`item.weaponFlags.${bucket}`),
        },
      )
    }

    if (Object.values(ItemUsage).includes(bucket as ItemUsage)) {
      return createHumanBucket(
        t(`item.usage.${bucket}.title`),
        createIcon(IconBucketType.Svg, itemUsageToIcon[bucket as ItemUsage]),
        {
          description: t(`item.usage.${bucket}.description`),
          title: t(`item.usage.${bucket}.title`),
        },
      )
    }
  }

  if (format === ItemFieldFormat.Damage && item !== undefined) {
    const damageType = getDamageType(aggregationKey, item)

    if (damageType === null || damageType === undefined) {
      return createHumanBucket(String(bucket), null, null)
    }

    return createHumanBucket(
      t('item.damageTypeFormat', {
        type: t(`item.damageType.${damageType}.short`),
        value: bucket,
      }),
      null,
      {
        description: t(`item.damageType.${damageType}.description`),
        title: t(`item.damageType.${damageType}.title`),
      },
    )
  }

  if (format === ItemFieldFormat.Requirement) {
    return createHumanBucket(
      t('item.requirementFormat', {
        value: bucket,
      }),
      null,
      null,
    )
  }

  if (format === ItemFieldFormat.Number) {
    return createHumanBucket(n(bucket as number), null, null)
  }

  return createHumanBucket(String(bucket), null, null)
}

interface GroupedItems {
  type: ItemType
  items: ItemFlat[]
  weaponClass: WeaponClass | null
}

export const groupItemsByTypeAndWeaponClass = (items: ItemFlat[]) => {
  return items.reduce((out, item) => {
    const currentEl = out.find((el) => {
      // merge Shield classes
      if (item.type === ItemType.Shield) {
        return el.type === item.type
      }

      return el.type === item.type && el.weaponClass === item.weaponClass
    })

    if (currentEl !== undefined) {
      currentEl.items.push(item)
    }
    else {
      out.push({
        items: [item],
        type: item.type,
        weaponClass: item.weaponClass,
      })
    }

    return out
  }, [] as GroupedItems[])
}

export const getCompareItemsResult = (items: ItemFlat[], aggregationsConfig: AggregationConfig) => {
  return (Object.keys(aggregationsConfig) as Array<keyof ItemFlat>)
    .filter(k => aggregationsConfig[k]?.compareRule !== undefined)
    .reduce((out, k) => {
      const values = items.map(fi => fi[k]).filter(v => typeof v === 'number') as number[]
      out[k]
        = aggregationsConfig[k]!.compareRule === ItemFieldCompareRule.Less
          ? Math.min(...values)
          : Math.max(...values)
      return out
    }, {} as CompareItemsResult)
}

export const getRelativeEntries = (item: ItemFlat, aggregationsConfig: AggregationConfig) => {
  return (Object.keys(aggregationsConfig) as Array<keyof ItemFlat>)
    .filter(k => aggregationsConfig[k]?.compareRule !== undefined)
    .reduce((out, k) => {
      if (typeof item[k] === 'number') {
        out[k] = item[k] as number
      }
      return out
    }, {} as CompareItemsResult)
}

export const getItemFieldAbsoluteDiffStr = (
  compareRule: ItemFieldCompareRule,
  value: number,
  bestValue: number,
) => {
  const DEFAULT_STR = ''

  if (value === bestValue) {
    return DEFAULT_STR
  }

  if (compareRule === ItemFieldCompareRule.Less) {
    if (bestValue > value) {
      return DEFAULT_STR
    }

    return `+${n(roundFLoat(Math.abs(value - bestValue)))}`
  }

  if (bestValue < value) {
    return DEFAULT_STR
  }

  return `-${n(roundFLoat(Math.abs(bestValue - value)))}`
}

// TODO: spec
export const getItemFieldRelativeDiffStr = (value: number, relativeValue: number) => {
  const DEFAULT_STR = ''

  if (value === relativeValue) {
    return DEFAULT_STR
  }

  if (relativeValue > value) {
    return `-${n(roundFLoat(Math.abs(value - relativeValue)))}`
  }

  return `+${n(roundFLoat(Math.abs(value - relativeValue)))}`
}

export const getItemGraceTimeEnd = (userItem: UserItem) => {
  const graceTimeEnd = userItem.createdAt
  graceTimeEnd.setHours(graceTimeEnd.getHours() + 1) // TODO: to constants
  return graceTimeEnd
}

export const isGraceTimeExpired = (itemGraceTimeEnd: Date) => itemGraceTimeEnd < new Date()

export const computeSalePrice = (userItem: UserItem) => {
  const graceTimeEnd = getItemGraceTimeEnd(userItem)

  if (isGraceTimeExpired(graceTimeEnd)) {
    return {
      graceTimeEnd: null,
      price: Math.floor(userItem.item.price * itemSellCostPenalty),
    }
  }

  // If the item was recently bought it is sold at 100% of its original price.
  return { graceTimeEnd, price: userItem.item.price }
}

export const computeAverageRepairCostPerHour = (price: number) =>
  Math.floor(price * itemRepairCostPerSecond * 3600 * itemBreakChance)

export const computeBrokenItemRepairCost = (price: number) =>
  Math.floor(price * itemRepairCostPerSecond * brokenItemRepairPenaltySeconds)

export const getRankColor = (rank: ItemRank) => {
  switch (rank) {
    case 1:
      return '#4ade80'

    case 2:
      return '#60a5fa'

    case 3:
      return '#c084fc'

    default:
      return '#fff'
  }
}

export const canUpgrade = (type: ItemType) => type !== ItemType.Banner

export const canAddedToClanArmory = (type: ItemType) => type !== ItemType.Banner

export const reforgeCostByRank: Record<ItemRank, number> = {
  0: itemReforgeCostPerRank[0],
  1: itemReforgeCostPerRank[1],
  2: itemReforgeCostPerRank[2],
  3: itemReforgeCostPerRank[3],
}

export const itemIsNewDays = 1

export const checkIsWeaponBySlot = (slot: ItemSlot) => weaponSlots.includes(slot)

const itemParamIsEmpty = (field: keyof ItemFlat, itemFlat: ItemFlat) => {
  const value = itemFlat[field]

  if (Array.isArray(value) && value.length === 0) {
    return true
  }

  if (value === 0) {
    return true
  }

  return false
}

// TODO: spec
export const getItemAggregations = (itemFlat: ItemFlat) => {
  const aggsConfig = getVisibleAggregationsConfig(
    getAggregationsConfig(itemFlat.type, itemFlat.weaponClass),
  )
  return omitBy(aggsConfig, (_value, field) => itemParamIsEmpty(field, itemFlat))
}

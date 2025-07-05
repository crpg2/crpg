import {
  getItems as _getItems,
  getItemsUpgradesByBaseId,
} from '#hey-api/sdk.gen'
import {
  brokenItemRepairPenaltySeconds,
  itemBreakChance,
  itemReforgeCostPerRank,
  itemRepairCostPerSecond,
  itemSellCostPenalty,
  itemSellGracePeriodMinutes,
} from '~root/data/constants.json'
import { omitBy } from 'es-toolkit'

import type { EquippedItemsBySlot } from '~/models/character'
import type { Culture } from '~/models/culture'
import type { ArmorMaterialType, CompareItemsResult, DamageType, Item, ItemFamilyType, ItemFieldCompareRule, ItemFlag, ItemFlat, ItemSlot, ItemType, ItemUsage, WeaponClass, WeaponFlag } from '~/models/item'
import type { UserItem } from '~/models/user'

import {
  DAMAGE_TYPE,
  ITEM_FAMILY_TYPE,
  ITEM_FIELD_COMPARE_RULE,
  ITEM_FIELD_FORMAT,
  ITEM_FLAG,
  ITEM_SLOT,
  ITEM_TYPE,
  ITEM_USAGE,
  WEAPON_CLASS,
  WEAPON_FLAG,
} from '~/models/item'
import { createItemIndex } from '~/services/item-search-service/indexator'

import type { AggregationConfig } from './item-search-service/aggregations'

import { cultureToIcon } from './culture-service'
import { getAggregationsConfig, getVisibleAggregationsConfig } from './item-search-service'
import { aggregationsConfig } from './item-search-service/aggregations'

export const getItems = async (): Promise<Item[]> => {
  const { data } = await _getItems({ composable: '$fetch' })
  return data!
}

export const extractItem = <T extends { item: Item }>(wrapper: T): Item => wrapper.item

export const getItemImage = (baseId: string) => `/items/${baseId}.webp`

export const getItemUpgrades = async (baseId: string): Promise<ItemFlat[]> => {
  const { data } = await getItemsUpgradesByBaseId({ composable: '$fetch', path: { baseId } })
  // console.log('data', createItemIndex(data! as Item[]))

  return createItemIndex(data! as Item[])
  // TODO: hotfix, avoid duplicate items with multiply weaponClass
  // .filter(el => el?.weaponClass === item?.weaponClass)
}

export const armorTypes: ItemType[] = [
  ITEM_TYPE.HeadArmor,
  ITEM_TYPE.ShoulderArmor,
  ITEM_TYPE.BodyArmor,
  ITEM_TYPE.HandArmor,
  ITEM_TYPE.LegArmor,
]

export const itemTypeByWeaponClass: Record<WeaponClass, ItemType> = {
  [WEAPON_CLASS.Arrow]: ITEM_TYPE.Ammo,
  [WEAPON_CLASS.Banner]: ITEM_TYPE.Banner,
  [WEAPON_CLASS.Bolt]: ITEM_TYPE.Ammo,
  [WEAPON_CLASS.Boulder]: ITEM_TYPE.Thrown,
  [WEAPON_CLASS.Bow]: ITEM_TYPE.Ranged,
  [WEAPON_CLASS.Cartridge]: ITEM_TYPE.Ammo,
  [WEAPON_CLASS.Crossbow]: ITEM_TYPE.Ranged,
  [WEAPON_CLASS.Dagger]: ITEM_TYPE.OneHandedWeapon,
  [WEAPON_CLASS.Javelin]: ITEM_TYPE.Thrown,
  [WEAPON_CLASS.LargeShield]: ITEM_TYPE.Shield,
  [WEAPON_CLASS.LowGripPolearm]: ITEM_TYPE.Polearm,
  [WEAPON_CLASS.Mace]: ITEM_TYPE.OneHandedWeapon,
  [WEAPON_CLASS.Musket]: ITEM_TYPE.Ranged,
  [WEAPON_CLASS.OneHandedAxe]: ITEM_TYPE.OneHandedWeapon,
  [WEAPON_CLASS.OneHandedPolearm]: ITEM_TYPE.Polearm,
  [WEAPON_CLASS.OneHandedSword]: ITEM_TYPE.OneHandedWeapon,
  [WEAPON_CLASS.Pick]: ITEM_TYPE.TwoHandedWeapon,
  [WEAPON_CLASS.Pistol]: ITEM_TYPE.Ranged,
  [WEAPON_CLASS.SmallShield]: ITEM_TYPE.Shield,
  [WEAPON_CLASS.Stone]: ITEM_TYPE.Thrown,
  [WEAPON_CLASS.ThrowingAxe]: ITEM_TYPE.Thrown,
  [WEAPON_CLASS.ThrowingKnife]: ITEM_TYPE.Thrown,
  [WEAPON_CLASS.TwoHandedAxe]: ITEM_TYPE.TwoHandedWeapon,
  [WEAPON_CLASS.TwoHandedMace]: ITEM_TYPE.TwoHandedWeapon,
  [WEAPON_CLASS.TwoHandedPolearm]: ITEM_TYPE.Polearm,
  [WEAPON_CLASS.TwoHandedSword]: ITEM_TYPE.TwoHandedWeapon,
  [WEAPON_CLASS.Undefined]: ITEM_TYPE.Undefined,
}

export const WeaponClassByItemUsage: Partial<Record<ItemUsage, WeaponClass>> = {
  [ITEM_USAGE.Polearm]: WEAPON_CLASS.OneHandedPolearm, // jousting lances
  [ITEM_USAGE.PolearmCouch]: WEAPON_CLASS.OneHandedPolearm,
}

const WeaponClassByItemType: Partial<Record<ItemType, WeaponClass[]>> = {
  [ITEM_TYPE.OneHandedWeapon]: [
    WEAPON_CLASS.OneHandedSword,
    WEAPON_CLASS.OneHandedAxe,
    WEAPON_CLASS.Mace,
    WEAPON_CLASS.Dagger,
  ],
  [ITEM_TYPE.TwoHandedWeapon]: [
    WEAPON_CLASS.TwoHandedSword,
    WEAPON_CLASS.TwoHandedAxe,
    WEAPON_CLASS.TwoHandedMace,
  ],
  [ITEM_TYPE.Polearm]: [
    WEAPON_CLASS.TwoHandedPolearm,
    WEAPON_CLASS.OneHandedPolearm,
  ],
  [ITEM_TYPE.Thrown]: [
    WEAPON_CLASS.Javelin,
    WEAPON_CLASS.ThrowingAxe,
    WEAPON_CLASS.ThrowingKnife,
    WEAPON_CLASS.Stone,
  ],
  [ITEM_TYPE.Ranged]: [
    WEAPON_CLASS.Bow,
    WEAPON_CLASS.Crossbow,
    WEAPON_CLASS.Pistol,
    WEAPON_CLASS.Musket,
  ],
  [ITEM_TYPE.Ammo]: [
    WEAPON_CLASS.Arrow,
    WEAPON_CLASS.Bolt,
  ],
}

export const hasWeaponClassesByItemType = (type: ItemType) =>
  Object.keys(WeaponClassByItemType).includes(type)

export const getWeaponClassesByItemType = (type: ItemType): WeaponClass[] =>
  WeaponClassByItemType?.[type] || []

const WEAPON_TYPES: ItemType[] = [
  ITEM_TYPE.Shield,
  ITEM_TYPE.Bow,
  ITEM_TYPE.Crossbow,
  ITEM_TYPE.Musket,
  ITEM_TYPE.Pistol,
  ITEM_TYPE.OneHandedWeapon,
  ITEM_TYPE.TwoHandedWeapon,
  ITEM_TYPE.Polearm,
  ITEM_TYPE.Thrown,
  ITEM_TYPE.Arrows,
  ITEM_TYPE.Bolts,
  ITEM_TYPE.Bullets,
]

export const itemTypesBySlot: Record<ItemSlot, ItemType[]> = {
  [ITEM_SLOT.Body]: [ITEM_TYPE.BodyArmor],
  [ITEM_SLOT.Hand]: [ITEM_TYPE.HandArmor],
  [ITEM_SLOT.Head]: [ITEM_TYPE.HeadArmor],
  [ITEM_SLOT.Leg]: [ITEM_TYPE.LegArmor],
  [ITEM_SLOT.Mount]: [ITEM_TYPE.Mount],
  [ITEM_SLOT.MountHarness]: [ITEM_TYPE.MountHarness],
  [ITEM_SLOT.Shoulder]: [ITEM_TYPE.ShoulderArmor],
  [ITEM_SLOT.Weapon0]: WEAPON_TYPES,
  [ITEM_SLOT.Weapon1]: WEAPON_TYPES,
  [ITEM_SLOT.Weapon2]: WEAPON_TYPES,
  [ITEM_SLOT.Weapon3]: WEAPON_TYPES,
  [ITEM_SLOT.WeaponExtra]: [ITEM_TYPE.Banner],
}

const WEAPON_SLOTS: ItemSlot[] = [
  ITEM_SLOT.Weapon0,
  ITEM_SLOT.Weapon1,
  ITEM_SLOT.Weapon2,
  ITEM_SLOT.Weapon3,
]

export const isWeaponBySlot = (slot: ItemSlot) => WEAPON_SLOTS.includes(slot)

export const itemSlotsByType: Partial<Record<ItemType, ItemSlot[]>> = {
  [ITEM_TYPE.BodyArmor]: [ITEM_SLOT.Body],
  [ITEM_TYPE.HandArmor]: [ITEM_SLOT.Hand],
  [ITEM_TYPE.HeadArmor]: [ITEM_SLOT.Head],
  [ITEM_TYPE.LegArmor]: [ITEM_SLOT.Leg],
  [ITEM_TYPE.Mount]: [ITEM_SLOT.Mount],
  [ITEM_TYPE.MountHarness]: [ITEM_SLOT.MountHarness],
  [ITEM_TYPE.ShoulderArmor]: [ITEM_SLOT.Shoulder],
  //
  [ITEM_TYPE.Arrows]: WEAPON_SLOTS,
  [ITEM_TYPE.Bolts]: WEAPON_SLOTS,
  [ITEM_TYPE.Bullets]: WEAPON_SLOTS,
  [ITEM_TYPE.Bow]: WEAPON_SLOTS,
  [ITEM_TYPE.Crossbow]: WEAPON_SLOTS,
  [ITEM_TYPE.Musket]: WEAPON_SLOTS,
  [ITEM_TYPE.Pistol]: WEAPON_SLOTS,
  [ITEM_TYPE.OneHandedWeapon]: WEAPON_SLOTS,
  [ITEM_TYPE.Polearm]: WEAPON_SLOTS,
  [ITEM_TYPE.Shield]: WEAPON_SLOTS,
  [ITEM_TYPE.Thrown]: WEAPON_SLOTS,
  [ITEM_TYPE.TwoHandedWeapon]: WEAPON_SLOTS,
  //
  [ITEM_TYPE.Banner]: [ITEM_SLOT.WeaponExtra],
}

export const isLargeShield = (item: Item) =>
  item.type === ITEM_TYPE.Shield && item.weapons[0]?.class === WEAPON_CLASS.LargeShield

export const getAvailableSlotsByItem = (
  item: Item,
  equippedItems: EquippedItemsBySlot,
): { slots: ItemSlot[], warning: string | null } => {
  // family type: compatibility with mount and mountHarness
  if (
    item.type === ITEM_TYPE.MountHarness
    && ITEM_SLOT.Mount in equippedItems
    && item.armor!.familyType !== equippedItems[ITEM_SLOT.Mount].item.mount!.familyType
  ) {
    return { slots: [], warning: null }
  }

  if (
    item.type === ITEM_TYPE.Mount
    && ITEM_SLOT.MountHarness in equippedItems
    && item.mount!.familyType !== equippedItems[ITEM_SLOT.MountHarness].item.armor!.familyType
  ) {
    return { slots: [], warning: null }
  }

  // Pikes
  if (item.flags.includes(ITEM_FLAG.DropOnWeaponChange)) {
    return { slots: [ITEM_SLOT.WeaponExtra], warning: null }
  }

  // Wanning the use of large shields on horseback
  if (
    (ITEM_SLOT.Mount in equippedItems && isLargeShield(item))
    || (item.type === ITEM_TYPE.Mount
      && Object.values(equippedItems).some(item => isLargeShield(item.item)))
  ) {
    return { slots: [], warning: 'character.inventory.item.cantUseOnHorseback.notify.warning' }
  }

  // Family type: compatibility with EBA BodyArmor and EBA LegArmor
  // TODO: to fn

  if (
    (
      item.type === ITEM_TYPE.BodyArmor
      && item.armor!.familyType === ITEM_FAMILY_TYPE.EBA
      && (!(ITEM_SLOT.Leg in equippedItems) || (ITEM_SLOT.Leg in equippedItems && item.armor!.familyType !== equippedItems[ITEM_SLOT.Leg].item.armor!.familyType))
    )
    //
    || (
      item.type === ITEM_TYPE.LegArmor
      && ITEM_SLOT.Body in equippedItems
      && equippedItems[ITEM_SLOT.Body].item.armor!.familyType === ITEM_FAMILY_TYPE.EBA
      && item.armor!.familyType !== equippedItems[ITEM_SLOT.Body].item.armor!.familyType
    )
  ) {
    return { slots: [], warning: 'character.inventory.item.EBAArmorCompatible.notify.warning' }
  }

  return { slots: itemSlotsByType[item.type] || [], warning: null }
}

export const getLinkedSlots = (slot: ItemSlot, equippedItems: EquippedItemsBySlot): ItemSlot[] => {
  // Family type: compatibility with EBA BodyArmor and EBA LegArmor
  if (
    slot === ITEM_SLOT.Leg
    && ITEM_SLOT.Body in equippedItems
    && equippedItems[ITEM_SLOT.Body].item.armor!.familyType === ITEM_FAMILY_TYPE.EBA
  ) {
    return [ITEM_SLOT.Body]
  }

  return []
}

export const VISIBLE_ITEM_FLAGS: ItemFlag[] = [
  ITEM_FLAG.DropOnWeaponChange,
  ITEM_FLAG.DropOnAnyAction,
  ITEM_FLAG.UseTeamColor,
]

export const VISIBLE_WEAPON_FLAGS: WeaponFlag[] = [
  WEAPON_FLAG.BonusAgainstShield,
  WEAPON_FLAG.CanCrushThrough,
  WEAPON_FLAG.CanDismount,
  WEAPON_FLAG.CanHook,
  WEAPON_FLAG.CanKnockDown,
  WEAPON_FLAG.CanPenetrateShield,
  WEAPON_FLAG.CantReloadOnHorseback,
  WEAPON_FLAG.CanReloadOnHorseback,
  WEAPON_FLAG.MultiplePenetration,
  WEAPON_FLAG.CantUseOnHorseback,
]

export const VISIBLE_ITEM_USAGE: ItemUsage[] = [
  ITEM_USAGE.LongBow,
  ITEM_USAGE.Bow,
  ITEM_USAGE.Crossbow,
  ITEM_USAGE.CrossbowLight,
  ITEM_USAGE.PolearmCouch,
  ITEM_USAGE.PolearmBracing,
  ITEM_USAGE.PolearmPike,
  ITEM_USAGE.Polearm,
]

export const itemTypeToIcon: Record<ItemType, string> = {
  [ITEM_TYPE.Arrows]: 'item-type-arrow',
  [ITEM_TYPE.Banner]: 'item-type-banner',
  [ITEM_TYPE.BodyArmor]: 'item-type-body-armor',
  [ITEM_TYPE.Bolts]: 'item-type-bolt',
  [ITEM_TYPE.Bow]: 'item-type-bow',
  [ITEM_TYPE.Ranged]: 'item-type-bow', // TODO: need a icon
  [ITEM_TYPE.Ammo]: 'item-type-arrow', // TODO: need a icon
  [ITEM_TYPE.Bullets]: 'item-type-bullet',
  [ITEM_TYPE.Crossbow]: 'item-type-crossbow',
  [ITEM_TYPE.HandArmor]: 'item-type-hand-armor',
  [ITEM_TYPE.HeadArmor]: 'item-type-head-armor',
  [ITEM_TYPE.LegArmor]: 'item-type-leg-armor',
  [ITEM_TYPE.Mount]: 'item-type-mount',
  [ITEM_TYPE.MountHarness]: 'item-type-mount-harness',
  [ITEM_TYPE.Musket]: 'item-type-musket',
  [ITEM_TYPE.OneHandedWeapon]: 'item-type-one-handed-weapon',
  [ITEM_TYPE.Pistol]: 'item-type-pistol',
  [ITEM_TYPE.Polearm]: 'item-type-polearm',
  [ITEM_TYPE.Shield]: 'item-type-shield',
  [ITEM_TYPE.ShoulderArmor]: 'item-type-shoulder-armor',
  [ITEM_TYPE.Thrown]: 'item-type-throwing-weapon',
  [ITEM_TYPE.TwoHandedWeapon]: 'item-type-two-handed-weapon',
  [ITEM_TYPE.Undefined]: '',
}

export const weaponClassToIcon: Record<WeaponClass, string> = {
  [WEAPON_CLASS.Arrow]: 'item-type-arrow',
  [WEAPON_CLASS.Banner]: '',
  [WEAPON_CLASS.Bolt]: 'item-type-bolt',
  [WEAPON_CLASS.Boulder]: '',
  [WEAPON_CLASS.Bow]: 'item-type-bow',
  [WEAPON_CLASS.Cartridge]: 'item-type-bullet',
  [WEAPON_CLASS.Crossbow]: 'item-type-crossbow',
  [WEAPON_CLASS.Dagger]: 'weapon-class-one-handed-dagger',
  [WEAPON_CLASS.Javelin]: 'weapon-class-throwing-spear',
  [WEAPON_CLASS.LargeShield]: 'weapon-class-shield-large',
  [WEAPON_CLASS.LowGripPolearm]: '',
  [WEAPON_CLASS.Mace]: 'weapon-class-one-handed-mace',
  [WEAPON_CLASS.Musket]: 'item-type-musket',
  [WEAPON_CLASS.OneHandedAxe]: 'weapon-class-one-handed-axe',
  [WEAPON_CLASS.OneHandedPolearm]: 'weapon-class-one-handed-polearm',
  [WEAPON_CLASS.OneHandedSword]: 'weapon-class-one-handed-sword',
  [WEAPON_CLASS.Pick]: '',
  [WEAPON_CLASS.Pistol]: 'item-type-pistol',
  [WEAPON_CLASS.SmallShield]: 'weapon-class-shield-small',
  [WEAPON_CLASS.Stone]: 'weapon-class-throwing-stone',
  [WEAPON_CLASS.ThrowingAxe]: 'weapon-class-throwing-axe',
  [WEAPON_CLASS.ThrowingKnife]: 'weapon-class-throwing-knife',
  [WEAPON_CLASS.TwoHandedAxe]: 'weapon-class-two-handed-axe',
  [WEAPON_CLASS.TwoHandedMace]: 'weapon-class-two-handed-mace',
  [WEAPON_CLASS.TwoHandedPolearm]: 'weapon-class-two-handed-polearm',
  [WEAPON_CLASS.TwoHandedSword]: 'weapon-class-two-handed-sword',
  [WEAPON_CLASS.Undefined]: '',
}

export const itemFlagsToIcon: Record<ItemFlag, string | null> = {
  [ITEM_FLAG.CanBePickedUpFromCorpse]: null,
  [ITEM_FLAG.CannotBePickedUp]: null,
  [ITEM_FLAG.Civilian]: null,
  [ITEM_FLAG.DoesNotHideChest]: null,
  [ITEM_FLAG.DoNotScaleBodyAccordingToWeaponLength]: null,
  [ITEM_FLAG.DropOnAnyAction]: null,
  [ITEM_FLAG.DropOnWeaponChange]: 'item-flag-drop-on-change',
  [ITEM_FLAG.ForceAttachOffHandPrimaryItemBone]: null,
  [ITEM_FLAG.ForceAttachOffHandSecondaryItemBone]: null,
  [ITEM_FLAG.HasToBeHeldUp]: null,
  [ITEM_FLAG.HeldInOffHand]: null,
  [ITEM_FLAG.NotStackable]: null,
  [ITEM_FLAG.NotUsableByFemale]: null,
  [ITEM_FLAG.NotUsableByMale]: null,
  [ITEM_FLAG.QuickFadeOut]: null,
  [ITEM_FLAG.UseTeamColor]: 'item-flag-use-team-color',
  [ITEM_FLAG.WoodenAttack]: null,
  [ITEM_FLAG.WoodenParry]: null,
}

export const weaponFlagsToIcon: Partial<Record<WeaponFlag, string>> = {
  [WEAPON_FLAG.AutoReload]: 'item-flag-auto-reload',
  [WEAPON_FLAG.BonusAgainstShield]: 'item-flag-bonus-against-shield',
  [WEAPON_FLAG.CanDismount]: 'item-flag-can-dismount',
  [WEAPON_FLAG.CanKnockDown]: 'item-flag-can-knock-down',
  [WEAPON_FLAG.CanPenetrateShield]: 'item-flag-can-penetrate-shield',
  [WEAPON_FLAG.CanReloadOnHorseback]: 'item-flag-can-reload-on-horseback',
  [WEAPON_FLAG.CantReloadOnHorseback]: 'item-flag-cant-reload-on-horseback',
  [WEAPON_FLAG.CantUseOnHorseback]: 'item-flag-cant-reload-on-horseback',
  [WEAPON_FLAG.MultiplePenetration]: 'item-flag-multiply-penetration',
  [WEAPON_FLAG.TwoHandIdleOnMount]: 'item-flag-two-hand-idle',
}

export const itemUsageToIcon: Partial<Record<ItemUsage, string | null>> = {
  [ITEM_USAGE.Bow]: 'item-flag-short-bow',
  [ITEM_USAGE.Crossbow]: 'item-flag-heavy-crossbow',
  [ITEM_USAGE.CrossbowLight]: 'item-flag-light-crossbow',
  [ITEM_USAGE.LongBow]: 'item-flag-longbow',
  [ITEM_USAGE.Polearm]: 'item-flag-jousting',
  [ITEM_USAGE.PolearmBracing]: 'item-flag-brace',
  [ITEM_USAGE.PolearmCouch]: 'item-flag-couch',
  [ITEM_USAGE.PolearmPike]: 'item-flag-pike',
}

export const itemFamilyTypeToIcon: Record<ItemFamilyType, string | null> = {
  [ITEM_FAMILY_TYPE.Camel]: 'mount-type-camel',
  [ITEM_FAMILY_TYPE.EBA]: null,
  [ITEM_FAMILY_TYPE.Horse]: 'mount-type-horse',
  [ITEM_FAMILY_TYPE.Undefined]: null,
}

export const damageTypeToIcon: Record<DamageType, string | null> = {
  [DAMAGE_TYPE.Blunt]: 'damage-type-blunt',
  [DAMAGE_TYPE.Cut]: 'damage-type-cut',
  [DAMAGE_TYPE.Pierce]: 'damage-type-pierce',
  [DAMAGE_TYPE.Undefined]: null,
}

export interface HumanBucket {
  label: string
  icon: string | null
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
  icon: string | null,
  tooltip: {
    title: string
    description: string | null
  } | null,
): HumanBucket => ({
  icon,
  label,
  tooltip,
})

export const humanizeBucket = (
  aggregationKey: keyof ItemFlat,
  bucket: any,
  item?: ItemFlat,
): HumanBucket => {
  const { n, t } = useI18n() // TODO: FIXME:

  if (bucket === null || bucket === undefined) {
    return createHumanBucket('', null, null)
  }

  if (aggregationKey === 'type') {
    return createHumanBucket(
      t(`item.type.${bucket as ItemType}`),
      itemTypeToIcon[bucket as ItemType],
      {
        title: t(`item.type.${bucket}`),
        description: '',
      },
    )
  }

  if (aggregationKey === 'weaponClass') {
    return createHumanBucket(
      t(`item.weaponClass.${bucket as WeaponClass}`),
      weaponClassToIcon[bucket as WeaponClass],
      {
        title: t(`item.weaponClass.${bucket}`),
        description: '',
      },
    )
  }

  if (aggregationKey === 'damageType') {
    return createHumanBucket(
      t(`item.damageType.${bucket}.long`),
      damageTypeToIcon[bucket as DamageType],
      {
        description: t(`item.damageType.${bucket}.description`),
        title: t(`item.damageType.${bucket}.title`),
      },
    )
  }

  if (aggregationKey === 'culture') {
    return createHumanBucket(
      t(`item.culture.${bucket}`),
      cultureToIcon[bucket as Culture],
      {
        description: null,
        title: t(`item.culture.${bucket}`),
      },
    )
  }

  if (['mountArmorFamilyType', 'mountFamilyType', 'armorFamilyType'].includes(aggregationKey)) {
    return createHumanBucket(
      t(`item.familyType.${bucket}.title`),
      itemFamilyTypeToIcon[bucket as ItemFamilyType],
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
    if (Object.values(ITEM_FLAG).includes(bucket as ItemFlag)) {
      return createHumanBucket(
        t(`item.flags.${bucket}`),
        itemFlagsToIcon[bucket as ItemFlag],
        {
          description: null,
          title: t(`item.flags.${bucket}`),
        },
      )
    }

    if (Object.values(WEAPON_FLAG).includes(bucket as WeaponFlag)) {
      return createHumanBucket(
        t(`item.weaponFlags.${bucket}`),
        weaponFlagsToIcon[bucket as WeaponFlag] ?? null,
        {
          description: null,
          title: t(`item.weaponFlags.${bucket}`),
        },
      )
    }

    if (Object.values(ITEM_USAGE).includes(bucket as ItemUsage)) {
      return createHumanBucket(
        t(`item.usage.${bucket}.title`),
        itemUsageToIcon[bucket as ItemUsage] ?? null,
        {
          description: t(`item.usage.${bucket}.description`),
          title: t(`item.usage.${bucket}.title`),
        },
      )
    }
  }

  const format = aggregationsConfig[aggregationKey]?.format

  if (format === ITEM_FIELD_FORMAT.Damage && item !== undefined) {
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

  if (format === ITEM_FIELD_FORMAT.Requirement) {
    return createHumanBucket(
      t('item.requirementFormat', {
        value: bucket,
      }),
      null,
      null,
    )
  }

  if (format === ITEM_FIELD_FORMAT.Number) {
    return createHumanBucket(n(bucket as number), null, null)
  }

  return createHumanBucket(String(bucket), null, null)
}

interface GroupedItems {
  type: ItemType
  weaponClass: WeaponClass | null
  items: ItemFlat[]
}

export const groupItemsByTypeAndWeaponClass = (items: ItemFlat[]) => {
  return items.reduce((out, item) => {
    const currentEl = out.find((el) => {
      // merge Shield classes
      if (item.type === ITEM_TYPE.Shield) {
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

// TODO: FIXME: spec + desc
export const getCompareItemsResult = (items: ItemFlat[], aggregationsConfig: AggregationConfig): CompareItemsResult => {
  return (Object.keys(aggregationsConfig) as Array<keyof ItemFlat>)
    .filter(k => aggregationsConfig[k]?.compareRule !== undefined)
    .reduce((out, k) => {
      const values = items.map(fi => fi[k]).filter(v => typeof v === 'number') as number[]
      out[k] = aggregationsConfig[k]!.compareRule === ITEM_FIELD_COMPARE_RULE.Less ? Math.min(...values) : Math.max(...values)
      return out
    }, {} as CompareItemsResult)
}

// TODO: FIXME: spec + desc
export const getRelativeEntries = (item: ItemFlat, aggregationsConfig: AggregationConfig): CompareItemsResult => {
  return (Object.keys(aggregationsConfig) as Array<keyof ItemFlat>)
    .filter(k => aggregationsConfig[k]?.compareRule !== undefined)
    .reduce((out, k) => {
      if (typeof item[k] === 'number') {
        out[k] = item[k] as number
      }
      return out
    }, {} as CompareItemsResult)
}

// TODO: description, spec
export const getItemFieldAbsoluteDiffStr = (
  compareRule: ItemFieldCompareRule,
  value: number,
  bestValue: number,
) => {
  const { n } = useI18n() // TODO: FIXME:

  const DEFAULT_STR = ''

  if (value === bestValue) {
    return DEFAULT_STR
  }

  if (compareRule === ITEM_FIELD_COMPARE_RULE.Less) {
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

//  TODO: spec
export const getItemFieldRelativeDiffStr = (value: number, relativeValue: number) => {
  const { n } = useI18n() // TODO: FIXME:

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
  const graceTimeEnd = new Date(userItem.createdAt)
  graceTimeEnd.setMinutes(graceTimeEnd.getMinutes() + itemSellGracePeriodMinutes)
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

export const getRankColor = (rank: number) => {
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

export const canUpgrade = (type: ItemType) => type !== ITEM_TYPE.Banner

export const canAddedToClanArmory = (type: ItemType) => type !== ITEM_TYPE.Banner

export const reforgeCostByRank: Record<number, number> = {
  0: itemReforgeCostPerRank[0] ?? 0,
  1: itemReforgeCostPerRank[1] ?? 0,
  2: itemReforgeCostPerRank[2] ?? 0,
  3: itemReforgeCostPerRank[3] ?? 0,
}

export const itemIsNewDays = 14

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

// // TODO: spec
export const getItemAggregations = (itemFlat: ItemFlat, omitEmpty = true): AggregationConfig => {
  const aggsConfig = getVisibleAggregationsConfig(
    getAggregationsConfig(itemFlat.type, itemFlat.weaponClass),
    ['type', 'weaponClass'],
  )

  return omitEmpty
    ? omitBy(aggsConfig, (_value, field) => itemParamIsEmpty(field as keyof ItemFlat, itemFlat))
    : aggsConfig
}

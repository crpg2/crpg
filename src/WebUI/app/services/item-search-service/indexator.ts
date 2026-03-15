import { uniq } from 'es-toolkit'

import type {
  Item,
  ItemFlag,
  ItemFlat,
  ItemType,
  ItemUsage,
  ItemWeaponComponent,
  WeaponClass,
  WeaponFlag,
} from '~/models/item'

import {
  DAMAGE_TYPE,
  ITEM_FAMILY_TYPE,
  ITEM_FLAG,
  ITEM_TYPE,
  ITEM_USAGE,
  WEAPON_CLASS,
  WEAPON_FLAG,
  WEAPON_USAGE,
} from '~/models/item'
import {
  computeAverageRepairCostPerHour,
  isLargeShield,
  itemIsNewDays,
} from '~/services/item-service'
import { roundFLoat } from '~/utils/math'

const WeaponClassByItemUsage: Partial<Record<ItemUsage, WeaponClass>> = {
  [ITEM_USAGE.Polearm]: WEAPON_CLASS.OneHandedPolearm, // jousting lances
  [ITEM_USAGE.PolearmCouch]: WEAPON_CLASS.OneHandedPolearm,
}

const VISIBLE_ITEM_FLAGS: ItemFlag[] = [
  ITEM_FLAG.DropOnWeaponChange,
  ITEM_FLAG.DropOnAnyAction,
  ITEM_FLAG.UseTeamColor,
]

const VISIBLE_ITEM_USAGE: ItemUsage[] = [
  ITEM_USAGE.LongBow,
  ITEM_USAGE.Bow,
  ITEM_USAGE.Crossbow,
  ITEM_USAGE.CrossbowLight,
  ITEM_USAGE.PolearmCouch,
  ITEM_USAGE.PolearmBracing,
  ITEM_USAGE.PolearmPike,
  ITEM_USAGE.Polearm,
]

const VISIBLE_WEAPON_FLAGS: WeaponFlag[] = [
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

const VISIBLE_ITEM_FLAGS_SET = new Set(VISIBLE_ITEM_FLAGS)
const VISIBLE_ITEM_USAGE_SET = new Set(VISIBLE_ITEM_USAGE)
const VISIBLE_WEAPON_FLAGS_SET = new Set(VISIBLE_WEAPON_FLAGS)

const RANGED_ITEM_TYPES_SET = new Set<ItemType>([ITEM_TYPE.Bow, ITEM_TYPE.Crossbow, ITEM_TYPE.Musket, ITEM_TYPE.Pistol])
const AMMO_DAMAGE_ITEM_TYPES_SET = new Set<ItemType>([ITEM_TYPE.Bolts, ITEM_TYPE.Arrows, ITEM_TYPE.Thrown, ITEM_TYPE.Bullets])
const AMMO_ITEM_TYPES_SET = new Set<ItemType>([ITEM_TYPE.Arrows, ITEM_TYPE.Bolts, ITEM_TYPE.Bullets])

const itemTypeByWeaponClass: Record<WeaponClass, ItemType> = {
  [WEAPON_CLASS.Arrow]: ITEM_TYPE.Ammo,
  [WEAPON_CLASS.Bolt]: ITEM_TYPE.Ammo,
  [WEAPON_CLASS.Cartridge]: ITEM_TYPE.Ammo,
  [WEAPON_CLASS.Bullets]: ITEM_TYPE.Ammo,

  [WEAPON_CLASS.Boulder]: ITEM_TYPE.Thrown,
  [WEAPON_CLASS.Javelin]: ITEM_TYPE.Thrown,
  [WEAPON_CLASS.ThrowingAxe]: ITEM_TYPE.Thrown,
  [WEAPON_CLASS.ThrowingKnife]: ITEM_TYPE.Thrown,
  [WEAPON_CLASS.Stone]: ITEM_TYPE.Thrown,

  [WEAPON_CLASS.Bow]: ITEM_TYPE.Ranged,
  [WEAPON_CLASS.Crossbow]: ITEM_TYPE.Ranged,
  [WEAPON_CLASS.Musket]: ITEM_TYPE.Ranged,
  [WEAPON_CLASS.Pistol]: ITEM_TYPE.Ranged,

  [WEAPON_CLASS.Dagger]: ITEM_TYPE.OneHandedWeapon,
  [WEAPON_CLASS.Mace]: ITEM_TYPE.OneHandedWeapon,
  [WEAPON_CLASS.OneHandedAxe]: ITEM_TYPE.OneHandedWeapon,
  [WEAPON_CLASS.OneHandedSword]: ITEM_TYPE.OneHandedWeapon,

  [WEAPON_CLASS.Pick]: ITEM_TYPE.TwoHandedWeapon,
  [WEAPON_CLASS.TwoHandedAxe]: ITEM_TYPE.TwoHandedWeapon,
  [WEAPON_CLASS.TwoHandedMace]: ITEM_TYPE.TwoHandedWeapon,
  [WEAPON_CLASS.TwoHandedSword]: ITEM_TYPE.TwoHandedWeapon,

  [WEAPON_CLASS.LowGripPolearm]: ITEM_TYPE.Polearm,
  [WEAPON_CLASS.OneHandedPolearm]: ITEM_TYPE.Polearm,
  [WEAPON_CLASS.TwoHandedPolearm]: ITEM_TYPE.Polearm,

  [WEAPON_CLASS.LargeShield]: ITEM_TYPE.Shield,
  [WEAPON_CLASS.SmallShield]: ITEM_TYPE.Shield,

  [WEAPON_CLASS.Banner]: ITEM_TYPE.Banner,

  [WEAPON_CLASS.Undefined]: ITEM_TYPE.Undefined,
}

const createEmptyWeapon = () => ({
  accuracy: null,
  aimSpeed: null,
  damage: null,
  damageType: null,
  handling: null,
  itemUsage: [],
  length: null,
  missileSpeed: null,
  reloadSpeed: null,
  shieldArmor: null,
  shieldDurability: null,
  shieldSpeed: null,
  stackAmount: null,
  swingDamage: null,
  swingDamageType: null,
  swingSpeed: null,
  thrustDamage: null,
  thrustDamageType: null,
  thrustSpeed: null,
  weaponClass: null,
  weaponFlags: [],
  weaponPrimaryClass: null,
})

const mapWeaponProps = (item: Item) => {
  const emptyWeapon = createEmptyWeapon()

  if (item.weapons.length === 0) {
    return emptyWeapon
  }

  const [originalWeapon] = item.weapons

  if (!originalWeapon) {
    return emptyWeapon
  }

  const weapon = {
    ...emptyWeapon,
    accuracy: originalWeapon.accuracy,
    handling: originalWeapon.handling,
    itemUsage: [originalWeapon.itemUsage],
    length: originalWeapon.length,
    missileSpeed: originalWeapon.missileSpeed,
    stackAmount: originalWeapon.stackAmount,
    swingDamage: originalWeapon.swingSpeed !== 0 ? originalWeapon.swingDamage : 0,
    swingDamageType:
      originalWeapon.swingDamageType === DAMAGE_TYPE.Undefined || originalWeapon.swingDamage === 0
        ? undefined
        : originalWeapon.swingDamageType,
    swingSpeed: originalWeapon.swingDamage !== 0 ? originalWeapon.swingSpeed : 0,
    thrustDamage: originalWeapon.thrustSpeed !== 0 ? originalWeapon.thrustDamage : 0,
    thrustDamageType:
      originalWeapon.thrustDamageType === DAMAGE_TYPE.Undefined || originalWeapon.thrustDamage === 0
        ? undefined
        : originalWeapon.thrustDamageType,
    thrustSpeed: originalWeapon.thrustDamage !== 0 ? originalWeapon.thrustSpeed : 0,
    weaponClass: originalWeapon.class,
    weaponFlags: [...originalWeapon.flags],
    weaponPrimaryClass: originalWeapon.class,
  }

  if (item.type === ITEM_TYPE.Shield) {
    const weaponFlags = isLargeShield(item) && !weapon.weaponFlags.includes(WEAPON_FLAG.CantUseOnHorseback)
      ? [...weapon.weaponFlags, WEAPON_FLAG.CantUseOnHorseback]
      : weapon.weaponFlags

    return {
      ...weapon,
      weaponFlags,
      shieldArmor: originalWeapon.bodyArmor,
      shieldDurability: originalWeapon.stackAmount,
      shieldSpeed: originalWeapon.swingSpeed,
    }
  }

  if (RANGED_ITEM_TYPES_SET.has(item.type)) {
    // add custom flag
    const weaponFlags = (
      item.type === ITEM_TYPE.Crossbow
      && !originalWeapon.flags.includes(WEAPON_FLAG.CantReloadOnHorseback)
      && !weapon.weaponFlags.includes(WEAPON_FLAG.CanReloadOnHorseback)
    )
      ? [...weapon.weaponFlags, WEAPON_FLAG.CanReloadOnHorseback]
      : weapon.weaponFlags

    return {
      ...weapon,
      weaponFlags,
      aimSpeed: originalWeapon.thrustSpeed,
      damage: originalWeapon.thrustDamage,
      reloadSpeed: originalWeapon.swingSpeed,
    }
  }

  if (AMMO_DAMAGE_ITEM_TYPES_SET.has(item.type)) {
    return {
      ...weapon,
      damage: originalWeapon.thrustDamage,
      damageType:
        originalWeapon.thrustDamageType === DAMAGE_TYPE.Undefined
          ? undefined
          : originalWeapon.thrustDamageType,
    }
  }

  return weapon
}

const mapArmorProps = (item: Item) => {
  if (item.armor === null) {
    return {
      armArmor: null,
      armorFamilyType: null,
      armorMaterialType: null,
      bodyArmor: null,
      headArmor: null,
      legArmor: null,
      mountArmor: null,
      mountArmorFamilyType: null,
    }
  }

  if (item.type === ITEM_TYPE.MountHarness) {
    return {
      ...item.armor,
      armorFamilyType: null,
      armorMaterialType: item.armor.materialType,
      mountArmor: item.armor.bodyArmor,
      mountArmorFamilyType: item.armor.familyType,
    }
  }

  return {
    ...item.armor,
    armorFamilyType:
      item.armor.familyType !== ITEM_FAMILY_TYPE.Undefined ? item.armor.familyType : undefined,
    armorMaterialType: item.armor.materialType,
    mountArmor: null,
    mountArmorFamilyType: null,
  }
}

const mapWeight = (item: Item) => {
  if (AMMO_DAMAGE_ITEM_TYPES_SET.has(item.type)) {
    const [weapon] = item.weapons

    return {
      stackWeight: roundFLoat(item.weight * weapon!.stackAmount),
      weight: null,
    }
  }

  return {
    stackWeight: null,
    weight: roundFLoat(item.weight),
  }
}

const mapMountProps = (item: Item) => {
  if (item.mount === null) {
    return {
      bodyLength: null,
      chargeDamage: null,
      hitPoints: null,
      maneuver: null,
      mountFamilyType: null,
      speed: null,
    }
  }

  return {
    ...item.mount,
    mountFamilyType: item.mount.familyType,
  }
}

const generateModId = (itemId: Item, itemType: ItemType, weaponClass?: WeaponClass) => {
  return `${itemId.id}_${itemType}${weaponClass !== undefined ? `_${weaponClass}` : ''}`
}

/**
 *
 * @description Change the type for grouping to UI (e.g., group ranged or ammo)
 */
const mapItemType = (type: ItemType): ItemType => {
  if (RANGED_ITEM_TYPES_SET.has(type)) {
    return ITEM_TYPE.Ranged
  }

  if (AMMO_ITEM_TYPES_SET.has(type)) {
    return ITEM_TYPE.Ammo
  }

  return type
}

const itemToFlat = <
  TItem extends { item: Item },
  TMeta extends { [key: string]: unknown } = { [key: string]: unknown },
>(
  wrapper: TItem,
  newItemDateThreshold: number,
  writeMeta?: (item: TItem) => TMeta,
): ItemFlat<TMeta> => {
  const weaponProps = mapWeaponProps(wrapper.item)

  const flags = uniq([
    ...wrapper.item.flags.filter(flag => VISIBLE_ITEM_FLAGS_SET.has(flag)),
    ...weaponProps.weaponFlags.filter(wf => VISIBLE_WEAPON_FLAGS_SET.has(wf)),
    ...weaponProps.itemUsage.filter(iu => VISIBLE_ITEM_USAGE_SET.has(iu)),
  ])

  const { id, type, baseId, culture, name, createdAt, price, rank, requirement, tier } = wrapper.item

  const resolvedType = mapItemType(type)
  return {
    id,
    type: resolvedType,
    baseId,
    culture,
    flags,
    modId: generateModId(wrapper.item, resolvedType, weaponProps.weaponClass ?? undefined),
    name,
    isNew: createdAt.getTime() > newItemDateThreshold,
    price,
    rank,
    requirement,
    tier: roundFLoat(tier),
    upkeep: computeAverageRepairCostPerHour(price),
    weaponUsage: [WEAPON_USAGE.Primary],
    ...mapWeight(wrapper.item),
    ...mapArmorProps(wrapper.item),
    ...mapMountProps(wrapper.item),
    ...weaponProps,
    upgrades: [],
    meta: writeMeta ? writeMeta(wrapper) : ({} as TMeta),
  }
}

const normalizeWeaponClass = (_itemType: ItemType, weapon: ItemWeaponComponent) => {
  if (weapon.itemUsage in WeaponClassByItemUsage) {
    return WeaponClassByItemUsage[weapon.itemUsage]!
  }

  return weapon.class
}

const checkWeaponIsPrimaryUsage = (
  itemType: ItemType,
  weapon: ItemWeaponComponent,
  weapons: ItemWeaponComponent[],
) => {
  let isPrimaryUsage = false

  const weaponClass = normalizeWeaponClass(itemType, weapon)

  if (itemType === ITEM_TYPE.Polearm) {
    const hasCouch = weapons.some(w => w.itemUsage === ITEM_USAGE.PolearmCouch)
    // jousting lances
    const isJoustingLanceHack = weapons.some(w => w.itemUsage === ITEM_USAGE.Polearm)
    if (!isJoustingLanceHack && hasCouch && weapon.class !== WEAPON_CLASS.OneHandedPolearm) {
      return false
    }
  }

  isPrimaryUsage = itemType === itemTypeByWeaponClass[weaponClass]

  return isPrimaryUsage
}

const getPrimaryWeaponClass = (item: Item) => {
  const primaryWeapon = item.weapons.find(w =>
    checkWeaponIsPrimaryUsage(item.type, w, item.weapons),
  )

  if (primaryWeapon !== undefined) {
    return primaryWeapon.class
  }

  return null
}

export interface CreateItemIndexOptions<TItem, TMeta extends { [key: string]: unknown }> {
  cloneMultipleUsageWeapon?: boolean
  writeMeta?: (item: TItem) => TMeta
}

export function createItemIndex<
  TMeta extends { [key: string]: unknown },
>(
  items: Item[],
  options?: CreateItemIndexOptions<Item, TMeta>,
): ItemFlat[]
export function createItemIndex<
  TWrappedItem extends { item: Item },
  TMeta extends { [key: string]: unknown },
>(
  items: TWrappedItem[],
  options?: CreateItemIndexOptions<TWrappedItem, TMeta>):
ItemFlat<TMeta>[]

export function createItemIndex<
  TWrappedItem extends { item: Item },
  TMeta extends { [key: string]: unknown } = { [key: string]: unknown },
>(
  items: (Item | TWrappedItem)[],
  options?: CreateItemIndexOptions<TWrappedItem, TMeta>,
): ItemFlat<TMeta>[] {
  const writeMeta = options?.writeMeta
  const cloneMultipleUsageWeapon = options?.cloneMultipleUsageWeapon ?? false
  const newItemDateThreshold = new Date().setDate(new Date().getDate() - itemIsNewDays)
  const outByModId = new Map<string, number>()

  // TODO: try to remove cloneDeep
  const result = items
    .reduce<ItemFlat<TMeta>[]>((out, _wrapper) => {
      const wrapper = 'item' in _wrapper ? _wrapper as TWrappedItem : { item: _wrapper } as TWrappedItem
      // TODO: bows have 2 loading modes, so there are several objects in the weapons data structure
      if (wrapper.item.weapons.length > 1 && wrapper.item.type !== ITEM_TYPE.Bow) {
        wrapper.item.weapons.forEach((w) => {
          const weaponClass = normalizeWeaponClass(wrapper.item.type, w)
          const isPrimaryUsage = checkWeaponIsPrimaryUsage(wrapper.item.type, w, wrapper.item.weapons)
          const targetType = itemTypeByWeaponClass[weaponClass]
          const targetModId = generateModId(wrapper.item, targetType, weaponClass)
          // fixes a duplicate class, ex. Hoe: 1h/2h/1h
          const itemTypeAlreadyExistIdx = outByModId.get(targetModId) ?? -1

          // console.table({
          //   idx,
          //   out: JSON.stringify(out.map(dd => ({ class: dd.weaponClass, type: dd.type }))),
          //   type: item.type,
          //   class: w.class,
          //   itemTypeByWeaponClass: itemTypeByWeaponClass[w.class],
          //   itemTypeAlreadyExistIdx,
          //   isPrimaryUsage,
          //   modId: generateModId(item, w.class),
          // });

          // merge itemUsage, if the weapon has several of the same class
          if (itemTypeAlreadyExistIdx !== -1) {
            if (VISIBLE_ITEM_USAGE_SET.has(w.itemUsage) && !out[itemTypeAlreadyExistIdx]?.flags.includes(w.itemUsage)) {
              out[itemTypeAlreadyExistIdx]?.flags.push(w.itemUsage)
            }
            return
          }

          if (isPrimaryUsage || cloneMultipleUsageWeapon) {
            out.push({
              ...itemToFlat(
                {
                  ...wrapper,
                  item: {
                    ...wrapper.item,
                    type: targetType,
                    weapons: [{ ...w, class: weaponClass }], // TODO: ?
                  },
                },
                newItemDateThreshold,
                writeMeta,
              ),
              weaponPrimaryClass: isPrimaryUsage ? weaponClass : getPrimaryWeaponClass(wrapper.item),
              weaponUsage: [isPrimaryUsage ? WEAPON_USAGE.Primary : WEAPON_USAGE.Secondary],
            })
            outByModId.set(targetModId, out.length - 1)
          }
        })
      }
      else {
        const flat = itemToFlat(
          wrapper,
          newItemDateThreshold,
          writeMeta,
        )
        out.push(flat)
        outByModId.set(flat.modId, out.length - 1)
      }

      return out
    }, [])

  // console.log(result);

  return result
}

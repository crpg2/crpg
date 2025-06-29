import { cloneDeep } from 'es-toolkit'

import type {
  Item,
  ItemFlat,
  ItemWeaponComponent,
} from '~/models/item'

import {
  DamageType,
  ItemFamilyType,
  ItemType,
  ItemUsage,
  WeaponClass,
  WeaponFlags,
  WeaponUsage,
} from '~/models/item'
import {
  computeAverageRepairCostPerHour,
  isLargeShield,
  itemIsNewDays,
  itemTypeByWeaponClass,
  visibleItemFlags,
  visibleItemUsage,
  visibleWeaponFlags,
  WeaponClassByItemUsage,
} from '~/services/item-service'
import { roundFLoat } from '~/utils/math'

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
      originalWeapon.swingDamageType === DamageType.Undefined || originalWeapon.swingDamage === 0
        ? undefined
        : originalWeapon.swingDamageType,
    swingSpeed: originalWeapon.swingDamage !== 0 ? originalWeapon.swingSpeed : 0,
    thrustDamage: originalWeapon.thrustSpeed !== 0 ? originalWeapon.thrustDamage : 0,
    thrustDamageType:
      originalWeapon.thrustDamageType === DamageType.Undefined || originalWeapon.thrustDamage === 0
        ? undefined
        : originalWeapon.thrustDamageType,
    thrustSpeed: originalWeapon.thrustDamage !== 0 ? originalWeapon.thrustSpeed : 0,
    weaponClass: originalWeapon.class,
    weaponFlags: originalWeapon.flags,
    weaponPrimaryClass: originalWeapon.class,
  }

  if (item.type === ItemType.Shield) {
    return {
      ...weapon,
      shieldArmor: originalWeapon.bodyArmor,
      shieldDurability: originalWeapon.stackAmount,
      shieldSpeed: originalWeapon.swingSpeed,
    }
  }

  if ([ItemType.Bow, ItemType.Crossbow, ItemType.Musket, ItemType.Pistol].includes(item.type)) {
    // add custom flag
    if (
      item.type === ItemType.Crossbow
      && !originalWeapon.flags.includes(WeaponFlags.CantReloadOnHorseback)
    ) {
      weapon.weaponFlags.push(WeaponFlags.CanReloadOnHorseback)
    }

    return {
      ...weapon,
      aimSpeed: originalWeapon.thrustSpeed,
      damage: originalWeapon.thrustDamage,
      reloadSpeed: originalWeapon.swingSpeed,
    }
  }

  if ([ItemType.Bolts, ItemType.Arrows, ItemType.Thrown, ItemType.Bullets].includes(item.type)) {
    return {
      ...weapon,
      damage: originalWeapon.thrustDamage,
      damageType:
        originalWeapon.thrustDamageType === DamageType.Undefined
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

  if (item.type === ItemType.MountHarness) {
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
      item.armor.familyType !== ItemFamilyType.Undefined ? item.armor.familyType : undefined,
    armorMaterialType: item.armor.materialType,
    mountArmor: null,
    mountArmorFamilyType: null,
  }
}

const mapWeight = (item: Item) => {
  if ([ItemType.Thrown, ItemType.Bolts, ItemType.Arrows].includes(item.type)) {
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

const generateModId = (item: Item, weaponClass?: WeaponClass) => {
  return `${item.id}_${item.type}${weaponClass !== undefined ? `_${weaponClass}` : ''}`
}

/**
 *
 * @description Change the type for grouping to UI (e.g., group ranged or ammo)
 */
const mapItemType = (type: ItemType): ItemType => {
  if ([ItemType.Bow, ItemType.Crossbow, ItemType.Musket, ItemType.Pistol].includes(type)) {
    return ItemType.Ranged
  }

  if ([ItemType.Arrows, ItemType.Bolts, ItemType.Bullets].includes(type)) {
    return ItemType.Ammo
  }

  return type
}

const itemToFlat = (item: Item): ItemFlat => {
  const newItemDateThreshold = new Date().setDate(new Date().getDate() - itemIsNewDays)

  const weaponProps = mapWeaponProps(item)

  const flags = [
    ...item.flags.filter(flag => visibleItemFlags.includes(flag)),
    ...weaponProps.weaponFlags.filter(wf => visibleWeaponFlags.includes(wf)),
    ...weaponProps.itemUsage.filter(iu => visibleItemUsage.includes(iu)),
  ]

  // Banning the use of large shields on horseback
  if (isLargeShield(item)) {
    flags.push(WeaponFlags.CantUseOnHorseback)
  }

  return {
    id: item.id,
    type: mapItemType(item.type),
    baseId: item.baseId,
    culture: item.culture,
    flags,
    modId: generateModId(item, weaponProps?.weaponClass ?? undefined),
    name: item.name,
    new: new Date(item.createdAt).getTime() > newItemDateThreshold ? 1 : 0,
    price: item.price,
    rank: item.rank,
    requirement: item.requirement,
    tier: roundFLoat(item.tier),
    upkeep: computeAverageRepairCostPerHour(item.price),
    weaponUsage: [WeaponUsage.Primary],
    ...mapWeight(item),
    ...mapArmorProps(item),
    ...mapMountProps(item),
    ...weaponProps,
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

  if (itemType === ItemType.Polearm) {
    const hasCouch = weapons.some(w => w.itemUsage === ItemUsage.PolearmCouch)
    // TODO: jousting lances
    const isJoustingLanceHack = weapons.some(w => w.itemUsage === ItemUsage.Polearm)

    // const hasBrace = weapons.some(w => w.itemUsage === ItemUsage.PolearmBracing);
    // const hasPike = weapons.some(w => w.itemUsage === ItemUsage.PolearmPike);

    // console.log({itemType, 'weapon.class': weapon.class, weaponClass, hasCouch });

    if (!isJoustingLanceHack && hasCouch && weapon.class !== WeaponClass.OneHandedPolearm) {
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

// TODO: FIXME: SPEC cloneMultipleUsageWeapon param
export const createItemIndex = (items: Item[], cloneMultipleUsageWeapon = false): ItemFlat[] => {
  // TODO: try to remove cloneDeep
  const result = cloneDeep(items).reduce((out, item) => {
    if (item.weapons.length > 1
    // TODO: bows have 2 loading modes, so there are several objects in the weapons data structure
      && item.type !== ItemType.Bow) {
      item.weapons.forEach((w, _idx) => {
        const weaponClass = normalizeWeaponClass(item.type, w)

        const isPrimaryUsage = checkWeaponIsPrimaryUsage(item.type, w, item.weapons)

        // fixes a duplicate class, ex. Hoe: 1h/2h/1h
        const itemTypeAlreadyExistIdx = out.findIndex((fi) => {
          // console.table({
          //   fiType: fi.type,
          //   itemType: item.type,
          //   fiModId: fi.modId,
          //   itemId: generateModId(item, w.class),
          //   exist:
          //     fi.modId ===
          //     generateModId({ ...item, type: itemTypeByWeaponClass[w.class] }, w.class),
          // });

          return (
            fi.modId
            === generateModId({ ...item, type: itemTypeByWeaponClass[weaponClass] }, weaponClass)
          )
        })

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
          if (visibleItemUsage.includes(w.itemUsage)) {
            out[itemTypeAlreadyExistIdx]?.flags.push(w.itemUsage)
          }

          return
        }

        if (isPrimaryUsage || cloneMultipleUsageWeapon) {
          out.push({
            ...itemToFlat({
              ...item,
              type: itemTypeByWeaponClass[weaponClass],
              weapons: [{ ...w, class: weaponClass }], // TODO:
            }),
            weaponPrimaryClass: isPrimaryUsage ? weaponClass : getPrimaryWeaponClass(item),
            weaponUsage: [isPrimaryUsage ? WeaponUsage.Primary : WeaponUsage.Secondary],
          })
        }
      })
    }
    //
    else {
      out.push(itemToFlat(item))
    }

    return out
  }, [] as ItemFlat[])

  // console.log(result);

  return result
}

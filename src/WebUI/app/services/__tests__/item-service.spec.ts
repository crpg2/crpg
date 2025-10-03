// import type { PartialDeep } from 'type-fest'

// // import { mockGet } from 'vi-fetch'
// import { describe, expect, it, vi } from 'vitest'

// import type { EquippedItemsBySlot } from '~/models/character'
// import type { Item, ItemFlat } from '~/models/item'
// import type { AggregationConfig } from '~/models/item-search'
// import type { UserItem } from '~/models/user'

// import { response } from '~/__mocks__/crpg-client'
// import mockItems from '~/__mocks__/items.json'
// import { Culture } from '~/models/culture'
// import {
//   DamageType,
//   ItemFamilyType,
//   ItemFieldCompareRule,
//   ItemFieldFormat,
//   ItemFlags,
//   ItemSlot,
//   ItemType,
//   ItemUsage,
//   WeaponClass,
//   WeaponFlags,
// } from '~/models/item'
// import { AggregationView } from '~/models/item-search'

// import type { HumanBucket } from './item-service'

// import {
//   canAddedToClanArmory,
//   canUpgrade,
//   computeAverageRepairCostPerHour,
//   computeBrokenItemRepairCost,
//   computeSalePrice,
//   getAvailableSlotsByItem,
//   getCompareItemsResult,
//   getItemFieldAbsoluteDiffStr,
//   getItemGraceTimeEnd,
//   getItemImage,
//   getItems,
//   //   getItemUpgrades,
//   getLinkedSlots,
//   getRelativeEntries,
//   getWeaponClassesByItemType,
//   groupItemsByTypeAndWeaponClass,
//   hasWeaponClassesByItemType,
//   humanizeBucket,
//   IconBucketType,
//   isGraceTimeExpired,
// } from './item-service'

// vi.mock('~/services/item-search-service/aggregations.ts', () => ({
//   aggregationsConfig: {
//     damage: {
//       format: ItemFieldFormat.Damage,
//       view: AggregationView.Range,
//     },
//     flags: {
//       format: ItemFieldFormat.List,
//       view: AggregationView.Checkbox,
//     },
//     price: {
//       format: ItemFieldFormat.Number,
//       view: AggregationView.Range,
//     },
//     requirement: {
//       compareRule: ItemFieldCompareRule.Less,
//       format: ItemFieldFormat.Requirement,
//       view: AggregationView.Range,
//     },
//     swingDamage: {
//       format: ItemFieldFormat.Damage,
//       view: AggregationView.Range,
//     },
//     thrustDamage: {
//       format: ItemFieldFormat.Damage,
//       view: AggregationView.Range,
//     },
//     type: {
//       view: AggregationView.Checkbox,
//     },
//   } satisfies AggregationConfig,
// }))

// const { mockedNotify } = vi.hoisted(() => ({
//   mockedNotify: vi.fn(),
// }))

// beforeEach(() => {
//   vi.useFakeTimers()
//   vi.setSystemTime('2022-11-27T21:00:00.0000000Z')
// })

// afterEach(() => {
//   vi.useRealTimers()
// })

// it('getItems', async () => {
//   mockGet('/items').willResolve(response<Item[]>(mockItems as unknown as Item[]))

//   expect(await getItems()).toEqual(mockItems)
// })

// it('getItemImage', () => {
//   expect(getItemImage('crpg_aserai_noble_sword_2_t5')).toEqual(
//     '/items/crpg_aserai_noble_sword_2_t5.webp',
//   )
// })

// it.each<[PartialDeep<Item>, PartialDeep<EquippedItemsBySlot>, ItemSlot[]]>([
//   [
//     { armor: { familyType: ItemFamilyType.Horse }, flags: [], type: ItemType.MountHarness },
//     {},
//     [ItemSlot.MountHarness],
//   ],
//   [
//     { armor: { familyType: ItemFamilyType.Horse }, flags: [], type: ItemType.MountHarness },
//     { [ItemSlot.Mount]: { item: { mount: { familyType: ItemFamilyType.Horse } } } },
//     [ItemSlot.MountHarness],
//   ],
//   [
//     { armor: { familyType: ItemFamilyType.Camel }, flags: [], type: ItemType.MountHarness },
//     {},
//     [ItemSlot.MountHarness],
//   ],
//   [
//     { armor: { familyType: ItemFamilyType.Camel }, flags: [], type: ItemType.MountHarness },
//     { [ItemSlot.Mount]: { item: { mount: { familyType: ItemFamilyType.Horse } } } },
//     [],
//   ],
//   [
//     { armor: { familyType: ItemFamilyType.Horse }, flags: [], type: ItemType.MountHarness },
//     { [ItemSlot.Mount]: { item: { mount: { familyType: ItemFamilyType.Camel } } } },
//     [],
//   ],
//   [
//     { flags: [], type: ItemType.OneHandedWeapon },
//     {},
//     [ItemSlot.Weapon0, ItemSlot.Weapon1, ItemSlot.Weapon2, ItemSlot.Weapon3],
//   ],
//   [{ flags: [], type: ItemType.Banner }, {}, [ItemSlot.WeaponExtra]],
//   [{ flags: [ItemFlags.DropOnWeaponChange], type: ItemType.Polearm }, {}, [ItemSlot.WeaponExtra]], // Pike

//   // Large shields on horseback
//   [
//     { flags: [], type: ItemType.Shield, weapons: [{ class: WeaponClass.LargeShield }] },
//     {},
//     [ItemSlot.Weapon0, ItemSlot.Weapon1, ItemSlot.Weapon2, ItemSlot.Weapon3],
//   ],
//   [
//     { flags: [], type: ItemType.Shield, weapons: [{ class: WeaponClass.LargeShield }] },
//     { [ItemSlot.Mount]: { item: { mount: { familyType: ItemFamilyType.Horse } } } },
//     [],
//   ],
//   [
//     { flags: [], type: ItemType.Mount },
//     {
//       [ItemSlot.Weapon2]: {
//         item: { type: ItemType.Shield, weapons: [{ class: WeaponClass.LargeShield }] },
//       },
//     },
//     [],
//   ],
//   [
//     { flags: [], type: ItemType.Shield, weapons: [{ class: WeaponClass.SmallShield }] },
//     { [ItemSlot.Mount]: { item: { mount: { familyType: ItemFamilyType.Horse } } } },
//     [ItemSlot.Weapon0, ItemSlot.Weapon1, ItemSlot.Weapon2, ItemSlot.Weapon3],
//   ],
//   [
//     { flags: [], type: ItemType.Mount },
//     {
//       [ItemSlot.Weapon2]: {
//         item: { type: ItemType.Shield, weapons: [{ class: WeaponClass.SmallShield }] },
//       },
//     },
//     [ItemSlot.Mount],
//   ],

//   // EBA
//   [{ armor: { familyType: ItemFamilyType.EBA }, flags: [], type: ItemType.BodyArmor }, {}, []],
//   [
//     { armor: { familyType: ItemFamilyType.EBA }, flags: [], type: ItemType.BodyArmor },
//     {
//       [ItemSlot.Leg]: {
//         item: { armor: { familyType: ItemFamilyType.Undefined }, type: ItemType.LegArmor },
//       },
//     },
//     [],
//   ],
//   [
//     { armor: { familyType: ItemFamilyType.EBA }, flags: [], type: ItemType.BodyArmor },
//     {
//       [ItemSlot.Leg]: {
//         item: { armor: { familyType: ItemFamilyType.EBA }, type: ItemType.LegArmor },
//       },
//     },
//     [ItemSlot.Body],
//   ],
//   [
//     { armor: { familyType: ItemFamilyType.EBA }, flags: [], type: ItemType.BodyArmor },
//     {
//       [ItemSlot.Leg]: {
//         item: { armor: { familyType: ItemFamilyType.Undefined }, type: ItemType.LegArmor },
//       },
//     },
//     [],
//   ],
//   [
//     { armor: { familyType: ItemFamilyType.Undefined }, flags: [], type: ItemType.LegArmor },
//     {
//       [ItemSlot.Body]: {
//         item: { armor: { familyType: ItemFamilyType.EBA }, type: ItemType.BodyArmor },
//       },
//     },
//     [],
//   ],
//   [
//     { armor: { familyType: ItemFamilyType.EBA }, flags: [], type: ItemType.LegArmor },
//     {
//       [ItemSlot.Body]: {
//         item: { armor: { familyType: ItemFamilyType.EBA }, type: ItemType.BodyArmor },
//       },
//     },
//     [ItemSlot.Leg],
//   ],
// ])('getAvailableSlotsByItem - item: %j, equipedItems: %j', (item, equipedItems, expectation) => {
//   expect(getAvailableSlotsByItem(item as Item, equipedItems as EquippedItemsBySlot)).toEqual(
//     expectation,
//   )
// })

// it.each<[ItemSlot, PartialDeep<EquippedItemsBySlot>, ItemSlot[]]>([
//   // EBA
//   [
//     ItemSlot.Leg,
//     {
//       [ItemSlot.Body]: {
//         item: { armor: { familyType: ItemFamilyType.EBA }, type: ItemType.BodyArmor },
//       },
//     },
//     [ItemSlot.Body],
//   ],
//   [
//     ItemSlot.Leg,
//     {
//       [ItemSlot.Body]: {
//         item: { armor: { familyType: ItemFamilyType.Undefined }, type: ItemType.BodyArmor },
//       },
//     },
//     [],
//   ],
// ])('getLinkedSlots - slot: %s', (slot, equipedItems, expectation) => {
//   expect(getLinkedSlots(slot, equipedItems as EquippedItemsBySlot)).toEqual(expectation)
// })

// it.each([
//   [ItemType.Bow, false],
//   [ItemType.Polearm, true],
// ])('hasWeaponClassesByItemType - itemType: %s', (itemType, expectation) => {
//   expect(hasWeaponClassesByItemType(itemType)).toEqual(expectation)
// })

// it.each<[ItemType, WeaponClass[]]>([
//   [
//     ItemType.OneHandedWeapon,
//     [WeaponClass.OneHandedSword, WeaponClass.OneHandedAxe, WeaponClass.Mace, WeaponClass.Dagger],
//   ],
//   [ItemType.Bow, []],
// ])('getWeaponClassesByItemType - itemType: %s', (itemType, expectation) => {
//   expect(getWeaponClassesByItemType(itemType)).toEqual(expectation)
// })

// describe('humanizeBucket', () => {
//   it.each<[keyof ItemFlat, any, HumanBucket]>([
//     ['type', null, { icon: null, label: '', tooltip: null }],
//     [
//       'type',
//       ItemType.OneHandedWeapon,
//       {
//         icon: { name: 'item-type-one-handed-weapon', type: IconBucketType.Svg },
//         label: 'item.type.OneHandedWeapon',
//         tooltip: null,
//       },
//     ],
//     [
//       'weaponClass',
//       WeaponClass.OneHandedPolearm,
//       {
//         icon: { name: 'weapon-class-one-handed-polearm', type: IconBucketType.Svg },
//         label: 'item.weaponClass.OneHandedPolearm',
//         tooltip: null,
//       },
//     ],
//     [
//       'damageType',
//       DamageType.Cut,
//       {
//         icon: { name: 'damage-type-cut', type: IconBucketType.Svg },
//         label: 'item.damageType.Cut.long',
//         tooltip: {
//           description: 'item.damageType.Cut.description',
//           title: 'item.damageType.Cut.title',
//         },
//       },
//     ],
//     [
//       'culture',
//       Culture.Vlandia,
//       {
//         icon: { name: 'culture-vlandia', type: IconBucketType.Asset },
//         label: 'item.culture.Vlandia',
//         tooltip: {
//           description: null,
//           title: 'item.culture.Vlandia',
//         },
//       },
//     ],
//     [
//       'mountArmorFamilyType',
//       ItemFamilyType.Horse,
//       {
//         icon: { name: 'mount-type-horse', type: IconBucketType.Svg },
//         label: 'item.familyType.1.title',
//         tooltip: {
//           description: 'item.familyType.1.description',
//           title: 'item.familyType.1.title',
//         },
//       },
//     ],
//     [
//       'mountFamilyType',
//       ItemFamilyType.Camel,
//       {
//         icon: { name: 'mount-type-camel', type: IconBucketType.Svg },
//         label: 'item.familyType.2.title',
//         tooltip: {
//           description: 'item.familyType.2.description',
//           title: 'item.familyType.2.title',
//         },
//       },
//     ],
//     [
//       'armorFamilyType',
//       ItemFamilyType.EBA,
//       {
//         icon: null,
//         label: 'item.familyType.3.title',
//         tooltip: {
//           description: 'item.familyType.3.description',
//           title: 'item.familyType.3.title',
//         },
//       },
//     ],
//     [
//       'flags',
//       ItemFlags.DropOnWeaponChange,
//       {
//         icon: { name: 'item-flag-drop-on-change', type: IconBucketType.Svg },
//         label: 'item.flags.DropOnWeaponChange',
//         tooltip: {
//           description: null,
//           title: 'item.flags.DropOnWeaponChange',
//         },
//       },
//     ],
//     [
//       'flags',
//       WeaponFlags.CanDismount,
//       {
//         icon: { name: 'item-flag-can-dismount', type: IconBucketType.Svg },
//         label: 'item.weaponFlags.CanDismount',
//         tooltip: {
//           description: null,
//           title: 'item.weaponFlags.CanDismount',
//         },
//       },
//     ],
//     [
//       'flags',
//       ItemUsage.PolearmBracing,
//       {
//         icon: { name: 'item-flag-brace', type: IconBucketType.Svg },
//         label: 'item.usage.polearm_bracing.title',
//         tooltip: {
//           description: 'item.usage.polearm_bracing.description',
//           title: 'item.usage.polearm_bracing.title',
//         },
//       },
//     ],
//     [
//       'requirement',
//       18,
//       {
//         icon: null,
//         label: 'item.requirementFormat::value:18',
//         tooltip: null,
//       },
//     ],
//     [
//       'price',
//       1234,
//       {
//         icon: null,
//         label: '1234',
//         tooltip: null,
//       },
//     ],
//     [
//       'handling',
//       12,
//       {
//         icon: null,
//         label: '12',
//         tooltip: null,
//       },
//     ],
//   ])('aggKey: %s, bucket: %s ', (aggKey, bucket, expectation) => {
//     expect(humanizeBucket(aggKey, bucket)).toEqual(expectation)
//   })

//   it('swingDamage', () => {
//     const item: Partial<ItemFlat> = {
//       swingDamageType: DamageType.Cut,
//     }

//     expect(humanizeBucket('swingDamage', 10, item as ItemFlat)).toEqual({
//       icon: null,
//       label: 'item.damageTypeFormat::value:10,type:item.damageType.Cut.short',
//       tooltip: {
//         description: 'item.damageType.Cut.description',
//         title: 'item.damageType.Cut.title',
//       },
//     })
//   })

//   it('thrustDamage', () => {
//     const item: Partial<ItemFlat> = {}

//     expect(humanizeBucket('thrustDamage', 0, item as ItemFlat)).toEqual({
//       icon: null,
//       label: '0',
//       tooltip: null,
//     })
//   })
// })

// it('compareItemsResult', () => {
//   const items = [
//     {
//       flags: ['UseTeamColor'],
//       handling: 82,
//       length: 105,
//       weight: 2.22,
//     },
//     {
//       flags: [],
//       handling: 86,
//       length: 99,
//       weight: 2.07,
//     },
//   ] as ItemFlat[]

//   const aggregationConfig = {
//     flags: {
//       title: 'Flags',
//     },
//     handling: {
//       compareRule: 'Bigger',
//       title: 'Handling',
//     },
//     length: {
//       compareRule: 'Bigger',
//       title: 'Length',
//     },
//     weight: {
//       compareRule: 'Less',
//       title: 'Thrust damage',
//     },
//   } as AggregationConfig

//   expect(getCompareItemsResult(items, aggregationConfig)).toEqual({
//     handling: 86,
//     length: 105,
//     weight: 2.07,
//   })
// })

// it('groupItemsByTypeAndWeaponClass', () => {
//   const items = [
//     {
//       id: 'item1',
//       type: ItemType.OneHandedWeapon,
//       weaponClass: WeaponClass.OneHandedSword,
//     },
//     {
//       id: 'item2',
//       type: ItemType.OneHandedWeapon,
//       weaponClass: WeaponClass.OneHandedSword,
//     },
//     {
//       id: 'item3',
//       type: ItemType.OneHandedWeapon,
//       weaponClass: WeaponClass.OneHandedAxe,
//     },
//     // Shield Case
//     {
//       id: 'item4',
//       type: ItemType.Shield,
//       weaponClass: WeaponClass.SmallShield,
//     },
//     {
//       id: 'item5',
//       type: ItemType.Shield,
//       weaponClass: WeaponClass.LargeShield,
//     },
//   ] as ItemFlat[]

//   expect(groupItemsByTypeAndWeaponClass(items)).toEqual([
//     {
//       items: [
//         {
//           id: 'item1',
//           type: 'OneHandedWeapon',
//           weaponClass: 'OneHandedSword',
//         },
//         {
//           id: 'item2',
//           type: 'OneHandedWeapon',
//           weaponClass: 'OneHandedSword',
//         },
//       ],
//       type: 'OneHandedWeapon',
//       weaponClass: 'OneHandedSword',
//     },
//     {
//       items: [
//         {
//           id: 'item3',
//           type: 'OneHandedWeapon',
//           weaponClass: 'OneHandedAxe',
//         },
//       ],
//       type: 'OneHandedWeapon',
//       weaponClass: 'OneHandedAxe',
//     },
//     {
//       items: [
//         {
//           id: 'item4',
//           type: 'Shield',
//           weaponClass: 'SmallShield',
//         },
//         {
//           id: 'item5',
//           type: 'Shield',
//           weaponClass: 'LargeShield',
//         },
//       ],
//       type: 'Shield',
//       weaponClass: 'SmallShield',
//     },
//   ])
// })

// it('getRelativeEntries', () => {
//   const item = {
//     flags: ['UseTeamColor'],
//     handling: 82,
//     length: 105,
//     weight: 2.22,
//   } as ItemFlat

//   const aggregationConfig = {
//     flags: {
//       title: 'Flags',
//     },
//     handling: {
//       compareRule: 'Bigger',
//       title: 'Handling',
//     },
//     length: {
//       compareRule: 'Bigger',
//       title: 'Length',
//     },
//     weight: {
//       compareRule: 'Less',
//       title: 'Thrust damage',
//     },
//   } as AggregationConfig

//   expect(getRelativeEntries(item, aggregationConfig)).toEqual({
//     handling: 82,
//     length: 105,
//     weight: 2.22,
//   })
// })

// it.each<[ItemFieldCompareRule, number, number, string]>([
//   [ItemFieldCompareRule.Bigger, 0, 0, ''],
//   [ItemFieldCompareRule.Bigger, -1, 0, '-1'],
//   [ItemFieldCompareRule.Bigger, -1, -1, ''],

//   [ItemFieldCompareRule.Bigger, 1, 1, ''],
//   [ItemFieldCompareRule.Bigger, 2, 1, ''],
//   [ItemFieldCompareRule.Bigger, 1, 2, '-1'],

//   [ItemFieldCompareRule.Less, 0, 0, ''],
//   [ItemFieldCompareRule.Less, 0, -1, '+1'],
//   [ItemFieldCompareRule.Less, -1, -1, ''],

//   [ItemFieldCompareRule.Less, 1, 1, ''],
//   [ItemFieldCompareRule.Less, 1, 2, ''],
//   [ItemFieldCompareRule.Less, 2, 1, '+1'],
// ])(
//   'getItemFieldAbsoluteDiffStr - compareRule: %s, value: %s, bestValue: %s',
//   (compareRule, value, bestValue, expectation) => {
//     expect(getItemFieldAbsoluteDiffStr(compareRule, value, bestValue)).toEqual(expectation)
//   },
// )

// it('getItemGraceTimeEnd', () => {
//   const userItem: PartialDeep<UserItem> = {
//     createdAt: new Date('2022-11-27T20:00:00.0000000Z'),
//   }

//   expect(getItemGraceTimeEnd(userItem as UserItem)).toEqual(
//     new Date('2022-11-27T21:00:00.0000000Z'),
//   )
// })

// it.each<[Date, boolean]>([
//   [new Date('2022-11-27T20:00:00.0000000Z'), true],
//   [new Date('2022-11-27T20:59:00.0000000Z'), true],
//   [new Date('2022-11-27T21:00:00.0000000Z'), false],
//   [new Date('2022-11-27T21:01:00.0000000Z'), false],
// ])('isGraceTimeExpired - date: %s,', (itemGraceTimeEnd, expectation) => {
//   expect(isGraceTimeExpired(itemGraceTimeEnd)).toEqual(expectation)
// })

// it.each<[PartialDeep<UserItem>, { price: number, graceTimeEnd: Date | null }]>([
//   [
//     {
//       createdAt: new Date('2022-11-27T20:00:00.0000000Z'),
//       item: {
//         price: 1100,
//       },
//     },
//     {
//       graceTimeEnd: new Date('2022-11-27T21:00:00.000Z'),
//       price: 1100,
//     },
//   ],
//   [
//     {
//       createdAt: new Date('2022-11-27T19:00:00.0000000Z'),
//       item: {
//         price: 1100,
//       },
//     },
//     {
//       graceTimeEnd: null,
//       price: 550,
//     },
//   ],
// ])('computeSalePrice - userItem: %s,', (userItem, expectation) => {
//   expect(computeSalePrice(userItem as UserItem)).toEqual(expectation)
// })

// it.each<[number, number]>([
//   [1, 0],
//   [10, 0],
//   [100, 1],
//   [1000, 19],
//   [10000, 192],
//   [100000, 1919],
// ])('computeBrokenItemRepairCost - price: %s,', (price, expectation) => {
//   expect(computeBrokenItemRepairCost(price)).toEqual(expectation)
// })

// it.each<[number, number]>([
//   [1, 0],
//   [10, 1],
//   [100, 11],
//   [1000, 115],
//   [10000, 1152],
//   [100000, 11519],
// ])('computeAverageRepairCostPerHour - price: %s,', (price, expectation) => {
//   expect(computeAverageRepairCostPerHour(price)).toEqual(expectation)
// })

// it.each<[ItemType, boolean]>([
//   [ItemType.OneHandedWeapon, true],
//   [ItemType.Banner, false],
// ])('canUpgrade - type: %s', (itemType, expectation) => {
//   expect(canUpgrade(itemType)).toEqual(expectation)
// })

// it.todo('tODO', () => {
//   // getItemUpgrades();
// })

// it.each<[ItemType, boolean]>([
//   [ItemType.OneHandedWeapon, true],
//   [ItemType.Banner, false],
// ])('canAddedToClanArmory - type: %s', (itemType, expectation) => {
//   expect(canAddedToClanArmory(itemType)).toEqual(expectation)
// })

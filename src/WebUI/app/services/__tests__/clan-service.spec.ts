// mport type { PartialDeep } from 'type-fest'

// import { mockDelete, mockGet, mockPost, mockPut } from 'vi-fetch'

// import type {
//   Clan,
//   ClanArmoryItem,
//   ClanInvitation,
//   ClanMember,
//   ClanWithMemberCount,
// } from '~/models/clan'
// import type { UserItem, UserPublic } from '~/models/user'

// import { response } from '~/__mocks__/crpg-client'
// import {
//   ClanInvitationStatus,
//   ClanInvitationType,
//   ClanMemberRole,
// } from '~/models/clan'
// import { Language } from '~/models/language'
// import { Region } from '~/models/region'

// import {
//   addItemToClanArmory,
//   borrowItemFromClanArmory,
//   canKickMemberValidate,
//   canManageApplicationsValidate,
//   canUpdateClanValidate,
//   canUpdateMemberValidate,
//   createClan,
//   getClan,
//   getClanArmory,
//   getClanArmoryItemBorrower,
//   getClanArmoryItemLender,
//   getClanInvitations,
//   getClanMember,
//   getClanMembers,
//   getClans,
//   getFilteredClans,
//   inviteToClan,
//   isClanArmoryItemInInventory,
//   isOwnClanArmoryItem,
//   kickClanMember,
//   removeItemFromClanArmory,
//   respondToClanInvitation,
//   returnItemToClanArmory,
//   updateClan,
//   updateClanMember,
// } from './clan-service'

// vi.mock('~/services/auth-service', () => ({
//   getToken: vi.fn().mockResolvedValue('mockedToken'),
// }))

// const { ARGB_INT, HEX_COLOR } = vi.hoisted(() => ({
//   ARGB_INT: 'ArgbInt',
//   HEX_COLOR: 'HexColor',
// }))
// const { mockedArgbIntToRgbHexColor, mockedRgbHexColorToArgbInt } = vi.hoisted(() => ({
//   mockedArgbIntToRgbHexColor: vi.fn().mockReturnValue(HEX_COLOR),
//   mockedRgbHexColorToArgbInt: vi.fn().mockReturnValue(ARGB_INT),
// }))
// vi.mock('~/utils/color', () => ({
//   argbIntToRgbHexColor: mockedArgbIntToRgbHexColor,
//   rgbHexColorToArgbInt: mockedRgbHexColorToArgbInt,
// }))

// it('getClans', async () => {
//   const mockGetClans = [
//     {
//       clan: {
//         name: 'My Little Pony',
//         primaryColor: 4278190080,
//         secondaryColor: 4278190080,
//         tag: 'mlp',
//       },
//       memberCount: 4,
//     },
//   ]

//   mockGet('/clans').willResolve(response(mockGetClans))
//   await getClans()
//   expect(mockedArgbIntToRgbHexColor).toBeCalledTimes(2)
// })

// it.each<[Region, Language[], string, number[]]>([
//   [Region.Eu, [], '', [1, 3]],
//   [Region.Na, [], '', [2]],
//   [Region.Na, [], 'UNI', [2]],
//   [Region.Na, [], 'FOAL', []],
//   [Region.Eu, [], 'Unicorns', []],
//   [Region.Eu, [], 'Po', [3]],
//   [Region.Eu, [Language.Fr], 'Po', [3]],
// ])(
//   'getFilteredClans - region: %s, searchQuery: %s',
//   (region, languages, searchQuery, expectation) => {
//     const clans = [
//       { clan: { id: 1, name: 'Foals', region: Region.Eu, tag: 'FOAL' } },
//       { clan: { id: 2, name: 'Unicorns', region: Region.Na, tag: 'UNIC' } },
//       { clan: { id: 3, languages: [Language.Fr], name: 'Ponies', region: Region.Eu, tag: 'PONY' } },
//     ] as ClanWithMemberCount[]

//     expect(getFilteredClans(clans, region, languages, searchQuery).map(c => c.clan.id)).toEqual(
//       expectation,
//     )
//   },
// )

// it('createClan', async () => {
//   const newClan = {
//     bannerKey: '22222222',
//     discord: 'gg.mlp.gg',
//     name: 'My Little Pony Clan',
//     primaryColor: '#fff',
//     region: Region.Eu,
//     secondaryColor: '#fff',
//     tag: 'MLP',
//   } as Omit<Clan, 'id'>

//   const mock = mockPost('/clans').willResolve(response(newClan))

//   expect(await createClan(newClan)).toEqual({
//     ...newClan,
//     primaryColor: HEX_COLOR,
//     secondaryColor: HEX_COLOR,
//   })

//   expect(mock).toHaveFetchedWithBody({
//     ...newClan,
//     primaryColor: ARGB_INT,
//     secondaryColor: ARGB_INT,
//   })
// })

// it('updateClan', async () => {
//   const clanId = 1
//   const clan = {
//     bannerKey: '22222222',
//     discord: 'gg.mlp.gg',
//     id: 1,
//     name: 'My Little Pony Clan',
//     primaryColor: '4278190080',
//     region: Region.Eu,
//     secondaryColor: '4278190080',
//     tag: 'MLP',
//   } as Clan

//   const mock = mockPut(`/clans/${clanId}`).willResolve(response(clan))

//   expect(await updateClan(clanId, clan)).toEqual({
//     ...clan,
//     primaryColor: HEX_COLOR,
//     secondaryColor: HEX_COLOR,
//   })

//   expect(mock).toHaveFetchedWithBody({
//     ...clan,
//     primaryColor: ARGB_INT,
//     secondaryColor: ARGB_INT,
//   })
// })

// it('getClan', async () => {
//   const clanId = 1
//   const clan = {
//     bannerKey: '22222222',
//     discord: 'gg.mlp.gg',
//     id: 1,
//     name: 'My Little Pony Clan',
//     primaryColor: 4278190080,
//     region: Region.Eu,
//     secondaryColor: 4278190080,
//     tag: 'MLP',
//   }

//   mockGet(`/clans/${clanId}`).willResolve(response(clan))

//   expect(await getClan(clanId)).toEqual({
//     ...clan,
//     primaryColor: HEX_COLOR,
//     secondaryColor: HEX_COLOR,
//   })
// })

// it('getClanMembers', async () => {
//   const clanId = 1
//   const members = [
//     {
//       role: 'Leader',
//       user: {
//         id: 1,
//         name: 'Rarity',
//       },
//     },
//   ]

//   mockGet(`/clans/${clanId}/members`).willResolve(response(members))

//   expect(await getClanMembers(clanId)).toEqual(members)
// })

// it('updateClanMember', async () => {
//   const clanId = 1
//   const memberId = 4
//   const newRole = ClanMemberRole.Officer
//   const updatedMember = {
//     role: newRole,
//     user: {
//       id: memberId,
//       name: 'Rarity',
//     },
//   }

//   const mock = mockPut(`/clans/${clanId}/members/${memberId}`).willResolve(response(updatedMember))

//   expect(await updateClanMember(clanId, memberId, newRole)).toEqual(updatedMember)

//   expect(mock).toHaveFetchedWithBody({ role: newRole })
// })

// it('kickClanMember', async () => {
//   const clanId = 1
//   const memberId = 4

//   mockDelete(`/clans/${clanId}/members/${memberId}`).willResolve(null, 204)

//   expect(await kickClanMember(clanId, memberId)).toEqual(null)
// })

// it('inviteToClan', async () => {
//   const clanId = 1
//   const inviteeId = 3
//   const newInvitation = {
//     id: 1,
//     invitee: {},
//     inviter: {},
//     status: 'Pending',
//     type: 'Request',
//   } as ClanInvitation

//   const mock = mockPost(`/clans/${clanId}/invitations`).willResolve(response(newInvitation))

//   expect(await inviteToClan(clanId, inviteeId)).toEqual(newInvitation)

//   expect(mock).toHaveFetchedWithBody({ inviteeId })
// })

// it('getClanInvitations', async () => {
//   const clanId = 1
//   const clanInvitations = [
//     {
//       id: 1,
//       invitee: {},
//       inviter: {},
//       status: 'Pending',
//       type: 'Request',
//     },
//   ] as ClanInvitation[]

//   mockGet(`/clans/${clanId}/invitations?type%5B%5D=Request&status%5B%5D=Pending`, true).willResolve(
//     response(clanInvitations),
//   )

//   expect(
//     await getClanInvitations(clanId, [ClanInvitationType.Request], [ClanInvitationStatus.Pending]),
//   ).toEqual(clanInvitations)
// })

// it('respondToClanInvitation', async () => {
//   const clanId = 1
//   const clanInvitationId = 4
//   const clanInvitation = {
//     id: 1,
//     invitee: {},
//     inviter: {},
//     status: 'Accepted',
//     type: 'Request',
//   } as ClanInvitation

//   const mock = mockPut(`/clans/${clanId}/invitations/${clanInvitationId}/response`).willResolve(
//     response(clanInvitation),
//   )

//   expect(await respondToClanInvitation(clanId, clanInvitationId, true)).toEqual(clanInvitation)
//   expect(mock).toHaveFetchedWithBody({ accept: true })
// })

// it.each<[PartialDeep<ClanMember[]>, number, PartialDeep<ClanMember> | null]>([
//   [[], 1, null],
//   [[{ user: { id: 1 } }], 1, { user: { id: 1 } }],
//   [[{ user: { id: 1 } }, { user: { id: 2 } }], 2, { user: { id: 2 } }],
//   [[{ user: { id: 2 } }], 1, null],
// ])('getClanMember - members: %j, userId: %s', (members, userId, expectation) => {
//   expect(getClanMember(members as ClanMember[], userId)).toEqual(expectation)
// })

// it.each<[ClanMemberRole, boolean]>([
//   [ClanMemberRole.Leader, true],
//   [ClanMemberRole.Officer, true],
//   [ClanMemberRole.Member, false],
// ])('canManageApplicationsValidate - role: %j', (role, expectation) => {
//   expect(canManageApplicationsValidate(role)).toEqual(expectation)
// })

// it.each<[ClanMemberRole, boolean]>([
//   [ClanMemberRole.Leader, true],
//   [ClanMemberRole.Officer, false],
//   [ClanMemberRole.Member, false],
// ])('canUpdateClanValidate - role: %j', (role, expectation) => {
//   expect(canUpdateClanValidate(role)).toEqual(expectation)
// })

// it.each<[ClanMemberRole, boolean]>([
//   [ClanMemberRole.Leader, true],
//   [ClanMemberRole.Officer, false],
//   [ClanMemberRole.Member, false],
// ])('canUpdateMemberValidate - role: %j', (role, expectation) => {
//   expect(canUpdateMemberValidate(role)).toEqual(expectation)
// })

// it.each<[PartialDeep<ClanMember>, PartialDeep<ClanMember>, number, boolean]>([
//   // You can't leave the clan if there is more than one person
//   [
//     { role: ClanMemberRole.Leader, user: { id: 1 } },
//     { role: ClanMemberRole.Leader, user: { id: 1 } },
//     2,
//     false,
//   ],
//   [
//     { role: ClanMemberRole.Leader, user: { id: 1 } },
//     { role: ClanMemberRole.Leader, user: { id: 1 } },
//     1,
//     true,
//   ],
//   // You can leave the clan
//   [
//     { role: ClanMemberRole.Officer, user: { id: 1 } },
//     { role: ClanMemberRole.Officer, user: { id: 1 } },
//     2,
//     true,
//   ],
//   [
//     { role: ClanMemberRole.Leader, user: { id: 1 } },
//     { role: ClanMemberRole.Officer, user: { id: 2 } },
//     2,
//     true,
//   ],
//   // an officer cannot kick a leader
//   [
//     { role: ClanMemberRole.Officer, user: { id: 1 } },
//     { role: ClanMemberRole.Leader, user: { id: 2 } },
//     2,
//     false,
//   ],
//   // an officer can kick a member
//   [
//     { role: ClanMemberRole.Officer, user: { id: 1 } },
//     { role: ClanMemberRole.Member, user: { id: 2 } },
//     2,
//     true,
//   ],
// ])(
//   'canKickMemberValidate - selfMember: %j, member: %j, clanMembersCount: %s',
//   (selfMember, member, clanMembersCount, expectation) => {
//     expect(
//       canKickMemberValidate(selfMember as ClanMember, member as ClanMember, clanMembersCount),
//     ).toEqual(expectation)
//   },
// )

// it('getClanArmory', async () => {
//   const clanId = 1
//   const clanArmory = [{ borrowedItem: null, updatedAt: '', userItem: {} }]
//   mockGet(`/clans/${clanId}/armory`).willResolve(response(clanArmory))
//   expect(await getClanArmory(clanId)).toEqual(clanArmory)
// })

// it('addItemToClanArmory', async () => {
//   const clanId = 1
//   const userItemId = 1
//   const mock = mockPost(`/clans/${clanId}/armory`).willResolve(response('ok'))
//   expect(await addItemToClanArmory(clanId, userItemId)).toEqual('ok')
//   expect(mock).toHaveFetchedWithBody({ userItemId })
// })

// it('removeItemFromClanArmory', async () => {
//   const clanId = 1
//   const userItemId = 1
//   mockDelete(`/clans/${clanId}/armory/${userItemId}`).willResolve(response('ok'))
//   expect(await removeItemFromClanArmory(clanId, userItemId)).toEqual('ok')
// })

// it('borrowItemFromClanArmory', async () => {
//   const clanId = 1
//   const userItemId = 1
//   mockPut(`/clans/${clanId}/armory/${userItemId}/borrow`).willResolve(response('ok'))
//   expect(await borrowItemFromClanArmory(clanId, userItemId)).toEqual('ok')
// })

// it('returnItemToClanArmory', async () => {
//   const clanId = 1
//   const userItemId = 1
//   mockPut(`/clans/${clanId}/armory/${userItemId}/return`).willResolve(response('ok'))
//   expect(await returnItemToClanArmory(clanId, userItemId)).toEqual('ok')
// })

// it.each<[ClanArmoryItem, ClanMember[], UserPublic | null]>([
//   [
//     {
//       borrowedItem: null,
//       updatedAt: new Date(),
//       userItem: {},
//     } as ClanArmoryItem,
//     [],
//     null,
//   ],
//   [
//     {
//       borrowedItem: {
//         borrowerUserId: 1,
//       },
//       updatedAt: new Date(),
//       userItem: {},
//     } as ClanArmoryItem,
//     [],
//     null,
//   ],
//   [
//     {
//       borrowedItem: {
//         borrowerUserId: 1,
//       },
//       updatedAt: new Date(),
//       userItem: {},
//     } as ClanArmoryItem,
//     [
//       {
//         user: {
//           id: 1,
//         },
//       } as ClanMember,
//     ],
//     { id: 1 } as UserPublic,
//   ],
// ])('getClanArmoryItemBorrower', (clanArmoryItem, clanMembers, expectation) => {
//   expect(getClanArmoryItemBorrower(clanArmoryItem, clanMembers)).toEqual(expectation)
// })

// it.each<[UserItem, ClanMember[], UserPublic | null]>([
//   [
//     {
//       isArmoryItem: false,
//       userId: 1,
//     } as UserItem,
//     [],
//     null,
//   ],
//   [
//     {
//       isArmoryItem: true,
//       userId: 2,
//     } as UserItem,
//     [],
//     null,
//   ],
//   [
//     {
//       isArmoryItem: true,
//       userId: 2,
//     } as UserItem,
//     [
//       {
//         user: {
//           id: 1,
//         },
//       } as ClanMember,
//     ],
//     null,
//   ],
//   [
//     {
//       isArmoryItem: true,
//       userId: 2,
//     } as UserItem,
//     [
//       {
//         user: {
//           id: 2,
//         },
//       } as ClanMember,
//     ],
//     { id: 2 } as UserPublic,
//   ],
// ])('getClanArmoryItemLender', (userItem, clanMembers, expectation) => {
//   expect(getClanArmoryItemLender(userItem, clanMembers)).toEqual(expectation)
// })

// it.each<[ClanArmoryItem, number, boolean]>([
//   [{ userItem: { userId: 1 } } as ClanArmoryItem, 1, true],
//   [{ userItem: { userId: 1 } } as ClanArmoryItem, 2, false],
// ])('isOwnClanArmoryItem', (clanArmoryItem, userId, expectation) => {
//   expect(isOwnClanArmoryItem(clanArmoryItem, userId)).toEqual(expectation)
// })

// it.each<[ClanArmoryItem, UserItem[], boolean]>([
//   [{ userItem: { item: { id: 'item1' } } } as ClanArmoryItem, [], false],
//   [
//     { userItem: { item: { id: 'item1' } } } as ClanArmoryItem,
//     [
//       {
//         item: {
//           id: 'item1',
//         },
//       } as UserItem,
//     ],
//     true,
//   ],
//   [
//     { userItem: { item: { id: 'item1' } } } as ClanArmoryItem,
//     [
//       {
//         item: {
//           id: 'item2',
//         },
//       } as UserItem,
//     ],
//     false,
//   ],
// ])('isClanArmoryItemInInventory', (clanArmoryItem, userItems, expectation) => {
//   expect(isClanArmoryItemInInventory(clanArmoryItem, userItems)).toEqual(expectation)
// })

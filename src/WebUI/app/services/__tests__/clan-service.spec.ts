import type { PartialDeep } from 'type-fest'

import { describe, expect, it, vi } from 'vitest'

import type { ClanArmoryItem, ClanMember, ClanMemberRole } from '~/models/clan'
import type { UserPublic } from '~/models/user'

import { CLAN_MEMBER_ROLE } from '~/models/clan'

import { canKickMemberValidate, canManageApplicationsValidate, canUpdateClanValidate, canUpdateMemberValidate, getClanArmoryItemBorrower, getClanArmoryItemLender, isOwnClanArmoryItem } from '../clan-service'

const {
  mockedGetClans,
  mockedDeleteClansByClanIdArmoryByUserItemId,
  mockedDeleteClansByClanIdMembersByUserId,
  mockedGetClansByClanIdArmory,
  mockedGetClansByClanIdInvitations,
  mockedGetClansById,
  mockedGetClansByIdMembers,
  mockedPostClans,
  mockedPostClansByClanIdArmory,
  mockedPostClansByClanIdInvitations,
  mockedPutClansByClanId,
  mockedPutClansByClanIdArmoryByUserItemIdBorrow,
  mockedPutClansByClanIdArmoryByUserItemIdReturn,
  mockedPutClansByClanIdInvitationsByInvitationIdResponse,
  mockedPutClansByClanIdMembersByUserId,
} = vi.hoisted(() => ({
  mockedGetClans: vi.fn(),
  mockedDeleteClansByClanIdArmoryByUserItemId: vi.fn(),
  mockedDeleteClansByClanIdMembersByUserId: vi.fn(),
  mockedGetClansByClanIdArmory: vi.fn(),
  mockedGetClansByClanIdInvitations: vi.fn(),
  mockedGetClansById: vi.fn(),
  mockedGetClansByIdMembers: vi.fn(),
  mockedPostClans: vi.fn(),
  mockedPostClansByClanIdArmory: vi.fn(),
  mockedPostClansByClanIdInvitations: vi.fn(),
  mockedPutClansByClanId: vi.fn(),
  mockedPutClansByClanIdArmoryByUserItemIdBorrow: vi.fn(),
  mockedPutClansByClanIdArmoryByUserItemIdReturn: vi.fn(),
  mockedPutClansByClanIdInvitationsByInvitationIdResponse: vi.fn(),
  mockedPutClansByClanIdMembersByUserId: vi.fn(),
}))

vi.mock('#api/sdk.gen', () => ({
  getClans: mockedGetClans,
  deleteClansByClanIdArmoryByUserItemId: mockedDeleteClansByClanIdArmoryByUserItemId,
  deleteClansByClanIdMembersByUserId: mockedDeleteClansByClanIdMembersByUserId,
  getClansByClanIdArmory: mockedGetClansByClanIdArmory,
  getClansByClanIdInvitations: mockedGetClansByClanIdInvitations,
  getClansById: mockedGetClansById,
  getClansByIdMembers: mockedGetClansByIdMembers,
  postClans: mockedPostClans,
  postClansByClanIdArmory: mockedPostClansByClanIdArmory,
  postClansByClanIdInvitations: mockedPostClansByClanIdInvitations,
  putClansByClanId: mockedPutClansByClanId,
  putClansByClanIdArmoryByUserItemIdBorrow: mockedPutClansByClanIdArmoryByUserItemIdBorrow,
  putClansByClanIdArmoryByUserItemIdReturn: mockedPutClansByClanIdArmoryByUserItemIdReturn,
  putClansByClanIdInvitationsByInvitationIdResponse: mockedPutClansByClanIdInvitationsByInvitationIdResponse,
  putClansByClanIdMembersByUserId: mockedPutClansByClanIdMembersByUserId,
}))

describe('clan servie', () => {
  it.each<[ClanMemberRole, boolean]>([
    [CLAN_MEMBER_ROLE.Leader, true],
    [CLAN_MEMBER_ROLE.Officer, true],
    [CLAN_MEMBER_ROLE.Member, false],
  ])('canManageApplicationsValidate - role: %j', (role, expectation) => {
    expect(canManageApplicationsValidate(role)).toEqual(expectation)
  })

  it.each<[ClanMemberRole, boolean]>([
    [CLAN_MEMBER_ROLE.Leader, true],
    [CLAN_MEMBER_ROLE.Officer, false],
    [CLAN_MEMBER_ROLE.Member, false],
  ])('canUpdateClanValidate - role: %j', (role, expectation) => {
    expect(canUpdateClanValidate(role)).toEqual(expectation)
  })

  it.each<[ClanMemberRole, boolean]>([
    [CLAN_MEMBER_ROLE.Leader, true],
    [CLAN_MEMBER_ROLE.Officer, false],
    [CLAN_MEMBER_ROLE.Member, false],
  ])('canUpdateMemberValidate - role: %j', (role, expectation) => {
    expect(canUpdateMemberValidate(role)).toEqual(expectation)
  })

  it.each<[string, PartialDeep<ClanMember>, PartialDeep<ClanMember>, number, boolean]>([
    [
      'returns false when a leader tries to leave a clan with more than one member',
      { role: CLAN_MEMBER_ROLE.Leader, user: { id: 1 } },
      { role: CLAN_MEMBER_ROLE.Leader, user: { id: 1 } },
      2,
      false,
    ],
    [
      'returns true when a leader tries to leave a clan with only one member',
      { role: CLAN_MEMBER_ROLE.Leader, user: { id: 1 } },
      { role: CLAN_MEMBER_ROLE.Leader, user: { id: 1 } },
      1,
      true,
    ],
    [
      'returns true when an officer tries to leave the clan',
      { role: CLAN_MEMBER_ROLE.Officer, user: { id: 1 } },
      { role: CLAN_MEMBER_ROLE.Officer, user: { id: 1 } },
      2,
      true,
    ],
    [
      'returns true when a leader tries to remove an officer',
      { role: CLAN_MEMBER_ROLE.Leader, user: { id: 1 } },
      { role: CLAN_MEMBER_ROLE.Officer, user: { id: 2 } },
      2,
      true,
    ],
    [
      'returns false when an officer tries to kick a leader',
      { role: CLAN_MEMBER_ROLE.Officer, user: { id: 1 } },
      { role: CLAN_MEMBER_ROLE.Leader, user: { id: 2 } },
      2,
      false,
    ],
    [
      'returns true when an officer tries to kick a regular member',
      { role: CLAN_MEMBER_ROLE.Officer, user: { id: 1 } },
      { role: CLAN_MEMBER_ROLE.Member, user: { id: 2 } },
      2,
      true,
    ],
  ])(
    'canKickMemberValidate - %s',
    (_, selfMember, member, clanMembersCount, expectation) => {
      expect(
        canKickMemberValidate(
          selfMember as ClanMember,
          member as ClanMember,
          clanMembersCount,
        ),
      ).toEqual(expectation)
    },
  )

  it.each<[string, number, ClanMember[], UserPublic | null]>([
    [
      'returns null when the clan has no members',
      1,
      [],
      null,
    ],
    [
      'returns the borrower when the user is found among clan members',
      1,
      [
        {
          user: {
            id: 1,
          },
        },
      ] as ClanMember[],
      { id: 1 } as UserPublic,
    ],
    [
      'returns null when the borrower is not found among clan members',
      2,
      [
        {
          user: {
            id: 1,
          },
        },
      ] as ClanMember[],
      null,
    ],
  ])('getClanArmoryItemBorrower %s', (_, borrowerUserId, clanMembers, expectedResult) => {
    expect(getClanArmoryItemBorrower(borrowerUserId, clanMembers)).toEqual(expectedResult)
  })

  it.each<[string, number, ClanMember[], UserPublic | null]>([
    [
      'returns null when the clan has no members',
      1,
      [],
      null,
    ],
    [
      'returns the lender when the user is found among clan members',
      1,
      [
        {
          user: {
            id: 1,
          },
        },
      ] as ClanMember[],
      { id: 1 } as UserPublic,
    ],
    [
      'returns null when the lender is not found among clan members',
      2,
      [
        {
          user: {
            id: 1,
          },
        },
      ] as ClanMember[],
      null,
    ],
  ])('getClanArmoryItemLender - %s', (_, userId, clanMembers, expectedResult) => {
    expect(getClanArmoryItemLender(userId, clanMembers)).toEqual(expectedResult)
  })

  it.each<[string, ClanArmoryItem, number, boolean]>([
    [
      'returns true when the item belongs to the current user',
      { userId: 1 } as ClanArmoryItem,
      1,
      true,
    ],
    [
      'returns false when the item belongs to another user',
      { userId: 2 } as ClanArmoryItem,
      1,
      false,
    ],
  ])('isOwnClanArmoryItem - %s', (_, item, userId, expectedResult) => {
    expect(isOwnClanArmoryItem(item, userId)).toEqual(expectedResult)
  })
})

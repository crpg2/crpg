import {
  getClans as _getClans,
  deleteClansByClanIdArmoryByUserItemId,
  deleteClansByClanIdMembersByUserId,
  getClansByClanIdArmory,
  getClansByClanIdInvitations,
  getClansById,
  getClansByIdMembers,
  postClans,
  postClansByClanIdArmory,
  postClansByClanIdInvitations,
  putClansByClanId,
  putClansByClanIdArmoryByUserItemIdBorrow,
  putClansByClanIdArmoryByUserItemIdReturn,
  putClansByClanIdInvitationsByInvitationIdResponse,
  putClansByClanIdMembersByUserId,
} from '#hey-api/sdk.gen'

import type {
  Clan,
  ClanArmoryItem,
  ClanInvitation,
  ClanInvitationStatus,
  ClanInvitationType,
  ClanMember,
  ClanUpdate,
  ClanWithMemberCount,
} from '~/models/clan'
import type { UserItem } from '~/models/user'

import { ClanMemberRole } from '~/models/clan'

export const getClans = async (): Promise<ClanWithMemberCount[]> => {
  const { data } = await _getClans({ composable: '$fetch' })
  return data!
}

export const createClan = (
  clan: ClanUpdate,
): Promise<Clan> => postClans({ composable: '$fetch', body: clan })

export const updateClan = (
  clanId: number,
  clan: ClanUpdate,
): Promise<Clan> => putClansByClanId({ composable: '$fetch', path: { clanId }, body: clan })

export const getClan = async (id: number): Promise<Clan> => {
  const { data } = await getClansById({ composable: '$fetch', path: { id } })
  return data
}

export const getClanMembers = async (id: number): Promise<ClanMember[]> => {
  const { data } = await getClansByIdMembers({ composable: '$fetch', path: { id } })
  return data!
}

export const updateClanMember = (
  clanId: number,
  memberId: number,
  role: ClanMemberRole,
) => putClansByClanIdMembersByUserId({ composable: '$fetch', path: { clanId, userId: memberId }, body: { role } })

export const kickClanMember = (
  clanId: number,
  memberId: number,
) => deleteClansByClanIdMembersByUserId({ composable: '$fetch', path: { clanId, userId: memberId } })

export const inviteToClan = (
  clanId: number,
  inviteeId: number,
) =>
  postClansByClanIdInvitations({ composable: '$fetch', path: { clanId }, body: { inviteeId } })

export const getClanInvitations = async (
  clanId: number,
  types: ClanInvitationType[],
  statuses: ClanInvitationStatus[],
): Promise<ClanInvitation[]> => {
  const { data } = await getClansByClanIdInvitations({ composable: '$fetch', path: { clanId }, query: { 'status[]': statuses, 'type[]': types } })
  return data!
}

export const respondToClanInvitation = (
  clanId: number,
  invitationId: number,
  accept: boolean,
) => putClansByClanIdInvitationsByInvitationIdResponse({ composable: '$fetch', path: { clanId, invitationId }, body: { accept } })

export const canManageApplicationsValidate = (role: ClanMemberRole) =>
  [ClanMemberRole.Leader, ClanMemberRole.Officer].includes(role)

export const canUpdateClanValidate = (role: ClanMemberRole) =>
  [ClanMemberRole.Leader].includes(role)

export const canUpdateMemberValidate = (role: ClanMemberRole) =>
  [ClanMemberRole.Leader].includes(role)

export const canKickMemberValidate = (
  selfMember: ClanMember,
  member: ClanMember,
  clanMembersCount: number,
) => {
  if (
    member.user.id === selfMember.user.id
    && (member.role !== ClanMemberRole.Leader || clanMembersCount === 1)
  ) {
    return true
  }

  return (
    (selfMember.role === ClanMemberRole.Leader
      && [ClanMemberRole.Officer, ClanMemberRole.Member].includes(member.role))
    || (selfMember.role === ClanMemberRole.Officer && member.role === ClanMemberRole.Member)
  )
}

export const getClanArmory = async (clanId: number): Promise<ClanArmoryItem[]> => {
  const { data } = await getClansByClanIdArmory({ composable: '$fetch', path: { clanId } })
  return data!
}

export const addItemToClanArmory = (
  clanId: number,
  userItemId: number,
) => postClansByClanIdArmory({ composable: '$fetch', path: { clanId }, body: { userItemId } })

export const removeItemFromClanArmory = (
  clanId: number,
  userItemId: number,
) => deleteClansByClanIdArmoryByUserItemId({ composable: '$fetch', path: { clanId, userItemId } })

export const borrowItemFromClanArmory = (
  clanId: number,
  userItemId: number,
) => putClansByClanIdArmoryByUserItemIdBorrow({ composable: '$fetch', path: { clanId, userItemId } })

export const returnItemToClanArmory = (
  clanId: number,
  userItemId: number,
) => putClansByClanIdArmoryByUserItemIdReturn({ composable: '$fetch', path: { clanId, userItemId } })

export const getClanArmoryItemBorrower = (
  borrowerUserId: number,
  clanMembers: ClanMember[],
) => clanMembers.find(cm => cm.user.id === borrowerUserId)?.user ?? null

export const getClanArmoryItemLender = (
  userId: number,
  clanMembers: ClanMember[],
) => clanMembers.find(cm => cm.user.id === userId)?.user ?? null

export const isOwnClanArmoryItem = (
  item: ClanArmoryItem,
  userId: number,
) => item.userItemId === userId

export const isClanArmoryItemInInventory = (
  item: ClanArmoryItem,
  userItems: UserItem[],
) => userItems.some(ui => ui.item.id === item.item.id)

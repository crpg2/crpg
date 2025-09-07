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
  ClanMemberRole,
  ClanUpdate,
  ClanWithMemberCount,
} from '~/models/clan'
import type { UserItem } from '~/models/user'

import { CLAN_MEMBER_ROLE } from '~/models/clan'

export const getClans = async (): Promise<ClanWithMemberCount[]> => (await _getClans({ })).data!

export const createClan = async (
  clan: ClanUpdate,
): Promise<Clan> => {
  const { data } = await postClans({
    // @ts-expect-error TODO: FIXME: fix hey-api - WithRefs
    body: clan,
  })
  return data
}

export const updateClan = (
  clanId: number,
  clan: ClanUpdate,
) => putClansByClanId({
  path: { clanId },
  // @ts-expect-error TODO:FIXME:
  body: clan,
})

export const getClan = async (id: number): Promise<Clan> => (await getClansById({ path: { id } })).data

export const getClanMembers = async (id: number): Promise<ClanMember[]> => (await getClansByIdMembers({ path: { id } })).data!

export const updateClanMember = (
  clanId: number,
  memberId: number,
  role: ClanMemberRole,
) => putClansByClanIdMembersByUserId({ path: { clanId, userId: memberId }, body: { role } })

export const kickClanMember = (
  clanId: number,
  memberId: number,
) => deleteClansByClanIdMembersByUserId({ path: { clanId, userId: memberId } })

export const inviteToClan = (
  clanId: number,
  inviteeId: number,
) => postClansByClanIdInvitations({ path: { clanId }, body: { inviteeId } })

export const getClanInvitations = async (
  clanId: number,
  types: ClanInvitationType[],
  statuses: ClanInvitationStatus[],
): Promise<ClanInvitation[]> => (await getClansByClanIdInvitations({ path: { clanId }, query: { 'status[]': statuses, 'type[]': types } })).data!

export const respondToClanInvitation = (
  clanId: number,
  invitationId: number,
  accept: boolean,
) => putClansByClanIdInvitationsByInvitationIdResponse({ path: { clanId, invitationId }, body: { accept } })

export const canManageApplicationsValidate = (role: ClanMemberRole) =>
  ([CLAN_MEMBER_ROLE.Leader, CLAN_MEMBER_ROLE.Officer] as ClanMemberRole[]).includes(role)

export const canUpdateClanValidate = (role: ClanMemberRole) =>
  ([CLAN_MEMBER_ROLE.Leader] as ClanMemberRole[]).includes(role)

export const canUpdateMemberValidate = (role: ClanMemberRole) =>
  ([CLAN_MEMBER_ROLE.Leader] as ClanMemberRole[]).includes(role)

export const canKickMemberValidate = (
  selfMember: ClanMember,
  member: ClanMember,
  clanMembersCount: number,
) => {
  if (
    member.user.id === selfMember.user.id
    && (member.role !== CLAN_MEMBER_ROLE.Leader || clanMembersCount === 1)
  ) {
    return true
  }

  return (
    (selfMember.role === CLAN_MEMBER_ROLE.Leader
      && ([CLAN_MEMBER_ROLE.Officer, CLAN_MEMBER_ROLE.Member] as ClanMemberRole[]).includes(member.role))
    || (selfMember.role === CLAN_MEMBER_ROLE.Officer && member.role === CLAN_MEMBER_ROLE.Member)
  )
}

export const getClanArmory = async (clanId: number): Promise<ClanArmoryItem[]> => (await getClansByClanIdArmory({ path: { clanId } })).data!

export const addItemToClanArmory = (
  clanId: number,
  userItemId: number,
) => postClansByClanIdArmory({ path: { clanId }, body: { userItemId } })

export const removeItemFromClanArmory = (
  clanId: number,
  userItemId: number,
) => deleteClansByClanIdArmoryByUserItemId({ path: { clanId, userItemId } })

export const borrowItemFromClanArmory = (
  clanId: number,
  userItemId: number,
) => putClansByClanIdArmoryByUserItemIdBorrow({ path: { clanId, userItemId } })

export const returnItemToClanArmory = (
  clanId: number,
  userItemId: number,
) => putClansByClanIdArmoryByUserItemIdReturn({ path: { clanId, userItemId } })

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
) => item.userId === userId

export const isClanArmoryItemInInventory = (
  item: ClanArmoryItem,
  userItems: UserItem[],
) => userItems.some(ui => ui.item.id === item.item.id)

import type {
  Clan,
  ClanArmoryItem,
  ClanEdition,
  ClanInvitation,
  ClanInvitationStatus,
  ClanInvitationType,
  ClanMember,
  ClanWithMemberCount,
} from '~/models/clan'
import type { Language } from '~/models/language'
import type { Region } from '~/models/region'
import type { UserItem } from '~/models/user'

import { ClanMemberRole } from '~/models/clan'
import { del, get, post, put } from '~/services/crpg-client'
import { argbIntToRgbHexColor, rgbHexColorToArgbInt } from '~/utils/color'

const mapClanRequest = (payload: Omit<Clan, 'id'>): Omit<ClanEdition, 'id'> => {
  return {
    ...payload,
    primaryColor: rgbHexColorToArgbInt(payload.primaryColor),
    secondaryColor: rgbHexColorToArgbInt(payload.secondaryColor),
  }
}

export const mapClanResponse = (payload: ClanEdition): Clan => {
  return {
    ...payload,
    primaryColor: argbIntToRgbHexColor(payload.primaryColor),
    secondaryColor: argbIntToRgbHexColor(payload.secondaryColor),
  }
}

// TODO: backend pagination/region query!
export const getClans = async () => {
  const clans = await get<ClanWithMemberCount<ClanEdition>[]>('/clans')
  return clans.map(c => ({
    ...c,
    clan: mapClanResponse(c.clan),
  }))
}

export const getFilteredClans = (
  clans: ClanWithMemberCount<Clan>[],
  region: Region,
  languages: Language[],
  search: string,
) => {
  const searchQuery = search.toLowerCase()
  return clans.filter(
    c =>
      c.clan.region === region
      && (languages.length ? languages.some(l => c.clan.languages.includes(l)) : true)
      && (c.clan.tag.toLowerCase().includes(searchQuery)
        || c.clan.name.toLowerCase().includes(searchQuery)),
  )
}

export const createClan = async (clan: Omit<Clan, 'id'>) =>
  mapClanResponse(await post<ClanEdition>('/clans', mapClanRequest(clan)))

export const updateClan = async (clanId: number, clan: Clan) =>
  mapClanResponse(await put<ClanEdition>(`/clans/${clanId}`, mapClanRequest(clan)))

export const getClan = async (id: number) =>
  mapClanResponse(await get<ClanEdition>(`/clans/${id}`))

export const getClanMembers = async (id: number) => get<ClanMember[]>(`/clans/${id}/members`)

export const updateClanMember = async (clanId: number, memberId: number, role: ClanMemberRole) =>
  put<ClanMember>(`/clans/${clanId}/members/${memberId}`, { role })

export const kickClanMember = async (clanId: number, memberId: number) =>
  del(`/clans/${clanId}/members/${memberId}`)

export const inviteToClan = async (clanId: number, inviteeId: number) =>
  post<ClanInvitation>(`/clans/${clanId}/invitations`, { inviteeId })

export const getClanInvitations = async (
  clanId: number,
  types: ClanInvitationType[],
  statuses: ClanInvitationStatus[],
) => {
  const params = new URLSearchParams()
  types.forEach(t => params.append('type[]', t))
  statuses.forEach(s => params.append('status[]', s))
  return get<ClanInvitation[]>(`/clans/${clanId}/invitations?${params}`)
}

export const respondToClanInvitation = async (
  clanId: number,
  clanInvitationId: number,
  accept: boolean,
) => put<ClanInvitation>(`/clans/${clanId}/invitations/${clanInvitationId}/response`, { accept })

// TODO: need a name
export const getClanMember = (clanMembers: ClanMember[], userId: number) =>
  clanMembers.find(m => m.user.id === userId) || null

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

export const getClanArmory = (clanId: number) => get<ClanArmoryItem[]>(`/clans/${clanId}/armory`)

export const addItemToClanArmory = (clanId: number, userItemId: number) =>
  post(`/clans/${clanId}/armory`, {
    userItemId,
  })

export const removeItemFromClanArmory = (clanId: number, userItemId: number) =>
  del(`/clans/${clanId}/armory/${userItemId}`)

export const borrowItemFromClanArmory = (clanId: number, userItemId: number) =>
  put(`/clans/${clanId}/armory/${userItemId}/borrow`)

export const returnItemToClanArmory = (clanId: number, userItemId: number) =>
  put(`/clans/${clanId}/armory/${userItemId}/return`)

export const getClanArmoryItemBorrower = (
  clanArmoryItem: ClanArmoryItem,
  clanMembers: ClanMember[],
) => {
  if (clanArmoryItem.borrowedItem === null) {
    return null
  }
  return (
    clanMembers.find(cm => cm.user.id === clanArmoryItem.borrowedItem!.borrowerUserId)?.user || null
  )
}

export const getClanArmoryItemLender = (userItem: UserItem, clanMembers: ClanMember[]) => {
  if (!userItem.isArmoryItem) {
    return null
  }
  return clanMembers.find(cm => cm.user.id === userItem.userId)?.user || null
}

export const isOwnClanArmoryItem = (item: ClanArmoryItem, userId: number) =>
  item.userItem.userId === userId

export const isClanArmoryItemInInventory = (item: ClanArmoryItem, userItems: UserItem[]) =>
  userItems.some(ui => ui.item.id === item.userItem.item.id)

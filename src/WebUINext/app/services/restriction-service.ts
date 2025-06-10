import {
  getRestrictions as _getRestrictions,
  getUsersByIdRestrictions,
  getUsersByUserId,
  getUsersSearch,
  postRestrictions,
  putUsersByUserIdNote,
} from '#hey-api/sdk.gen'

import type { Platform } from '~/models/platform'
import type {
  UserPrivate,
  UserPublic,
  UserRestriction,
  UserRestrictionCreation,
  UserRestrictionWithActive,
} from '~/models/user'

import { checkIsDateExpired } from '~/utils/date'

export const checkIsRestrictionActive = (
  restrictions: UserRestriction[],
  { createdAt, id, restrictedUser, type }: UserRestriction,
): boolean => {
  return !restrictions.some(
    r =>
      restrictedUser!.id === r.restrictedUser!.id // groupBy user - there may be restrisctions for other users on the list (/admin page)
      && type === r.type
      && id !== r.id
      && createdAt.getTime() < r.createdAt.getTime(), // check whether the the current restriction is NOT the newest
  )
}

export const mapRestrictions = (restrictions: UserRestriction[]): UserRestrictionWithActive[] => {
  return restrictions.map((r) => {
    const isExpired = checkIsDateExpired(r.createdAt, Number(r.duration))
    const isRestrictionActive = checkIsRestrictionActive(restrictions, r)

    return ({
      ...r,
      active: !isExpired && isRestrictionActive,
    })
  })
}

export const getRestrictions = async (): Promise<UserRestrictionWithActive[]> => {
  const { data } = await _getRestrictions({ composable: '$fetch' })
  return mapRestrictions(data!)
}

export const getUserRestrictions = async (id: number): Promise<UserRestrictionWithActive[]> => {
  const { data } = await getUsersByIdRestrictions({ composable: '$fetch', path: { id } })
  return mapRestrictions(data!)
}

export const restrictUser = (restriction: UserRestrictionCreation) => postRestrictions({ composable: '$fetch', body: restriction })

export const updateUserNote = (userId: number, user: { note: string }) =>
  putUsersByUserIdNote({ composable: '$fetch', path: { userId }, body: { note } })

export const getUserById = async (userId: number): Promise<UserPrivate> => {
  const { data } = await getUsersByUserId({ composable: '$fetch', path: { userId } })
  return data
}

interface UserSearchQuery {
  name?: string
  platform?: Platform
  platformUserId?: string
}

export const searchUser = async (query: UserSearchQuery): Promise<UserPublic[]> => {
  const { data } = await getUsersSearch({
    composable: '$fetch',
    query,
  })

  return data!
}

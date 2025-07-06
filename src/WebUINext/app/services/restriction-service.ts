import {
  getRestrictions as _getRestrictions,
  getUsersByIdRestrictions,
  getUsersByUserId,
  getUsersSearch,
  postRestrictions,
  putUsersByUserIdNote,
} from '#hey-api/sdk.gen'

import type { RestrictionViewModel } from '~/api'
import type { Platform } from '~/models/platform'
import type { UserPrivate, UserPublic, UserRestriction, UserRestrictionCreation } from '~/models/user'

import { USER_RESTRICTION_STATUS } from '~/models/user'
import { checkIsDateExpired } from '~/utils/date'

const checkIsRestrictionActive = (
  restrictions: RestrictionViewModel[],
  { createdAt, id, restrictedUser, type }: RestrictionViewModel,
): boolean => {
  return !restrictions.some(
    r =>
      restrictedUser!.id === r.restrictedUser!.id // groupBy user - there may be restrisctions for other users on the list (/admin page)
      && type === r.type
      && id !== r.id
      && createdAt.getTime() < r.createdAt.getTime(), // check whether the the current restriction is NOT the newest
  )
}

const mapRestrictions = (restrictions: RestrictionViewModel[]): UserRestriction[] => {
  return restrictions.map((r) => {
    const isExpired = checkIsDateExpired(r.createdAt, Number(r.duration))
    const isRestrictionActive = checkIsRestrictionActive(restrictions, r)
    return ({
      ...r,
      status: (!isExpired && isRestrictionActive) ? USER_RESTRICTION_STATUS.Active : USER_RESTRICTION_STATUS.NonActive,
    })
  })
}

export const getRestrictions = async (): Promise<UserRestriction[]> => {
  const { data } = await _getRestrictions({ composable: '$fetch' })
  return mapRestrictions(data!)
}

export const getUserRestrictions = async (id: number): Promise<UserRestriction[]> => {
  const { data } = await getUsersByIdRestrictions({ composable: '$fetch', path: { id } })
  return mapRestrictions(data!)
}

export const restrictUser = (restriction: UserRestrictionCreation) => postRestrictions({ composable: '$fetch', body: restriction })

export const updateUserNote = (userId: number, note: string) => putUsersByUserIdNote({ composable: '$fetch', path: { userId }, body: { note } })

export const getUserById = async (userId: number): Promise<UserPrivate> => (await getUsersByUserId({ composable: '$fetch', path: { userId } })).data

interface UserSearchQuery {
  name?: string
  platform?: Platform
  platformUserId?: string
}

export const searchUser = async (query: UserSearchQuery): Promise<UserPublic[]> => (await getUsersSearch({ composable: '$fetch', query })).data!

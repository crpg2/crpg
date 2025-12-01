import type { GetUsersSearchData, RestrictionViewModel } from '#api'

import {
  getRestrictions as _getRestrictions,
  getUsersByIdRestrictions,
  getUsersByUserId,
  getUsersSearch,
  postRestrictions,
  putUsersByUserIdNote,
} from '#api/sdk.gen'

import type { UserPrivate, UserPublic, UserRestriction, UserRestrictionCreation } from '~/models/user'

import { USER_RESTRICTION_STATUS } from '~/models/user'

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

const checkIsDateExpired = (createdAt: Date, duration: number) => new Date().getTime() > new Date(createdAt).getTime() + duration

const mapRestrictions = (restrictions: RestrictionViewModel[]): UserRestriction[] => {
  return restrictions.map((r) => {
    const isExpired = checkIsDateExpired(r.createdAt, Number(r.duration))
    const isRestrictionActive = checkIsRestrictionActive(restrictions, r)
    return ({
      ...r,
      // TODO: move to backend
      status: (!isExpired && isRestrictionActive) ? USER_RESTRICTION_STATUS.Active : USER_RESTRICTION_STATUS.NonActive,
    })
  })
}

export const getRestrictions = async (): Promise<UserRestriction[]> =>
  mapRestrictions((await _getRestrictions({ })).data!)

export const getUserRestrictions = async (id: number): Promise<UserRestriction[]> =>
  mapRestrictions((await getUsersByIdRestrictions({ path: { id } })).data!)

export const restrictUser = (restriction: UserRestrictionCreation) =>
  postRestrictions({ body: restriction })

export const updateUserNote = (userId: number, note: string) =>
  putUsersByUserIdNote({ path: { userId }, body: { note } })

export const getUserById = async (userId: number): Promise<UserPrivate> =>
  (await getUsersByUserId({ path: { userId } })).data

export const searchUser = async (query: GetUsersSearchData['query']): Promise<UserPublic[]> =>
  (await getUsersSearch({ query })).data!

import {
  deleteUsersSelf,
  deleteUsersSelfItemsById,
  deleteUsersSelfNotificationsById,
  deleteUsersSelfNotificationsDeleteAll,
  getUsersSelf,
  getUsersSelfItems,
  getUsersSelfNotifications,
  getUsersSelfRestriction,
  postUsersSelfItems,
  putUsersSelfItemsByIdReforge,
  putUsersSelfItemsByIdRepair,
  putUsersSelfItemsByIdUpgrade,
  putUsersSelfNotificationsById,
  putUsersSelfNotificationsReadAll,
} from '#api/sdk.gen'
import { pick } from 'es-toolkit'

import type {
  User,
  UserItem,
  UserPublic,
  UserRestrictionPublic,
} from '~/models/user'

export const getUser = async (): Promise<User> => (await getUsersSelf({ })).data

export const mapUserToUserPublic = (user: User): UserPublic =>
  pick(user, ['id', 'platform', 'platformUserId', 'name', 'region', 'avatar', 'clanMembership'])

export const deleteUser = () => deleteUsersSelf({})

export const getUserItems = async (): Promise<UserItem[]> => (await getUsersSelfItems({})).data!

export const buyUserItem = (itemId: string) => postUsersSelfItems({ body: { itemId } })

export const sellUserItem = (userItemId: number) => deleteUsersSelfItemsById({ path: { id: userItemId } })

export const repairUserItem = (userItemId: number) => putUsersSelfItemsByIdRepair({ path: { id: userItemId } })

export const upgradeUserItem = (userItemId: number) => putUsersSelfItemsByIdUpgrade({ path: { id: userItemId } })

export const reforgeUserItem = (userItemId: number) => putUsersSelfItemsByIdReforge({ path: { id: userItemId } })

export const getUserRestriction = async (): Promise<UserRestrictionPublic> => (await getUsersSelfRestriction({ })).data

export const getUserNotifications = async () => (await getUsersSelfNotifications({ })).data

export const readUserNotification = (id: number) => putUsersSelfNotificationsById({ path: { id } })

export const readAllUserNotifications = () => putUsersSelfNotificationsReadAll({})

export const deleteUserNotification = (id: number) => deleteUsersSelfNotificationsById({ path: { id } })

export const deleteAllUserNotifications = () => deleteUsersSelfNotificationsDeleteAll({})

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
} from '#hey-api/sdk.gen'
import { pick } from 'es-toolkit'

import type { MetadataDict } from '~/models/metadata'
import type {
  User,
  UserItem,
  UserItemsByType,
  UserNotification,
  UserPublic,
  UserRestrictionPublic,
} from '~/models/user'

export const getUser = async (): Promise<User> => {
  const { data } = await getUsersSelf({ composable: '$fetch' })
  return data
}

export const mapUserToUserPublic = (user: User): UserPublic =>
  pick(user, ['id', 'platform', 'platformUserId', 'name', 'region', 'avatar', 'clanMembership'])

export const deleteUser = () => deleteUsersSelf({ composable: '$fetch' })

export const getUserItems = async (): Promise<UserItem[]> => {
  const { data } = await getUsersSelfItems({ composable: '$fetch' })
  return data!
}

export const buyUserItem = (itemId: string) =>
  postUsersSelfItems({ composable: '$fetch', body: { itemId } })

export const sellUserItem = (userItemId: number) =>
  deleteUsersSelfItemsById({ composable: '$fetch', path: { id: userItemId } })

export const repairUserItem = (userItemId: number) =>
  putUsersSelfItemsByIdRepair({ composable: '$fetch', path: { id: userItemId } })

export const upgradeUserItem = (userItemId: number) =>
  putUsersSelfItemsByIdUpgrade({ composable: '$fetch', path: { id: userItemId } })

export const reforgeUserItem = (userItemId: number) =>
  putUsersSelfItemsByIdReforge({ composable: '$fetch', path: { id: userItemId } })

export const groupUserItemsByType = (items: UserItem[]) =>
  items
    .reduce((itemsGroup, ui) => {
      const type = ui.item.type
      const currentGroup = itemsGroup.find(item => item.type === type)

      if (currentGroup) {
        currentGroup.items.push(ui)
      }
      else {
        itemsGroup.push({
          items: [ui],
          type,
        })
      }

      return itemsGroup
    }, [] as UserItemsByType[])
    .sort((a, b) => a.type.localeCompare(b.type))

export const getUserRestriction = async (): Promise<UserRestrictionPublic> => {
  const { data } = await getUsersSelfRestriction({ composable: '$fetch' })
  return data
}

interface GetUSerNotificationResponse {
  notifications: UserNotification[]
  dict: MetadataDict
}

export const getUserNotifications = async (): Promise<GetUSerNotificationResponse> => {
  const { data } = await getUsersSelfNotifications({ composable: '$fetch' })
  return data!
}

export const readUserNotification = (id: number) =>
  putUsersSelfNotificationsById({ composable: '$fetch', path: { id } })

export const readAllUserNotifications = () =>
  putUsersSelfNotificationsReadAll({ composable: '$fetch' })

export const deleteUserNotification = (id: number) =>
  deleteUsersSelfNotificationsById({ composable: '$fetch', path: { id } })

export const deleteAllUserNotifications = () =>
  deleteUsersSelfNotificationsDeleteAll({ composable: '$fetch' })

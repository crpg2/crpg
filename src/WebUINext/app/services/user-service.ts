// import qs from 'qs'
import { getUsersSelf } from '#hey-api/sdk.gen'
import { pick } from 'es-toolkit'

// import type { Item } from '~/models/item'
import type { MetadataDict } from '~/models/metadata'
// import type { Platform } from '~/models/platform'
// import type { PublicRestriction, RestrictionWithActive } from '~/models/restriction'
import type {
  User,
  UserItem,
  UserItemsByType,
  UserNotification,
  UserPrivate,
  UserPublic,
} from '~/models/user'

// import { del, get, post, put } from '~/services/crpg-client'
// import { mapRestrictions } from '~/services/restriction-service'

export const getUser = async (): Promise<User> => {
  const { data } = await getUsersSelf({ composable: '$fetch' })
  return data as unknown as User // TODO: FIXME:
}

// export const deleteUser = () => del('/users/self')

// export const getUserById = (id: number) => get<UserPrivate>(`/users/${id}`)

// export const updateUserNote = (id: number, user: { note: string }) =>
//   put<UserPrivate>(`/users/${id}/note`, user)

// interface UserSearchQuery {
//   name?: string
//   platform?: Platform
//   platformUserId?: string
// }

// export const searchUser = (payload: UserSearchQuery) =>
//   get<UserPublic[]>(`/users/search/?${qs.stringify(payload)}`)

// export const extractItemFromUserItem = (items: UserItem[]): Item[] => items.map(ui => ui.item)

// export const getUserItems = () => get<UserItem[]>('/users/self/items')

// export const buyUserItem = (itemId: string) => post<UserItem>('/users/self/items', { itemId })

// export const repairUserItem = (userItemId: number) =>
//   put<UserItem>(`/users/self/items/${userItemId}/repair`)

// export const upgradeUserItem = (userItemId: number) =>
//   put<UserItem>(`/users/self/items/${userItemId}/upgrade`)

// export const reforgeUserItem = (userItemId: number) =>
//   put<UserItem>(`/users/self/items/${userItemId}/reforge`)

// export const rewardUser = (
//   userId: number,
//   payload: { gold: number, heirloomPoints: number, itemId: string },
// ) => put(`/users/${userId}/rewards`, payload)

// export const sellUserItem = (userItemId: number) => del(`/users/self/items/${userItemId}`)

// export const groupUserItemsByType = (items: UserItem[]) =>
//   items
//     .reduce((itemsGroup, ui) => {
//       const type = ui.item.type
//       const currentGroup = itemsGroup.find(item => item.type === type)

//       if (currentGroup) {
//         currentGroup.items.push(ui)
//       }
//       else {
//         itemsGroup.push({
//           items: [ui],
//           type,
//         })
//       }

//       return itemsGroup
//     }, [] as UserItemsByType[])
//     .sort((a, b) => a.type.localeCompare(b.type))

// export const getUserRestriction = () => get<PublicRestriction>('/users/self/restriction')

// export const getUserRestrictions = async (id: number) =>
//   mapRestrictions(await get<RestrictionWithActive[]>(`/users/${id}/restrictions`))

export const mapUserToUserPublic = (user: User): UserPublic => pick(user, [
  'id',
  'platform',
  'platformUserId',
  'name',
  'region',
  'avatar',
  'clanMembership',
])

export const getUserNotifications = async (): Promise<{ notifications: UserNotification[], dict: MetadataDict }> => {
  const { data } = await getUsersSelfNotifications({ composable: '$fetch' })
  return data!
}

export const readUserNotification = (id: number) => putUsersSelfNotificationsById({ composable: '$fetch', path: { id } })

export const readAllUserNotifications = () => putUsersSelfNotificationsReadAll({ composable: '$fetch' })

export const deleteUserNotification = (id: number) => deleteUsersSelfNotificationsById({ composable: '$fetch', path: { id } })

export const deleteAllUserNotifications = () => deleteUsersSelfNotificationsDeleteAll({ composable: '$fetch' })

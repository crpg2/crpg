import type { UserItem } from '~/models/user'

import { USER_QUERY_KEYS } from '~/queries'
import { getUserItems } from '~/services/user-service'

export const useUserItemsProvider = () => {
  return useAsyncDataCustom(
    () => USER_QUERY_KEYS.items(),
    () => getUserItems(),
    {
      default: () => [],
    },
  )
}

export const useUserItems = () => {
  const _key = USER_QUERY_KEYS.items()
  const userItems = getAsyncData<UserItem[]>(_key)
  const refreshUserItems = refreshAsyncData(_key)

  return {
    userItems,
    refreshUserItems,
  }
}

import type { UserRestrictionPublic } from '~/models/user'

import { useState } from '#imports'
import { getUserRestriction } from '~/services/user-service'

export function useUserRestriction() {
  const restriction = useState<UserRestrictionPublic | null>('user-restriction', () => null)

  async function fetchUserRestriction() {
    restriction.value = await getUserRestriction()
  }

  return {
    restriction,
    fetchUserRestriction,
  }
}

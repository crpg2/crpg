import type { UserRestrictionPublic } from '~/models/user'

import { getUserRestriction } from '~/services/user-service'

export function useUserRestriction() {
  const restriction = useState<UserRestrictionPublic | null>('user-restriction', () => null)

  async function fetchUserRestriction() {
    const data = await getUserRestriction()
    restriction.value = data
  }

  return {
    restriction,
    fetchUserRestriction,
  }
}

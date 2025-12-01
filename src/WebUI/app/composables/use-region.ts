import type { Region } from '~/models/region'

import { useUser } from '~/composables/user/use-user'
import { REGION } from '~/models/region'

export const useRegionQuery = () => {
  const { user } = useUser()
  const regionModel = useRouteQuery<Region>('region', user.value?.region || REGION.Eu)

  const regions = Object.values(REGION)

  return {
    regionModel,
    regions,
  }
}

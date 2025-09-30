import type { Region } from '~/models/region'

import { REGION } from '~/models/region'
import { useUserStore } from '~/stores/user'

export const useRegionQuery = () => {
  const regionModel = useRouteQuery<Region>('region', useUserStore().user?.region || REGION.Eu)

  const regions = Object.values(REGION)

  return {
    regionModel,
    regions,
  }
}

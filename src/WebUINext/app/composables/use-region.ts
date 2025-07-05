import type { Region } from '~/models/region'

import { REGION } from '~/models/region'
import { useUserStore } from '~/stores/user'

export const useRegionQuery = () => {
  const route = useRoute()
  const router = useRouter()
  const { user } = toRefs(useUserStore())

  const regionModel = computed({
    get() {
      return (route.query?.region as Region) || user.value?.region || REGION.Eu
    },

    set(region: Region) {
      router.replace({
        query: {
          ...route.query,
          region,
        },
      })
    },
  })

  const regions = Object.values(REGION)

  return {
    regionModel,
    regions,
  }
}

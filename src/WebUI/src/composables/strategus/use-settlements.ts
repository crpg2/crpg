import type { LMap } from '@vue-leaflet/vue-leaflet'
import type { Position } from 'geojson'
import type { LatLngBounds, Map } from 'leaflet'

import { getSettlements } from '~/services/strategus-service'
import { shouldDisplaySettlement } from '~/services/strategus-service/map'
import { positionToLatLng } from '~/utils/geometry'

export const useSettlements = (
  map: Ref<typeof LMap | null>,
  mapBounds: Ref<LatLngBounds | null>,
  zoom: Ref<number>,
) => {
  const { execute: loadSettlements, state: settlements } = useAsyncState(
    () => getSettlements(),
    [],
    {
      immediate: false,
    },
  )

  const visibleSettlements = computed(() => {
    if (mapBounds.value === null) {
      return []
    }

    return settlements.value.filter(s => shouldDisplaySettlement(s, mapBounds.value!, zoom.value))
  })

  const shownSearch = ref<boolean>(false)
  const toggleSearch = () => {
    shownSearch.value = !shownSearch.value
  }

  const flyToSettlement = (coordinates: Position) => {
    (map.value!.leafletObject as Map)!.flyTo(positionToLatLng(coordinates), 5, {
      animate: false,
    })
  }

  return {
    loadSettlements,
    settlements,
    visibleSettlements,

    //
    flyToSettlement,
    shownSearch,

    toggleSearch,
  }
}

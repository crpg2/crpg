import type { LMap } from '@vue-leaflet/vue-leaflet'
import type { Position } from 'geojson'
import type { LatLngBounds, Map } from 'leaflet'

import { toValue } from 'vue'

import { shouldDisplaySettlement } from '~/services/strategus/map-service'
import { getSettlement, getSettlements } from '~/services/strategus/settlement-service'
import { positionToLatLng } from '~/utils/geometry'

export const useSettlements = (
  map: Ref<typeof LMap | null>,
  mapBounds: MaybeRefOrGetter<LatLngBounds | null>,
  zoom: MaybeRefOrGetter<number>,
) => {
  const { state: settlements, execute: loadSettlements } = useAsyncState(
    () => getSettlements(),
    [],
  )

  const visibleSettlements = computed(() => {
    if (toValue(mapBounds) === null) {
      return []
    }

    return settlements.value.filter(settlement => shouldDisplaySettlement(settlement, toValue(mapBounds)!, toValue(zoom)))
  })

  const shownSearch = ref<boolean>(false)
  const toggleSearch = () => {
    shownSearch.value = !shownSearch.value
  }

  const flyToSettlement = (coordinates: Position) => {
    (map.value!.leafletObject as Map).flyTo(positionToLatLng(coordinates), 5, {
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

export const useSettlement = () => {
  const { state: settlement, execute: loadSettlement, isLoading: loadingSettlement } = useAsyncState(
    () => getSettlement(1), // TODO: FIXME:
    null,
  )

  return {
    settlement,
    loadSettlement,
    loadingSettlement,
  }
}

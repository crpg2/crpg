import type { LMap } from '@vue-leaflet/vue-leaflet'
import type { Position } from 'geojson'
import type { LatLngBounds, Map } from 'leaflet'

import { toValue } from 'vue'

import type { SettlementPublic } from '~/models/strategus/settlement'

import { SETTLEMENT_QUERY_KEYS } from '~/queries'
import { shouldDisplaySettlement } from '~/services/strategus/map-service'
import { updateSettlementResources as _updateSettlementResources, getSettlement, getSettlementItems, getSettlements } from '~/services/strategus/settlement-service'
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

export const useSettlementProvider = (settlementId: number) => {
  return useAsyncData(
    toCacheKey(SETTLEMENT_QUERY_KEYS.byId(settlementId)),
    () => getSettlement(settlementId),
    {
      default: () => [],
    },
  )
}

export const useSettlement = () => {
  const route = useRoute('strategus-settlement-id')
  const _key = SETTLEMENT_QUERY_KEYS.byId(Number(route.params.id))

  const settlement = getAsyncData<SettlementPublic>(_key)
  const refreshSettlement = refreshAsyncData(_key)

  const updateSettlementResources = (troops: number) => _updateSettlementResources(settlement.value.id, troops)

  return {
    settlement,
    refreshSettlement,
    updateSettlementResources,
  }
}

export const useSettlementItems = () => {
  const { settlement } = useSettlement()

  const {
    data: settlementItems,
  } = useAsyncData(
    toCacheKey(SETTLEMENT_QUERY_KEYS.items(settlement.value.id)),
    () => getSettlementItems(settlement.value.id),
    {
      default: () => [],
    },
  )

  return {
    settlementItems,
  }
}

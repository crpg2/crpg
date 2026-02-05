import type { LMap } from '@vue-leaflet/vue-leaflet'
import type { Map, PointExpression } from 'leaflet'

import { strategusMapHeight, strategusMapWidth } from '~root/data/constants.json'
import { CRS, LatLngBounds } from 'leaflet'

// TODO:
export const useMap = () => {
  const map = useTemplateRef<typeof LMap>('map')

  const mapOptions = {
    crs: CRS.Simple,
    inertiaDeceleration: 2000,
    maxBoundsViscosity: 0.8,
    maxZoom: 7,
    minZoom: 3,
    zoomControl: false,
    zoomSnap: 0.5,
  }

  const tileLayerOptions = {
    attribution:
      '<a target="_blank" href="https://www.taleworlds.com">TaleWorlds Entertainment</a>',
    url: 'http://pecores.fr/gigamap/{z}/{y}/{x}.webp',
  }

  // TODO: from cfg
  const center = ref<PointExpression>([-100, 125])

  const mapBounds = ref<LatLngBounds | null>(null)

  const maxBounds = new LatLngBounds([
    [0, 0],
    [-strategusMapHeight, strategusMapWidth],
  ])

  const onMapMoveEnd = () => {
    if (!map.value) {
      return
    }
    mapBounds.value = (map.value.leafletObject as Map).getBounds()
  }

  const zoom = ref<number>(mapOptions.minZoom)

  const zoomIn = () => (map.value!.leafletObject as Map).zoomIn()
  const zoomOut = () => (map.value!.leafletObject as Map).zoomOut()

  return {
    center,
    map,
    mapBounds,
    mapOptions,
    maxBounds,
    onMapMoveEnd,
    zoom,
    zoomIn,
    zoomOut,
    //
    tileLayerOptions,
    //
  }
}

interface MapContext {
  zoom: Ref<number>
}

const mapContextKey: InjectionKey<MapContext> = Symbol('StrategusMap')

export const useMapContextProvider = (ctx: MapContext) => {
  provide(mapContextKey, ctx)
}

export const useMapContext = () => {
  const ctx = injectStrict(mapContextKey)
  return ctx
}

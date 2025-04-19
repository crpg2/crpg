import type { LMap } from '@vue-leaflet/vue-leaflet'
import type { Map, PointExpression } from 'leaflet'

import { strategusMapHeight, strategusMapWidth } from '~root/data/constants.json'
import { CRS, LatLngBounds } from 'leaflet'

export const useMap = () => {
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

  const center = ref<PointExpression>([-100, 125])

  const mapBounds = ref<LatLngBounds | null>(null)

  const maxBounds = new LatLngBounds([
    [0, 0],
    [-strategusMapHeight, strategusMapWidth],
  ])

  const onMapMoveEnd = () => {
    if (!map.value) { return }
    mapBounds.value = (map.value.leafletObject as Map).getBounds()
  }

  const zoom = ref<number>(mapOptions.minZoom)

  const map = useTemplateRef<typeof LMap>('map')

  return {
    center,
    map,
    mapBounds,
    mapOptions,
    maxBounds,
    onMapMoveEnd,
    zoom,
    //
    tileLayerOptions,
    //
  }
}

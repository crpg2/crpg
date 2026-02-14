import type { LMap } from '@vue-leaflet/vue-leaflet'
import type { LeafletMouseEvent, Map } from 'leaflet'
import type L from 'leaflet'

import { useParty } from '~/composables/strategus/use-party'
import { PARTY_ORDER_TYPE } from '~/models/strategus/party'

export const usePartyMove = (map: Ref<typeof LMap | null>) => {
  const { setPartyOrder, validateCanMove } = useParty()

  const isMoveMode = ref<boolean>(false)

  const onCreateMovePath = async (event: { shape: string, layer: L.Layer }) => {
    const { layer, shape } = event

    if (shape !== 'Line') {
      return
    }

    // @ts-expect-error TODO:
    const coordinates = layer.toGeoJSON().geometry.coordinates
    await setPartyOrder({
      type: PARTY_ORDER_TYPE.MoveToPoint,
      waypoints: { coordinates, type: 'MultiPoint' },
    })

    event.layer.removeFrom(map.value!.leafletObject as Map)
    isMoveMode.value = false
  }

  const onStartMove = (e: LeafletMouseEvent) => {
    if (!validateCanMove()) {
      return
    }

    const leafletObject = map.value!.leafletObject as Map

    // leafletObject.eachLayer((layer) => {
    // if (
    //   // layer instanceof L.Circle
    //   // @ts-expect-error custom option
    //   // && layer.options?.settlementZone === true
    // ) {
    // // zones.push(layer)
    // }
    // console.log(layer)
    // })

    isMoveMode.value = true
    leafletObject.pm.enableDraw('Line', {})
    // @ts-expect-error TODO: FIXME:
    leafletObject.pm.Draw.Line?._layer.addLatLng(e.latlng)
    // @ts-expect-error TODO: FIXME:
    leafletObject.pm.Draw.Line?._createMarker(e.latlng)
  }

  const applyMoveEvents = () => {
    const leafletObject = map.value!.leafletObject as Map

    leafletObject.on('pm:keyevent', (e) => {
      if (isMoveMode.value && (e.event as KeyboardEvent).code === 'Escape') {
        leafletObject.pm.disableDraw()
        isMoveMode.value = false
      }
    })

    leafletObject.on('pm:create', onCreateMovePath)
  }

  return {
    applyMoveEvents,
    isMoveMode,
    onStartMove,
  }
}

import type { LMap } from '@vue-leaflet/vue-leaflet'
import type L from 'leaflet'
import type { LatLngLiteral, LeafletMouseEvent, Map } from 'leaflet'

import type { MovementTargetType, MovementType } from '~/models/strategus'
import type { PartyCommon } from '~/models/strategus/party'
import type { SettlementPublic } from '~/models/strategus/settlement'

import { useParty } from '~/composables/strategus/use-party'
import { PartyStatus } from '~/models/strategus/party'
import { positionToLatLng } from '~/utils/geometry'

export const useMove = (map: Ref<typeof LMap | null>) => {
  const { moveParty } = useParty()

  const moveDialogCoordinates = ref<LatLngLiteral | null>(null)
  const moveDialogMovementTypes = ref<MovementType[]>([])

  const moveTargetType = ref<MovementTargetType | null>(null)
  const moveTarget = ref<PartyCommon | SettlementPublic | null>(null)

  const showMoveDialog = ({
    movementTypes,
    target,
    targetType,
  }: {
    target: PartyCommon | SettlementPublic
    targetType: MovementTargetType
    movementTypes: MovementType[]
  }) => {
    moveTarget.value = target
    moveTargetType.value = targetType

    moveDialogCoordinates.value = positionToLatLng(target.position.coordinates)
    moveDialogMovementTypes.value = movementTypes
  }

  const closeMoveDialog = () => {
    moveDialogCoordinates.value = null
    moveDialogMovementTypes.value = []
    moveTarget.value = null
    moveTargetType.value = null
  }

  const isMoveMode = ref<boolean>(false)

  const onCreateMovePath = async (event: { shape: string, layer: L.Layer }) => {
    const { layer, shape } = event

    if (shape === 'Line') {
      // @ts-expect-error TODO:
      const coordinates = layer.toGeoJSON().geometry.coordinates

      await moveParty({
        status: PartyStatus.MovingToPoint,
        waypoints: { coordinates, type: 'MultiPoint' },
      })

      event.layer.removeFrom(map.value!.leafletObject as Map)
      isMoveMode.value = false
    }
  }

  const onStartMove = (e: LeafletMouseEvent) => {
    if (!map.value) { return }
    const leafletObject = map.value.leafletObject as Map

    isMoveMode.value = true
    leafletObject.pm.enableDraw('Line', {})
    // @ts-expect-error TODO: FIXME:
    leafletObject.pm.Draw.Line._layer.addLatLng(e.latlng)
    // @ts-expect-error TODO: FIXME:
    leafletObject.pm.Draw.Line._createMarker(e.latlng)
  }

  const applyEvents = () => {
    if (!map.value) { return }
    const leafletObject = map.value.leafletObject as Map

    leafletObject.on('pm:keyevent', (e) => {
      if (isMoveMode.value && (e.event as KeyboardEvent).code === 'Escape') {
        leafletObject.pm.disableDraw()
        isMoveMode.value = false
      }
    })

    leafletObject.on('pm:create', onCreateMovePath)
  }

  return {
    applyEvents,

    isMoveMode,
    moveTarget,

    moveTargetType,
    onStartMove,
    //
    moveDialogCoordinates,
    moveDialogMovementTypes,
    //
    closeMoveDialog,
    showMoveDialog,
  }
}

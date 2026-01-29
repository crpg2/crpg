import type { LMap } from '@vue-leaflet/vue-leaflet'
import type { LatLngLiteral, LeafletMouseEvent, Map } from 'leaflet'
import type L from 'leaflet'

import { useMagicKeys } from '@vueuse/core'

import type { MapBattle } from '~/models/strategus/battle'
import type { MovementTargetType, MovementType } from '~/models/strategus/movement'
import type { PartyVisible } from '~/models/strategus/party'
import type { SettlementPublic } from '~/models/strategus/settlement'

import { useParty } from '~/composables/strategus/use-party'
import { BATTLE_SIDE } from '~/models/strategus/battle'
import { MOVEMENT_TARGET_TYPE, MOVEMENT_TYPE } from '~/models/strategus/movement'
import { PARTY_ORDER_TYPE, PARTY_STATUS } from '~/models/strategus/party'
import { UNMOVABLE_PARTY_STATUSES } from '~/services/strategus/party-service'
import { positionToLatLng } from '~/utils/geometry'

export const usePartyMove = (map: Ref<typeof LMap | null>) => {
  const { moveParty, partyState } = useParty()
  const toast = useToast()

  const moveDialogCoordinates = ref<LatLngLiteral | null>(null)
  const moveDialogMovementTypes = ref<MovementType[]>([])

  const moveTargetType = ref<MovementTargetType | null>(null)
  const moveTarget = ref<PartyVisible | SettlementPublic | MapBattle | null>(null)

  const validateCanMove = () => {
    if (UNMOVABLE_PARTY_STATUSES.includes(partyState.value.party.status)) {
      toast.add({
        description: 'Вы не можете двигаться TODO:',
        color: 'warning',
      })
      return false
    }

    return true
  }

  const showMoveDialog = ({
    target,
    movementTypes,
    targetType,
  }: {
    target: PartyVisible | SettlementPublic | MapBattle
    targetType: MovementTargetType
    movementTypes: MovementType[]
  }) => {
    if (!validateCanMove()) {
      return
    }

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

  const { shift } = useMagicKeys()

  const onMoveDialogConfirm = (mt: MovementType) => {
    if (!moveTarget.value || !moveTargetType.value) {
      return
    }

    switch (moveTargetType.value) {
      case MOVEMENT_TARGET_TYPE.Party:
        moveParty({
          type:
            mt === MOVEMENT_TYPE.Follow
              ? PARTY_ORDER_TYPE.FollowParty
              : PARTY_ORDER_TYPE.AttackParty,
          targetedPartyId: moveTarget.value.id,
        }, shift?.value)
        break
      case MOVEMENT_TARGET_TYPE.Settlement:
        moveParty({
          type:
            mt === MOVEMENT_TYPE.Move
              ? PARTY_ORDER_TYPE.MoveToSettlement
              : PARTY_ORDER_TYPE.AttackSettlement,
          targetedSettlementId: moveTarget.value.id,
        }, shift?.value)
        break
      case MOVEMENT_TARGET_TYPE.Battle:
        moveParty({
          type: PARTY_ORDER_TYPE.JoinBattle,
          targetedBattleId: moveTarget.value.id,
          battleJoinIntents: [
            ...(mt === MOVEMENT_TYPE.JoinToBattleForAttacker || mt === MOVEMENT_TYPE.JoinToBattleForBoth)
              ? [
                  {
                    side: BATTLE_SIDE.Attacker,
                    battleId: moveTarget.value.id,
                  },
                ]
              : [],
            ...(mt === MOVEMENT_TYPE.JoinToBattleForDefender || mt === MOVEMENT_TYPE.JoinToBattleForBoth)
              ? [
                  {
                    side: BATTLE_SIDE.Defender,
                    battleId: moveTarget.value.id,
                  },
                ]
              : [],
          ],
        }, shift?.value)
        break
    }

    closeMoveDialog()
  }

  const isMoveMode = ref<boolean>(false)

  const onCreateMovePath = async (event: { shape: string, layer: L.Layer }) => {
    const { layer, shape } = event

    if (shape !== 'Line') {
      return
    }

    // @ts-expect-error TODO:
    const coordinates = layer.toGeoJSON().geometry.coordinates

    await moveParty({
      type: PARTY_ORDER_TYPE.MoveToPoint,
      waypoints: { coordinates, type: 'MultiPoint' },
    }, shift?.value)

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

  const applyEvents = () => {
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

    onMoveDialogConfirm,
  }
}

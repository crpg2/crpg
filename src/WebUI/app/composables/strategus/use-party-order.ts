import { useMagicKeys } from '@vueuse/core'
import { LazyMapPartyTransferOfferCreateDrawer } from '#components'

import type { MapBattle } from '~/models/strategus/battle'
import type { PartyOrderType, PartyVisible } from '~/models/strategus/party'
import type { SettlementPublic } from '~/models/strategus/settlement'

import { useParty } from '~/composables/strategus/use-party'
import { BATTLE_SIDE } from '~/models/strategus/battle'
import { PARTY_ORDER_TYPE } from '~/models/strategus/party'
import { positionToLatLng } from '~/utils/geometry'

export type OrderTarget = PartyVisible | SettlementPublic | MapBattle

export const usePartyOrder = () => {
  const { setPartyOrder, validateCanMove } = useParty()

  const orderTarget = ref<OrderTarget | null>(null)
  const orderTargetCoordinates = computed(() => orderTarget.value ? positionToLatLng(orderTarget.value?.position.coordinates) : null)
  const availableOrders = ref<PartyOrderType[]>([])

  const openOrderDialog = (target: OrderTarget, orders: PartyOrderType[]) => {
    if (!validateCanMove()) {
      return
    }

    orderTarget.value = target
    availableOrders.value = orders
  }

  const closeOrderDialog = () => {
    orderTarget.value = null
    availableOrders.value = []
  }

  const { shift } = useMagicKeys() // TODO: временно, придумать норм UX
  const overlay = useOverlay()

  const confirmOrderDialog = async (order: PartyOrderType) => {
    if (!orderTarget.value) {
      return
    }

    switch (order) {
      case PARTY_ORDER_TYPE.AttackParty:
      case PARTY_ORDER_TYPE.FollowParty:
        setPartyOrder({ type: order, targetedPartyId: orderTarget.value.id }, shift?.value)
        break

      case PARTY_ORDER_TYPE.TransferOfferParty:
        await overlay
          .create(LazyMapPartyTransferOfferCreateDrawer)
          .open({
            targetParty: orderTarget.value as PartyVisible,
            onClose(_result, transferOffer) {
              if (!_result || !transferOffer) {
                return
              }
              setPartyOrder({
                type: order,
                targetedPartyId: orderTarget.value!.id,
                transferOfferPartyIntent: transferOffer,
              }, shift?.value)
            },
          })
        break

      case PARTY_ORDER_TYPE.MoveToSettlement:
      case PARTY_ORDER_TYPE.AttackSettlement:
        setPartyOrder({ type: order, targetedSettlementId: orderTarget.value.id }, shift?.value)
        break

      case PARTY_ORDER_TYPE.JoinBattle:
        // TODO: UI для выбора стороны!
        setPartyOrder({
          type: order,
          targetedBattleId: orderTarget.value.id,
          battleJoinIntents: [
            { side: BATTLE_SIDE.Attacker },
            { side: BATTLE_SIDE.Defender },
          ],
        }, shift?.value)
        break

      default:
        break
    }

    closeOrderDialog()
  }

  const openPartyOrderDialog = (target: PartyVisible) => openOrderDialog(target, [
    PARTY_ORDER_TYPE.FollowParty,
    PARTY_ORDER_TYPE.AttackParty,
    PARTY_ORDER_TYPE.TransferOfferParty,
  ])

  const openSettlementOrderDialog = (target: SettlementPublic) => openOrderDialog(target, [
    PARTY_ORDER_TYPE.MoveToSettlement,
    PARTY_ORDER_TYPE.AttackSettlement,
  ])

  const openBattleOrderDialog = (target: MapBattle) => openOrderDialog(target, [
    PARTY_ORDER_TYPE.JoinBattle,
  ])

  return {
    orderTargetCoordinates,
    openOrderDialog,
    closeOrderDialog,
    confirmOrderDialog,
    availableOrders,
    //
    openPartyOrderDialog,
    openSettlementOrderDialog,
    openBattleOrderDialog,
  }
}

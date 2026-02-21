<script setup lang="ts">
import type { TimelineItem } from '@nuxt/ui'

import type { Party, PartyOrder, PartyOrderType, TransferOfferParty } from '~/models/strategus/party'

import { PARTY_ORDER_TYPE } from '~/models/strategus/party'

const { party } = defineProps<{ party: Party }>()

defineEmits<{
  locate: []
  openTransferOffer: [offer: TransferOfferParty]
}>()

const { t } = useI18n()

function getOrderIcon(orderType: PartyOrderType): string {
  return ({
    [PARTY_ORDER_TYPE.MoveToPoint]: 'crpg:boot-prints',
    [PARTY_ORDER_TYPE.FollowParty]: 'crpg:boot-prints',
    [PARTY_ORDER_TYPE.AttackParty]: 'crpg:game-mode-duel',
    [PARTY_ORDER_TYPE.MoveToSettlement]: 'crpg:boot-prints',
    [PARTY_ORDER_TYPE.AttackSettlement]: 'crpg:game-mode-conquest',
    [PARTY_ORDER_TYPE.JoinBattle]: 'crpg:game-mode-duel',
    [PARTY_ORDER_TYPE.TransferOfferParty]: 'crpg:chest',
  } satisfies Record<PartyOrderType, string>)?.[orderType] ?? ''
}

type OrderTimlineItem = TimelineItem & {
  type: PartyOrder['type']
  distance: number
  estimatedTimeMs: number
  estimatedArrivalAt: Date
  targetedParty: PartyOrder['targetedParty']
  targetedBattle: PartyOrder['targetedBattle']
  targetedSettlement: PartyOrder['targetedSettlement']
  transferOfferPartyIntent: PartyOrder['transferOfferPartyIntent']
}

// TODO: useLocaleTimeAgo fo ref
function formatTimeFromNow(ms: number) {
  if (ms <= 0) {
    return 'just now'
  }

  const totalSec = Math.floor(ms / 1000)
  const sec = totalSec % 60
  const min = Math.floor(totalSec / 60) % 60
  const hours = Math.floor(totalSec / 3600)

  const parts = []
  if (hours > 0) {
    parts.push(`${hours} h`)
  }
  if (min > 0) {
    parts.push(`${min} m`)
  }
  if (sec > 0 || parts.length === 0) {
    parts.push(`${sec} s`)
  }

  return `in ${parts.join(' ')}`
}

const orders = computed<OrderTimlineItem[]>(() => {
  let totalDistance = 0
  const nowMs = Date.now()

  return party.orders
    .toSorted((a, b) => a.orderIndex - b.orderIndex)
    .map((order) => {
      const distance = order.pathSegments.reduce((sum, s) => sum + s.distance, 0) ?? 0
      totalDistance += distance

      const estimatedTimeMs = party.speed.finalSpeed > 0 ? (totalDistance / party.speed.finalSpeed) * 1000 : 0 // TODO: проверить, надо среднюю скорость из отрезков
      const estimatedArrivalAt = new Date(nowMs + estimatedTimeMs)

      return {
        type: order.type,
        title: t(`strategus.partyOrderType.${order.type}`),
        icon: getOrderIcon(order.type),
        distance: totalDistance,
        estimatedTimeMs,
        estimatedArrivalAt,
        targetedParty: order.targetedParty,
        targetedBattle: order.targetedBattle,
        targetedSettlement: order.targetedSettlement,
        transferOfferPartyIntent: order.transferOfferPartyIntent,
      }
    })
})
</script>

<template>
  <UTimeline
    :items="orders"
    size="lg"
    color="primary"
    :ui="{
      wrapper: 'pb-0 mt-0',
    }"
  >
    <template #title="{ item } : { item: OrderTimlineItem }">
      <div class="group flex items-center justify-between gap-2">
        <div class="flex items-center gap-2">
          <UiTextView variant="p">
            {{ item.title }}
          </UiTextView>

          <template v-if="item.targetedParty">
            <div class="flex items-center gap-1">
              <UserMedia :user="item.targetedParty.user" size="sm" />
              <UButton square size="sm" icon="i-lucide-locate-fixed" variant="link" color="neutral" @click="$emit('locate')" />

              <UButton
                v-if="item.type === PARTY_ORDER_TYPE.TransferOfferParty && item.transferOfferPartyIntent"
                size="sm"
                icon="crpg:chest"
                variant="subtle"
                color="neutral"
                @click="() => $emit('openTransferOffer', item.transferOfferPartyIntent!)"
              />
            </div>
          </template>

          <SettlementMedia v-if="item.targetedSettlement" :settlement="item.targetedSettlement" size="sm" />
        </div>

        <AppConfirmActionPopover
          @confirm="() => {
          // TODO:
          }"
        >
          <UButton
            variant="link" color="neutral" icon="i-lucide-x" size="xs" class="
              invisible
              group-hover:visible
            "
          />
        </AppConfirmActionPopover>
      </div>
    </template>

    <template #description="{ item } : { item: OrderTimlineItem }">
      <div class="flex items-center gap-2">
        <UiDataMedia icon="i-lucide-route" :label="`${$n(item.distance)}m`" size="sm" />
        <UiDataMedia icon="i-lucide-clock" :label="formatTimeFromNow(item.estimatedTimeMs)" size="sm" />
      </div>
    </template>
  </UTimeline>
</template>

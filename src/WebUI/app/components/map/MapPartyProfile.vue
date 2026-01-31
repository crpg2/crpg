<script setup lang="ts">
import type { TimelineItem } from '@nuxt/ui'

import { formatTimeAgo } from '@vueuse/core'
import { UiDataMedia } from '#components'

import type { Party, PartyOrder, PartyOrderType } from '~/models/strategus/party'

import { PARTY_ORDER_TYPE } from '~/models/strategus/party'

const { party } = defineProps<{ party: Party }>()

defineEmits<{
  locate: []
  startMove: []
}>()

const { n, t } = useI18n()

function getOrderIcon(orderType: PartyOrderType): string {
  return ({
    [PARTY_ORDER_TYPE.MoveToPoint]: 'i-lucide-move-up-right',
    [PARTY_ORDER_TYPE.FollowParty]: 'i-lucide-move-up-right',
    [PARTY_ORDER_TYPE.AttackParty]: 'crpg:game-mode-duel',
    [PARTY_ORDER_TYPE.MoveToSettlement]: 'i-lucide-move-up-right',
    [PARTY_ORDER_TYPE.AttackSettlement]: 'i-lucide-move-up-right',
    [PARTY_ORDER_TYPE.JoinBattle]: 'i-lucide-move-up-right',
  } satisfies Record<PartyOrderType, string>)?.[orderType] ?? ''
}

type OrderTimlineItem = TimelineItem & {
  distance: number
  estimatedTimeMs: number
  estimatedArrivalAt: Date
  targetedParty: PartyOrder['targetedParty']
  targetedBattle: PartyOrder['targetedBattle']
  targetedSettlement: PartyOrder['targetedSettlement']
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
      totalDistance += order.distance

      const estimatedTimeMs = party.speed > 0 ? (totalDistance / party.speed) * 1000 : 0
      const estimatedArrivalAt = new Date(nowMs + estimatedTimeMs)

      return {
        title: t(`strategus.partyOrderType.${order.type}`),
        icon: getOrderIcon(order.type),
        distance: totalDistance,
        estimatedTimeMs,
        estimatedArrivalAt,
        targetedParty: order.targetedParty,
        targetedBattle: order.targetedBattle,
        targetedSettlement: order.targetedSettlement,
      }
    })
})
</script>

<template>
  <UCard
    variant="subtle"
    :ui="{
      root: 'ring-2 ring-accented bg-elevated/90 backdrop-blur-md flex flex-col divide-muted',
    }"
  >
    <div class="flex flex-col gap-2">
      <div class="flex items-center gap-2">
        <!-- <div @click="$emit('locate')">
          <UIcon name="i-lucide-locate-fixed" class="size-6 cursor-pointer" />
        </div> -->

        <UserMedia
          :user="party.user"
          hidden-platform
          class="max-w-48"
        />
      </div>

      <div class="flex items-center gap-1.5">
        <AppCoin :value="party.gold" />
        <UiDataMedia icon="crpg:member" :label="$n(party.troops)" />
      </div>

      <div>Status: {{ party.orders.length ? 'Follows orders' : party.status }}</div>

      <div>Speed: {{ $n(party.speed) }}</div>

      <UTimeline
        :items="orders"
        size="sm"
        color="primary"
      >
        <template #title="{ item } : { item: OrderTimlineItem }">
          <div class="flex items-center gap-2">
            <UiTextView variant="p-sm">
              {{ item.title }}
            </UiTextView>
            <UserMedia v-if="item.targetedParty" :user="item.targetedParty.user" size="sm" />
            <SettlementMedia v-if="item.targetedSettlement" :settlement="item.targetedSettlement" size="sm" />
          </div>
        </template>

        <template #description="{ item } : { item: OrderTimlineItem }">
          <div class="flex items-center gap-2">
            <UiDataMedia icon="i-lucide-route" :label="$n(item.distance)" size="xs" />

            <UiDataMedia icon="i-lucide-calendar" :label="formatTimeFromNow(item.estimatedTimeMs)" size="xs" />
          </div>
        </template>
      </UTimeline>
    </div>
  </UCard>
</template>

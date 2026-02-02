<script setup lang="ts">
import type { TimelineItem } from '@nuxt/ui'

import { formatTimeAgo } from '@vueuse/core'
import { LazyMapPartyInventoryDrawer, UiDataMedia } from '#components'

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
    [PARTY_ORDER_TYPE.TransferOfferParty]: 'crpg:chest',
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

const incomingTransferOffers = computed(() => party.currentTransferOffers.filter(offer => offer.targetParty.id === party.id))

//

const overlay = useOverlay()
function openInventory() {
  overlay
    .create(LazyMapPartyInventoryDrawer)
    .open()
}
</script>

<template>
  <UCard
    variant="soft"
    :ui="{
      root: 'w-sm ring-1 ring-accented bg-elevated/90 backdrop-blur-md rounded-md',
      header: 'p-0 px-0! flex flex-col justify-center border-collapse items-center gap-2.5 ',
    }"
  >
    <template #header>
      <!-- <UserMedia
        :user="party.user"
        hidden-platform
        class="max-w-48"
      /> -->

      <UFieldGroup size="xl" class="w-full">
        <UTooltip text="Locate">
          <UButton
            icon="i-lucide-locate-fixed"
            block variant="outline" color="neutral" class="rounded-bl-none" @click="$emit('locate')"
          />
        </UTooltip>

        <UTooltip text="Open inventory">
          <UButton icon="crpg:chest" block variant="outline" color="neutral" @click="openInventory" />
        </UTooltip>

        <UTooltip text="Speed TODO: show calculate">
          <UButton variant="outline" block color="neutral">
            <UiDataMedia icon="crpg:horse" :label="$n(party.speed)" />
          </UButton>
        </UTooltip>

        <UTooltip>
          <UButton variant="outline" block color="neutral">
            <AppCoin :value="party.gold" compact />
          </UButton>
          <template #content>
            <AppCoin :value="party.gold" />
          </template>
        </UTooltip>

        <UTooltip>
          <UButton variant="outline" block color="neutral">
            <UiDataMedia icon="crpg:member" :label="$n(party.troops, 'compact')" />
          </UButton>
          <template #content>
            <UiDataMedia icon="crpg:member" :label="$n(party.troops)" />
          </template>
        </UTooltip>

        <UDropdownMenu
          :items="[
            {
              label: 'TODO:',
            },
          ]"
        >
          <UButton
            variant="outline" block color="neutral"
            class="rounded-br-none"
          >
            <UChip
              inset size="sm" show
              :ui="{ base: 'bg-notification' }"
            >
              <UIcon name="i-lucide-ellipsis-vertical" class="size-6" />
            </UChip>
          </UButton>
        </UDropdownMenu>
      </UFieldGroup>
    </template>

    <div class="flex flex-col gap-2">
      <UButton
        v-if="incomingTransferOffers.length"
        :label="`${incomingTransferOffers.length} incoming transfer offers`"
        variant="subtle"
      >
        <template #trailing>
          <UAvatarGroup :max="2" size="xs">
            <UAvatar
              v-for="offer in incomingTransferOffers"
              :key="offer.party.id"
              :src="offer.party.user.avatar || ''"
            />
          </UAvatarGroup>
        </template>
      </UButton>

      <!-- TODO: все статусы -->
      <UTimeline
        v-if="!party.orders.length"
        :items="[
          {
            title: party.status,
            icon: 'crpg:idle',
          },
        ]"
        size="lg"
        :ui="{
          wrapper: 'pb-0 mt-0',
        }"
      />

      <UTimeline
        v-else
        :items="orders"
        size="lg"
        color="primary"
        :ui="{
          wrapper: 'pb-0 mt-0',
        }"
      >
        <template #title="{ item } : { item: OrderTimlineItem }">
          <div class="flex items-center gap-2">
            <UiTextView variant="p">
              {{ item.title }}
            </UiTextView>

            <template v-if="item.targetedParty">
              <div class="flex items-center gap-1">
                <UserMedia :user="item.targetedParty.user" size="sm" />
                <UButton
                  square
                  size="sm"
                  icon="i-lucide-locate-fixed"
                  variant="link" color="neutral" @click="$emit('locate')"
                />
              </div>
            </template>
            <SettlementMedia v-if="item.targetedSettlement" :settlement="item.targetedSettlement" size="sm" />
          </div>
        </template>

        <template #description="{ item } : { item: OrderTimlineItem }">
          <div class="flex items-center gap-2">
            <UiDataMedia icon="i-lucide-route" :label="$n(item.distance)" size="sm" />
            <UiDataMedia icon="i-lucide-calendar" :label="formatTimeFromNow(item.estimatedTimeMs)" size="sm" />
          </div>
        </template>
      </UTimeline>
    </div>
  </UCard>
</template>

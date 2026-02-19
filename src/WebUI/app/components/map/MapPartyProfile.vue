<script setup lang="ts">
import type { TimelineItem } from '@nuxt/ui'

import { LazyMapPartyIncomingTransferOffersDrawer, LazyMapPartyInventoryDrawer, LazyMapPartyTransferOfferViewDrawer, UiDataMedia } from '#components'

import type { Party, PartyOrder, PartyOrderType, TransferOfferParty } from '~/models/strategus/party'

import { useParty } from '~/composables/strategus/use-party'
import { PARTY_ORDER_TYPE, PARTY_STATUS } from '~/models/strategus/party'
import { respondToPartyTransferOffer } from '~/services/strategus/party-service'

const { party } = defineProps<{ party: Party }>()

defineEmits<{
  locate: []
  startMove: []
}>()

const { clearPartyOrders, updateParty } = useParty()

const { t } = useI18n()
const toast = useToast()

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

const incomingTransferOffers = computed(() => party.currentTransferOffers.filter(offer => offer.targetParty.id === party.id))

type StatusTimlineItem = TimelineItem & {
  status: Party['status']
  currentParty: Party['currentParty']
  currentSettlement: Party['currentSettlement']
  currentBattle: Party['currentBattle']
  currentTransferOffer: TransferOfferParty
}

const statuses = computed<StatusTimlineItem[]>(() => {
  if (party.status === PARTY_STATUS.Idle) {
    return [
      {
        title: t(`strategus.partyStatus.${party.status}`),
        icon: 'crpg:idle',
      } as StatusTimlineItem,
    ]
  }

  if (party.status === PARTY_STATUS.AwaitingPartyOfferDecision) {
    const outgoingOffer = party.currentTransferOffers.find(offer => offer.targetParty.id !== party.id)
    if (!outgoingOffer) {
      return []
    }

    return [
      {
        status: party.status,
        title: t(`strategus.partyStatus.${party.status}`),
        icon: 'crpg:chest',
        currentTransferOffer: outgoingOffer,
        currentParty: party.currentParty,
      } as StatusTimlineItem,
    ]
  }

  return []
})

const overlay = useOverlay()
function openInventory() {
  overlay
    .create(LazyMapPartyInventoryDrawer)
    .open()
}

function openIncomingTransferOffers() {
  overlay
    .create(LazyMapPartyIncomingTransferOffersDrawer)
    .open({
      transferOffers: incomingTransferOffers.value,
      onClose: async (value, offer) => {
        if (!value || !offer) {
          return
        }

        if (offer.accepted) {
          toast.add({
            title: t('strategus.transferOffer.accepted'),
            description: t('strategus.transferOffer.acceptedDescription'),
            color: 'success',
          })

          await respondToPartyTransferOffer(
            offer.id,
            true,
            {
              gold: offer.accepted.gold,
              troops: offer.accepted.troops,
              items: offer.accepted.items,
            },
          )

          await updateParty()

          return
        }

        toast.add({
          title: t('strategus.transferOffer.declined'),
          description: t('strategus.transferOffer.declinedDescription'),
          color: 'error',
        })

        await respondToPartyTransferOffer(
          offer.id,
          false,
        )

        await updateParty()
      },
    })
}

// TODO: to composable
function openOutgoingTransferOffer(transferOffer: TransferOfferParty) {
  const drawer = overlay.create(LazyMapPartyTransferOfferViewDrawer)
  drawer.open({
    transferOffer,
    onCancel: () => {
      clearPartyOrders()
      drawer.close()
    },
  })
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

        <UTooltip>
          <UButton variant="outline" block color="neutral">
            <UiDataMedia icon="crpg:horse" :label="$n(party.speed.finalSpeed)" />
          </UButton>
          <template #content>
            <MapPartySpeedTooltipContent :speed="party.speed" />
          </template>
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
        block
        @click="openIncomingTransferOffers"
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
        :items="statuses"
        size="lg"
        :ui="{
          wrapper: 'pb-0 mt-0',
        }"
      >
        <template #title="{ item } : { item: StatusTimlineItem }">
          <div class="group flex items-center justify-between gap-2">
            <div class="flex items-center gap-2">
              <UiTextView variant="p">
                {{ item.title }}
              </UiTextView>

              <template v-if="item.currentParty">
                <div class="flex items-center gap-1">
                  <UserMedia :user="item.currentParty.user" size="sm" />
                  <UButton square size="sm" icon="i-lucide-locate-fixed" variant="link" color="neutral" @click="$emit('locate')" />
                  <UButton
                    v-if="item.status === PARTY_STATUS.AwaitingPartyOfferDecision"
                    size="sm"
                    icon="crpg:chest"
                    variant="subtle"
                    color="neutral"
                    @click="() => openOutgoingTransferOffer(item.currentTransferOffer)"
                  />
                </div>
              </template>
            </div>
          </div>
        </template>
      </UTimeline>

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
                    @click="() => openOutgoingTransferOffer(item.transferOfferPartyIntent!)"
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
    </div>
  </UCard>
</template>

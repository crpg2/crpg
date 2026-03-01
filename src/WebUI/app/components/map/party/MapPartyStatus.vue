<script setup lang="ts">
import type { TimelineItem } from '@nuxt/ui'

import type { Party, PartyStatus, TransferOfferParty } from '~/models/strategus/party'

import { PARTY_STATUS } from '~/models/strategus/party'

const { party } = defineProps<{ party: Party }>()

const emit = defineEmits<{
  locate: []
  openTransferOffer: [offer: TransferOfferParty]
}>()

const { t } = useI18n()

type StatusTimlineItem = TimelineItem & {
  status: Party['status']
  currentParty: Party['currentParty']
  currentSettlement: Party['currentSettlement']
  currentBattle: Party['currentBattle']
  currentTransferOffer: TransferOfferParty
}

function getStatusIcon(status: PartyStatus): string {
  return ({
    [PARTY_STATUS.Idle]: 'crpg:idle',
    [PARTY_STATUS.IdleInSettlement]: 'crpg:idle',
    [PARTY_STATUS.AwaitingPartyOfferDecision]: 'crpg:idle',
    [PARTY_STATUS.AwaitingBattleJoinDecision]: 'crpg:idle',
    [PARTY_STATUS.InBattle]: 'crpg:idle',
    [PARTY_STATUS.RecruitingInSettlement]: 'crpg:idle',
  } satisfies Record<PartyStatus, string>)?.[status] ?? ''
}

const statuses = computed<StatusTimlineItem[]>(() => {
  const title = t(`strategus.partyStatus.${party.status}`)
  const icon = getStatusIcon(party.status)

  if (party.status === PARTY_STATUS.Idle) {
    return [
      {
        status: party.status,
        title,
        icon,
      } as StatusTimlineItem,
    ]
  }

  if (party.status === PARTY_STATUS.IdleInSettlement) {
    return [
      {
        status: party.status,
        title,
        icon,
        currentSettlement: party.currentSettlement,
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
        title,
        icon,
        currentTransferOffer: outgoingOffer,
        currentParty: party.currentParty,
      } as StatusTimlineItem,
    ]
  }

  return []
})
</script>

<template>
  <UTimeline
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
                @click="() => $emit('openTransferOffer', item.currentTransferOffer)"
              />
            </div>
          </template>

          <template v-if="item.currentSettlement">
            <div class="flex items-center gap-1">
              <SettlementMedia :settlement="item.currentSettlement" size="sm" />
            </div>
          </template>
        </div>
      </div>
    </template>
  </UTimeline>
</template>

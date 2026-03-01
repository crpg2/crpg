<script setup lang="ts">
import { LazyMapPartyIncomingTransferOffersDrawer, LazyMapPartyInventoryDrawer, LazyMapPartyTransferOfferViewDrawer, UiDataMedia } from '#components'

import type { Party, TransferOfferParty } from '~/models/strategus/party'

import { useParty } from '~/composables/strategus/use-party'
import { respondToPartyTransferOffer } from '~/services/strategus/party-service'

const { party } = defineProps<{ party: Party }>()

const emit = defineEmits<{
  locate: []
  startMove: []
}>()

const { clearPartyOrders, updateParty } = useParty()

const { t } = useI18n()
const toast = useToast()

const incomingTransferOffers = computed(() => party.currentTransferOffers.filter(offer => offer.targetParty.id === party.id))

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

      <MapPartyStatus
        v-if="!party.orders.length"
        :party="party"
        @locate="$emit('locate')"
        @open-transfer-offer="openOutgoingTransferOffer"
      />

      <MapPartyOrders
        v-else
        :party="party"
        @locate="$emit('locate')"
        @open-transfer-offer="openOutgoingTransferOffer"
      />
    </div>
  </UCard>
</template>

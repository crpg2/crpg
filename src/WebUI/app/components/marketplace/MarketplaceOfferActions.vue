<script setup lang="ts">
import type { MarketplaceOffer } from '~/models/marketplace'
import type { User, UserItem } from '~/models/user'

import { canAcceptOffer } from '~/services/marketplace-service'

const { offer, user, userItems } = defineProps<{
  offer: MarketplaceOffer
  user: User
  userItems: UserItem[]
}>()

const emit = defineEmits<{
  delete: [offerId: number]
  accept: [offerId: number]
}>()

const isSeller = computed(() => user.id === offer.seller.id)
const canAccept = computed(() => canAcceptOffer(offer, user, userItems))
const hasRefund = computed(() => offer.goldFee > 0 || offer.offer.gold > 0 || offer.offer.heirloomPoints > 0)
</script>

<template>
  <div class="flex gap-2">
    <AppConfirmActionPopover
      v-if="isSeller"
      :content="{ side: 'left' }"
      @confirm="emit('delete', offer.id)"
    >
      <UButton
        variant="soft"
        color="error"
        :label="$t('marketplace.page.actions.delete')"
        icon="i-lucide-trash-2"
      />

      <template v-if="hasRefund" #description-content>
        <div class="space-y-2">
          <UiTextView variant="caption">
            {{ $t('marketplace.page.deletePopover.title') }}
          </UiTextView>

          <UiDataCell v-if="offer.goldFee > 0">
            <template #leftContent>
              {{ $t('marketplace.page.deletePopover.goldFee') }}
            </template>
            <AppCoin :value="offer.goldFee" />
          </UiDataCell>

          <UiDataCell v-if="offer.offer.gold > 0">
            <template #leftContent>
              {{ $t('marketplace.page.deletePopover.gold') }}
            </template>
            <AppCoin :value="offer.offer.gold" />
          </UiDataCell>

          <UiDataCell v-if="offer.offer.heirloomPoints > 0">
            <template #leftContent>
              {{ $t('marketplace.page.deletePopover.heirloomPoints') }}
            </template>
            <AppLoom :point="offer.offer.heirloomPoints" />
          </UiDataCell>
        </div>
      </template>
    </AppConfirmActionPopover>

    <AppConfirmActionDialog
      v-if="canAccept"
      :title="$t('marketplace.page.acceptDialog.title')"
      :confirm="$t('marketplace.page.acceptDialog.title')"
      :ui="{ content: 'max-w-2xl' }"
      @close="(confirmed) => confirmed && emit('accept', offer.id)"
    >
      <UButton
        variant="soft"
        color="success"
        :label="$t('marketplace.page.actions.accept')"
        icon="i-lucide-check"
      />

      <template #title>
        <div class="flex items-center justify-center gap-3">
          {{ $t('marketplace.page.acceptDialog.titleFrom') }}
          <UserMedia :user="offer.seller" />
        </div>
      </template>

      <template #description>
        <MarketplaceOfferAssetGroup
          :offered-title="$t('marketplace.page.acceptDialog.youWillReceive')"
          :offered="offer.request"
          :requested-title="$t('marketplace.page.acceptDialog.youWillGive')"
          :requested="offer.offer"
        />
      </template>
    </AppConfirmActionDialog>
  </div>
</template>

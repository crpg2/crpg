<script setup lang="ts">
import type { MarketplaceListing } from '~/models/marketplace'
import type { User, UserItem } from '~/models/user'

import { canAcceptListing } from '~/services/marketplace-service'

const { listing, user, userItems } = defineProps<{
  listing: MarketplaceListing
  user: User
  userItems: UserItem[]
}>()

const emit = defineEmits<{
  delete: [listingId: number]
  accept: [listingId: number]
}>()

const isSeller = computed(() => user.id === listing.seller.id)
const canAccept = computed(() => canAcceptListing(listing, user, userItems))
const hasRefund = computed(() => listing.goldFee > 0 || listing.offer.gold > 0 || listing.offer.heirloomPoints > 0)
</script>

<template>
  <div class="flex gap-2">
    <AppConfirmActionPopover
      v-if="isSeller"
      :content="{ side: 'left' }"
      @confirm="emit('delete', listing.id)"
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

          <UiDataCell v-if="listing.goldFee > 0">
            <template #leftContent>
              {{ $t('marketplace.page.deletePopover.goldFee') }}
            </template>
            <AppCoin :value="listing.goldFee" />
          </UiDataCell>

          <UiDataCell v-if="listing.offer.gold > 0">
            <template #leftContent>
              {{ $t('marketplace.page.deletePopover.gold') }}
            </template>
            <AppCoin :value="listing.offer.gold" />
          </UiDataCell>

          <UiDataCell v-if="listing.offer.heirloomPoints > 0">
            <template #leftContent>
              {{ $t('marketplace.page.deletePopover.heirloomPoints') }}
            </template>
            <AppLoom :point="listing.offer.heirloomPoints" />
          </UiDataCell>
        </div>
      </template>
    </AppConfirmActionPopover>

    <AppConfirmActionDialog
      v-if="canAccept"
      :title="$t('marketplace.page.acceptDialog.title')"
      :confirm="$t('marketplace.page.acceptDialog.title')"
      :ui="{ content: 'max-w-2xl' }"
      @close="(confirmed) => confirmed && emit('accept', listing.id)"
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
          <UserMedia :user="listing.seller" />
        </div>
      </template>

      <template #description>
        <MarketplaceListingAssetGroup
          :offered-title="$t('marketplace.page.acceptDialog.youWillReceive')"
          :offered="listing.request"
          :requested-title="$t('marketplace.page.acceptDialog.youWillGive')"
          :requested="listing.offer"
        />
      </template>
    </AppConfirmActionDialog>
  </div>
</template>

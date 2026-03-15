<script setup lang="ts">
import type { MarketplaceOfferAsset } from '~/models/marketplace'

defineProps<{
  offered: MarketplaceOfferAsset
  offeredTitle?: string
  requested: MarketplaceOfferAsset
  requestedTitle?: string
}>()
</script>

<template>
  <div class="mt-4 grid grid-cols-[1fr_auto_1fr] gap-3">
    <div class="space-y-3">
      <UiTextView variant="caption-sm">
        {{ offeredTitle ?? $t('marketplace.side.offered') }}
      </UiTextView>
      <MarketplaceOfferAssetView :asset="offered" class="justify-center" />
    </div>
    <USeparator orientation="vertical" icon="crpg:trade" />
    <div class="space-y-3">
      <UiTextView variant="caption-sm">
        {{ requestedTitle ?? $t('marketplace.side.requested') }}
      </UiTextView>
      <MarketplaceOfferAssetView :asset="requested" class="justify-center" />
    </div>
  </div>
</template>

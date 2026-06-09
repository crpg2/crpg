<script setup lang="ts">
import type { MarketplaceListingAsset } from '~/models/marketplace'

defineProps<{
  offered: MarketplaceListingAsset
  offeredTitle?: string
  requested: MarketplaceListingAsset
  requestedTitle?: string
}>()
</script>

<template>
  <div class="mt-4 grid grid-cols-[1fr_auto_1fr] gap-3">
    <div class="space-y-3">
      <UiTextView variant="caption-sm">
        {{ offeredTitle ?? $t('marketplace.side.offered') }}
      </UiTextView>
      <MarketplaceListingAssetView :asset="offered" class="justify-center" />
    </div>
    <USeparator orientation="vertical" icon="crpg:trade" />
    <div class="space-y-3">
      <UiTextView variant="caption-sm">
        {{ requestedTitle ?? $t('marketplace.side.requested') }}
      </UiTextView>
      <MarketplaceListingAssetView :asset="requested" class="justify-center" />
    </div>
  </div>
</template>

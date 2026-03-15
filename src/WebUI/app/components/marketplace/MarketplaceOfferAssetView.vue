<script setup lang="ts">
import type { MarketplaceOfferAsset } from '~/models/marketplace'

const { asset } = defineProps<{
  asset: MarketplaceOfferAsset
}>()
</script>

<template>
  <ItemMedia
    v-if="asset.item"
    :base-id="asset.item.baseId"
    :rank="asset.item.rank"
    :name="asset.item.name"
  >
    <template v-if="asset.gold || asset.heirloomPoints" #name-caption>
      <div class="flex items-center gap-3">
        <AppCoin v-if="asset.gold" :value="asset.gold" />
        <AppLoom v-if="asset.heirloomPoints" :point="asset.heirloomPoints" />
      </div>
    </template>
  </ItemMedia>

  <div v-else class="flex items-center gap-3">
    <AppCoin v-if="asset.gold" :value="asset.gold" />
    <AppLoom v-if="asset.heirloomPoints" :point="asset.heirloomPoints" />
  </div>
</template>

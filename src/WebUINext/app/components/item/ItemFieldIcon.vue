<script setup lang="ts">
import type { IconedBucket } from '~/services/item-service'

import { IconBucketType } from '~/services/item-service'

const { icon, size = 'xl' } = defineProps<{
  icon: IconedBucket
  size?: 'xl' | '2xl'
}>()
</script>

<template>
  <div
    class="
      flex items-center
      hover:opacity-80
    "
  >
    <UiSpriteSymbol
      v-if="icon.type === IconBucketType.Asset"
      :name="icon.name"
      viewBox="0 0 48 48"
      :class="size === '2xl' ? 'size-8' : 'size-6'"
    />
    <UIcon
      v-else-if="icon.type === IconBucketType.Svg"
      :name="`crpg:${icon.name}`"
      :class="size === '2xl' ? 'size-8' : 'size-6'"
    />
  </div>
</template>

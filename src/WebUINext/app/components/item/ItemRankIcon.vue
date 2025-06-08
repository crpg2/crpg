<script setup lang="ts">
import type { ItemRank } from '~/models/item'

import { getRankColor } from '~/services/item-service'

const { rank } = defineProps<{ rank: ItemRank }>()

const rankColor = computed(() => getRankColor(rank))
</script>

<template>
  <UTooltip>
    <OIcon
      icon="rare-duotone"
      size="sm"
      :style="{
        '--fa-primary-opacity': 0.15,
        '--fa-primary-color': '#fff',
        '--fa-secondary-opacity': 1,
        '--fa-secondary-color': rankColor,
      }"
    />
    <template #content>
      <i18n-t
        scope="global"
        keypath="item.format.rank"
        tag="span"
      >
        <template #rank>
          <span
            class="font-semibold"
            :style="{ color: rankColor }"
          >+{{ rank }}</span>
        </template>
      </i18n-t>
    </template>
  </UTooltip>
</template>

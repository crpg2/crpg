<script setup lang="ts">
import type { ItemRank } from '~/models/item'

import { getRankColor } from '~/services/item-service'

const { rank } = defineProps<{ rank: ItemRank }>()

const rankColor = computed(() => getRankColor(rank))
</script>

<template>
  <UTooltip>
    <UIcon
      name="crpg:item-rank-duotone"
      class="size-5 outline-0 select-none"
      :style="{
        color: rankColor,
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
            class="font-bold"
            :style="{ color: rankColor }"
          >
            +{{ rank }}
          </span>
        </template>
      </i18n-t>
    </template>
  </UTooltip>
</template>

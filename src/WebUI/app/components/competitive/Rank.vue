<script setup lang="ts">
import NumberFlow from '@number-flow/vue'

import type { Rank } from '~/models/competitive'

import { getRankByCompetitiveValue } from '~/services/leaderboard-service'

const { competitiveValue, rankTable, animated = true } = defineProps<{
  competitiveValue: number
  rankTable: Rank[]
  animated?: boolean
}>()

const rank = computed(() => getRankByCompetitiveValue(rankTable, competitiveValue))

const { locale } = useI18n()
</script>

<template>
  <div
    class="font-black"
    :style="{ color: rank.color }"
  >
    {{ rank.title }} -
    <NumberFlow
      :animated
      :value="competitiveValue"
      :locales="locale"
      :format="{
        maximumFractionDigits: 0,
        minimumFractionDigits: 0,
      }"
      will-change
    />
  </div>
</template>

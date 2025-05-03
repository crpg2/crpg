<script setup lang="ts">
import { groupBy, inRange } from 'es-toolkit'

import type { Rank } from '~/models/competitive'

const { competitiveValue = null, rankTable } = defineProps<{
  competitiveValue?: number | null
  rankTable: Rank[]
}>()

const groupedRankTable = computed(() => groupBy([...rankTable].reverse(), r => r.groupTitle))
</script>

<template>
  <div class="max-h-[90vh] space-y-8 overflow-y-auto px-12 pb-6 pt-8 text-center">
    <h4 class="text-xl">
      {{ $t('rankTable.title') }}
    </h4>

    <OTable
      :data="Object.entries(groupedRankTable)"
      bordered
      hoverable
    >
      <!-- TODO: spec! refactoring FIXME: -->
      <OTableColumn v-slot="{ row }: { row: [string, Rank[]] }">
        <span :style="{ color: row[1][0].color }">
          {{ row[0] }}
        </span>
      </OTableColumn>

      <OTableColumn
        v-for="(_col, idx) in 5"
        :key="idx"
        v-slot="{ row }: { row: [string, Rank[]] }"
        :label="String(5 - idx)"
      >
        <span
          v-if="
            competitiveValue !== null
              && inRange(competitiveValue, row[1][4 - idx].min, row[1][4 - idx].max)
          "
          :style="{ color: row[1][4 - idx].color }"
          class="font-black"
        >
          {{ row[1][4 - idx].min }} - {{ row[1][4 - idx].max }} ({{ $t('you') }})
        </span>

        <span
          v-else
          v-tooltip="`${row[0]} ${5 - idx}`"
          :style="{ color: row[1][4 - idx].color }"
        >
          {{ row[1][4 - idx].min }} - {{ row[1][4 - idx].max }}
        </span>
      </OTableColumn>
    </OTable>

    <div class="prose prose-invert space-y-3 text-left">
      <h5 class="text-content-100">
        {{ $t('character.statistics.rank.tooltip.title') }}
      </h5>
      <div
        class="text-2xs"
        v-html="$t('character.statistics.rank.tooltip.desc')"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import { UTooltip } from '#components'
import { groupBy, inRange } from 'es-toolkit'

import type { Rank } from '~/models/competitive'

const { competitiveValue, rankTable } = defineProps<{
  competitiveValue?: number
  rankTable: Rank[]
}>()

const groupedRankTable = computed(() => groupBy(rankTable, r => r.groupTitle))
</script>

<template>
  <div class="space-y-8">
    <div class="flex flex-col-reverse gap-5 overflow-x-auto">
      <div
        v-for="(ranks, groupTitle, groupIdx) in groupedRankTable"
        :key="groupIdx"
      >
        <div
          class="mb-1.5 text-xs font-bold"
          :style="{
            color: ranks.at(0)!.color,
          }"
        >
          {{ groupTitle }} • {{ $n(ranks.at(0)!.min) }} – {{ $n(ranks.at(-1)!.max) }}
        </div>

        <div class="flex flex-col-reverse gap-1.5">
          <UTooltip
            v-for="(rank, idx) in ranks"
            :key="idx"
            :text="`${rank.title} • ${$n(rank.min)} – ${$n(rank.max)}`"
          >
            <div
              class="
                flex h-8 items-center rounded px-2 text-2xs text-highlighted select-none
                text-shadow-lg/20
              "
              :style="{
                backgroundColor: rank.color,
                width: `calc(6rem + 0.5rem * ${groupIdx * ranks.length + idx + 1})`,
              }"
            >
              {{ rank.title }}  • {{ $n(rank.min) }} – {{ $n(rank.max) }}
              <template v-if="competitiveValue && inRange(competitiveValue, rank.min, rank.max)">
                ({{ $t('you') }} • {{ $n(competitiveValue) }})
              </template>
            </div>
          </UTooltip>
        </div>
      </div>
    </div>

    <div class="prose space-y-3 text-left prose-invert">
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

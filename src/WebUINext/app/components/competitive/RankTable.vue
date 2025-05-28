<script setup lang="ts">
import type { TableColumn } from '@nuxt/ui'

import { tw } from '#imports'
import { groupBy, inRange } from 'es-toolkit'

import type { Rank } from '~/models/competitive'

const { competitiveValue, rankTable } = defineProps<{
  competitiveValue?: number
  rankTable: Rank[]
}>()

interface RankGroup {
  title: string
  rank1: Rank
  rank2: Rank
  rank3: Rank
  rank4: Rank
  rank5: Rank
}

const groupedRankTable = computed<RankGroup[]>(() =>
  Object
    .entries(groupBy(rankTable.toReversed(), r => r.groupTitle))
    .map(([title, values]) => ({
      title,
      ...values.reduce((out, it, idx) => {
        out[`rank${idx + 1}`] = it
        return out
      }, {}),
    })))

const columns: TableColumn<RankGroup>[] = [
  {
    accessorKey: 'title',
    header: '',
    cell: ({ row }) => h('span', { style: `color: ${row.original.rank1.color}` }, row.original.title),
  },
  ...[
    'rank1',
    'rank2',
    'rank3',
    'rank4',
    'rank5',
  ].reverse().map<TableColumn<RankGroup>>((it, idx) => ({
    accessorFn: row => row[it],
    header: String(idx + 1),
    cell: ({ row }) => {
      const rank = row.original[it]
      return h('span', { style: `color: ${rank.color}` }, `${rank.min} - ${rank.max}`)
    },
  })),
]
</script>

<template>
  <div class="space-y-8">
    <UTable
      class="relative rounded-md border border-muted"
      :data="groupedRankTable"
      :columns
      :meta="{
        class: {
          tr: (row) => tw`text-[${row.original.rank1.color}]`,
        },
      }"
    />

    <!--
    TODO: FIXME:
      <span
          v-if="
            competitiveValue !== null
              && inRange(competitiveValue, row[1][4 - idx].min, row[1][4 - idx].max)
          "
          :style="{ color: row[1][4 - idx].color }"
          class="font-black"
        >
          {{ row[1][4 - idx].min }} - {{ row[1][4 - idx].max }} ({{ $t('you') }})
        </span> -->

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

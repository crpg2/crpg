<script lang="ts" setup>
import type { DateTimeDuration } from '@internationalized/date'
import type { TableColumn, TabsItem } from '@nuxt/ui'
import type { BarSeriesOption } from 'echarts/charts'
import type {
  DataZoomComponentOption,
  GridComponentOption,
  LegendComponentOption,
  TooltipComponentOption,
} from 'echarts/components'
import type { ComposeOption } from 'echarts/core'

import { getLocalTimeZone, now } from '@internationalized/date'
// TODO: FIXME: composition + components
import NumberFlow from '@number-flow/vue'
import { AppCoin, AppExperience, UIcon, UiDataCell } from '#components'
import { BarChart } from 'echarts/charts'
import {
  DataZoomComponent,
  GridComponent,
  LegendComponent,
  TooltipComponent,
} from 'echarts/components'
import { registerTheme, use } from 'echarts/core'
import { SVGRenderer } from 'echarts/renderers'
import VChart from 'vue-echarts'

import type { CharacterEarnedData, CharacterEarningType } from '~/models/character'
import type { GameMode } from '~/models/game-mode'
import type { TimeSeries } from '~/models/time-series'

import theme from '~/assets/echart-theme.json'
import { useCharacter } from '~/composables/character/use-character'
import { useAsyncCallback } from '~/composables/utils/use-async-callback'
import { CHARACTER_EARNING_TYPE } from '~/models/character'
import {
  convertCharacterEarningStatisticsToTimeSeries,
  getCharacterEarningStatistics,
  summaryByGameModeCharacterEarningStatistics,
} from '~/services/character-service'
import { gameModeToIcon } from '~/services/game-mode-service'

use([BarChart, TooltipComponent, LegendComponent, DataZoomComponent, GridComponent, SVGRenderer])
registerTheme('crpg', theme)

type EChartsOption = ComposeOption<
  | LegendComponentOption
  | TooltipComponentOption
  | GridComponentOption
  | BarSeriesOption
  | DataZoomComponentOption
>

const route = useRoute('characters-id-stats')

const { character } = useCharacter()

const { d, n, t } = useI18n()

interface LegendSelectEvent {
  name: string
  type: 'legendselectchanged'
  selected: Record<string, boolean>
}

const loading = ref(false)
const loadingOptions = {
  color: '#4ea397',
  maskColor: 'rgba(255, 255, 255, 0.4)',
  text: 'Loading…',
}

enum Zoom {
  '1h' = '1h',
  '3h' = '3h',
  '12h' = '12h',
  '2d' = '2d',
  '7d' = '7d',
  '14d' = '14d',
}

const durationByZoom: Record<Zoom, DateTimeDuration> = {
  [Zoom['1h']]: { hours: 1 },
  [Zoom['3h']]: { hours: 3 },
  [Zoom['12h']]: { hours: 12 },
  [Zoom['2d']]: { days: 2 },
  [Zoom['7d']]: { days: 7 },
  [Zoom['14d']]: { days: 14 },
}

const getStart = (zoom: Zoom) => {
  const duration = durationByZoom[zoom] ?? durationByZoom[Zoom['1h']]
  return now(getLocalTimeZone()).subtract(duration).toDate()
}

const toBarSeries = (ts: TimeSeries): BarSeriesOption => ({ ...ts, type: 'bar' })
const extractTSName = (ts: TimeSeries): string => ts.name

const zoomModel = ref<Zoom>(Zoom['1h'])
const start = computed(() => getStart(zoomModel.value))
const end = ref<Date>(new Date())

const {
  state: rawEarningStatistics,
  execute: loadCharacterEarningStatistics,
  isLoading: loadingEarningStatistics,
} = await useAsyncState(
  (id: number) => getCharacterEarningStatistics(id, start.value),
  [],
  {
    immediate: false,
    resetOnExecute: false,
  },
)

watch(
  () => route.params.id,
  () => {
    loadCharacterEarningStatistics(0, Number(route.params.id))
  },
  { immediate: true },
)

const statTypeModel = ref<CharacterEarningType>(CHARACTER_EARNING_TYPE.Exp)
const characterEarningStatistics = computed(() => convertCharacterEarningStatisticsToTimeSeries(rawEarningStatistics.value, statTypeModel.value))

const dataZoom = ref<[number, number]>([start.value.getTime(), end.value.getTime()])
const setDataZoom = (start: number, end: number) => {
  dataZoom.value = [start, end]
}

const chart = useTemplateRef('chart')

const legend = ref<string[]>(characterEarningStatistics.value.map(extractTSName))
const activeSeries = ref<string[]>(characterEarningStatistics.value.map(extractTSName))

const option = shallowRef<EChartsOption>({
  legend: {
    data: legend.value,
    itemGap: 16,
    formatter: name => t(`game-mode.${name}`),
    orient: 'vertical',
    right: 0,
    top: 'center',
  },
  series: characterEarningStatistics.value.map(toBarSeries),
  tooltip: {
    axisPointer: {
      label: {
        formatter: param => d(new Date(param.value), 'long'),
      },
      type: 'shadow',
    },
    trigger: 'axis',
  },
  xAxis: {
    max: Date.now(),
    min: getStart(Zoom['1h']),
    splitArea: {
      show: false,
    },
    splitLine: {
      show: false,
    },
    type: 'time',
  },
  yAxis: {
    splitArea: {
      show: false,
    },
    type: 'value',
  },
  dataZoom: [
    {
      type: 'slider',
      labelFormatter: value => d(new Date(value), 'short'),
    },
  ],
})

const setZoom = () => {
  end.value = new Date()
  option.value = {
    ...option.value,
    xAxis: {
      ...option.value.xAxis,
      max: end.value,
      min: start.value,
    },
  }
}

const onDataZoomChanged = () => {
  const option = chart.value?.getOption()
  // @ts-expect-error TODO: write types
  setDataZoom(option.dataZoom[0].startValue, option.dataZoom[0].endValue)
}

const onLegendSelectChanged = (e: LegendSelectEvent) => {
  activeSeries.value = Object.entries(e.selected)
    .filter(([, status]) => Boolean(status))
    .map(([legend]) => legend)
}

watch(characterEarningStatistics, () => {
  option.value = {
    ...option.value,
    legend: {
      ...option.value.legend,
      data: characterEarningStatistics.value.map(extractTSName),
    },
    series: characterEarningStatistics.value.map(toBarSeries),
  }
  activeSeries.value = characterEarningStatistics.value.map(extractTSName)
})
watch(statTypeModel, () => loadCharacterEarningStatistics(0, character.value.id))
watch(zoomModel, () => {
  setZoom()
  loadCharacterEarningStatistics(0, character.value.id)
  setDataZoom(start.value.getTime(), end.value.getTime())
})

const total = computed(() =>
  characterEarningStatistics.value
    .filter(ts => activeSeries.value.includes(ts.name))
    .flatMap(ts => ts.data)
    .filter(([date]) => {
      const time = date.getTime()
      const [from, to] = dataZoom.value
      return time >= from && time <= to
    })
    .reduce((total, [, value]) => total + value, 0),
)

interface CharacterEarnedDataWithGameMode extends CharacterEarnedData {
  gameMode: GameMode
}

const summary = computed<CharacterEarnedDataWithGameMode[]>(() =>
  Object.entries(summaryByGameModeCharacterEarningStatistics(rawEarningStatistics.value))
    .map(([gameMode, data]) => ({
      gameMode: gameMode as GameMode,
      ...data,
    })))

const statTypeItems = Object.values(CHARACTER_EARNING_TYPE).map<TabsItem>(statType => ({
  label: t(`character.earningStats.type.${statType}`),
  value: statType,
}))

const zoomItems = Object.entries(durationByZoom).map<TabsItem>(([zoomKey, zoomValue]) => ({
  label: t(`dateTimeFormat.${Object.keys(zoomValue).includes('days') ? 'dd' : 'hh'}`, zoomValue as any),
  value: zoomKey,
}))

const columns: TableColumn<CharacterEarnedDataWithGameMode>[] = [
  {
    accessorKey: 'gameMode',
    header: '',
    cell: ({ row }) => h(UiDataCell, null, {
      default: () => t(`game-mode.${row.original.gameMode}`),
      leftContent: () => h(UIcon, { name: `crpg:${gameModeToIcon[row.original.gameMode]}`, class: 'size-6' }),
    }),
  },
  {
    accessorKey: 'timeEffort',
    header: () => t('character.earningStats.summary.timeEffort'),
    cell: ({ row }) => t('dateTimeFormat.ss', { secondes: Math.round(row.original.timeEffort) }),
  },
  {
    accessorKey: 'experience',
    header: () => h(AppExperience, { size: 'lg' }),
    cell: ({ row }) => `${n(row.original.experience)}${row.original.timeEffort ? ` (${n(row.original.experience / row.original.timeEffort)}/s)` : ''}`,
  },
  {
    accessorKey: 'gold',
    header: () => h(AppCoin, { size: 'lg' }),
    cell: ({ row }) =>
      h('span', {
        class: row.original.gold < 0 ? 'text-error' : 'text-success',
      }, `${n(row.original.gold)}${row.original.timeEffort ? ` (${n(row.original.gold / row.original.timeEffort)}/s)` : ''}`),
  },
]
</script>

<template>
  <UContainer>
    <div class="flex min-w-[56rem] flex-col pt-8 pr-10 pl-5">
      <div class="flex items-center justify-center gap-3">
        <UTabs
          v-model="zoomModel"
          :items="zoomItems"
          variant="pill"
          :content="false"
        />

        <UTabs
          v-model="statTypeModel"
          :items="statTypeItems"
          variant="pill"
          :content="false"
        />

        <div class="min-w-[140px]">
          <AppCoin
            v-if="statTypeModel === CHARACTER_EARNING_TYPE.Gold"
            size="lg"
            :class="total < 0 ? '!text-error' : '!text-success'"
          >
            <NumberFlow
              :value="total"
              locales="en-US"
            />
          </AppCoin>

          <AppExperience
            v-else
            size="lg"
          >
            <NumberFlow
              :value="total"
              locales="en-US"
            />
          </AppExperience>
        </div>
      </div>

      <div class="mb-6 h-[30rem]">
        <VChart
          ref="chart"
          theme="crpg"
          :option="option"
          :loading="loading"
          :loading-options="loadingOptions"
          @legendselectchanged="onLegendSelectChanged"
          @datazoom="onDataZoomChanged"
        />
      </div>

      <UTable
        class="rounded-md border border-muted"
        :loading="loadingEarningStatistics"
        :data="summary"
        :columns
      >
        <template #empty>
          <UiResultNotFound />
        </template>
      </UTable>
    </div>
  </UContainer>
</template>

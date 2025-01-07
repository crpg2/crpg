<script lang="ts" setup>
// TODO: FIXME: composition + components
import type { BarSeriesOption } from 'echarts/charts'
import type {
  DataZoomComponentOption,
  GridComponentOption,
  LegendComponentOption,
  TooltipComponentOption,
} from 'echarts/components'
import type { ComposeOption } from 'echarts/core'
import type { DurationLike } from 'luxon'

import { BarChart } from 'echarts/charts'
import {
  DataZoomComponent,
  GridComponent,
  LegendComponent,
  TooltipComponent,
} from 'echarts/components'
import { registerTheme, use } from 'echarts/core'
import { SVGRenderer } from 'echarts/renderers'
import { DateTime } from 'luxon'
import VChart from 'vue-echarts'

import type { CharacterEarnedMetadata } from '~/models/activity-logs'
import type { CharacterEarnedData } from '~/models/character'
import type { GameMode } from '~/models/game-mode'
import type { TimeSeries } from '~/models/timeseries'

import theme from '~/assets/themes/oruga-tailwind/echart-theme.json'
import { useAsyncCallback } from '~/composables/utils/use-async-callback'
import { CharacterEarningType } from '~/models/character'
import { convertCharacterEarningStatisticsToTimeSeries, getCharacterEarningStatistics, summaryByGameModeCharacterEarningStatistics } from '~/services/characters-service'
import { gameModeToIcon } from '~/services/game-mode-service'
import { d } from '~/services/translate-service'
import { characterKey } from '~/symbols/character'

use([BarChart, TooltipComponent, LegendComponent, DataZoomComponent, GridComponent, SVGRenderer])
registerTheme('crpg', theme)
type EChartsOption = ComposeOption<
  | LegendComponentOption
  | TooltipComponentOption
  | GridComponentOption
  | BarSeriesOption
  | DataZoomComponentOption
>

definePage({
  meta: {
    roles: ['User', 'Moderator', 'Admin'],
  },
  props: true,
})

const character = injectStrict(characterKey)

enum Zoom {
  '1h' = '1h',
  '3h' = '3h',
  '12h' = '12h',
  '2d' = '2d',
  '7d' = '7d',
  '14d' = '14d',
}

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

const durationByZoom: Record<Zoom, DurationLike> = {
  [Zoom['1h']]: {
    hours: 1,
  },
  [Zoom['3h']]: {
    hours: 3,
  },
  [Zoom['12h']]: {
    hours: 12,
  },
  [Zoom['2d']]: {
    days: 2,
  },
  [Zoom['7d']]: {
    days: 7,
  },
  [Zoom['14d']]: {
    days: 14,
  },
}

const getStart = (zoom: Zoom) => {
  switch (zoom) {
    case Zoom['1h']:
      return DateTime.local().minus(durationByZoom[Zoom['1h']]).toJSDate()
    case Zoom['3h']:
      return DateTime.local().minus(durationByZoom[Zoom['3h']]).toJSDate()
    case Zoom['12h']:
      return DateTime.local().minus(durationByZoom[Zoom['12h']]).toJSDate()
    case Zoom['2d']:
      return DateTime.local().minus(durationByZoom[Zoom['2d']]).toJSDate()
    case Zoom['7d']:
      return DateTime.local().minus(durationByZoom[Zoom['7d']]).toJSDate()
    case Zoom['14d']:
      return DateTime.local().minus(durationByZoom[Zoom['14d']]).toJSDate()

    default:
      return DateTime.local().minus(durationByZoom[Zoom['1h']]).toJSDate()
  }
}

const zoomModel = ref<Zoom>(Zoom['1h'])
const start = computed(() => getStart(zoomModel.value))
const end = ref<Date>(new Date())

const statTypeModel = ref<CharacterEarningType>(CharacterEarningType.Exp)

const dataZoom = ref<[number, number]>([start.value.getTime(), end.value.getTime()])
const setDataZoom = (start: number, end: number) => {
  dataZoom.value = [start, end]
}

const onDataZoomChanged = () => {
  const option = chart.value?.getOption()
  // @ts-expect-error TODO: write types
  setDataZoom(option.dataZoom[0].startValue, option.dataZoom[0].endValue)
}

// TODO: spec
const { execute: loadCharacterEarningStatistics, state: rawEarningStatistics }
  = await useAsyncState(
    ({ id }: { id: number }) => getCharacterEarningStatistics(id, start.value),
    [],
    {
      immediate: false,
      resetOnExecute: false,
    },
  )

const characterEarningStatistics = computed(() => convertCharacterEarningStatisticsToTimeSeries(rawEarningStatistics.value, statTypeModel.value))

const toBarSeries = (ts: TimeSeries): BarSeriesOption => ({ ...ts, type: 'bar' })
const extractTSName = (ts: TimeSeries) => ts.name

const legend = ref<string[]>(characterEarningStatistics.value.map(extractTSName))
const activeSeries = ref<string[]>(characterEarningStatistics.value.map(extractTSName))

const { execute: onUpdate } = useAsyncCallback(async (characterId: number) => {
  await loadCharacterEarningStatistics(0, { id: characterId })
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

watch(statTypeModel, () => onUpdate(character.value.id))
watch(zoomModel, () => {
  setZoom()
  onUpdate(character.value.id)
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
    .reduce((total, [_date, value]) => total + value, 0),
)

interface CharacterEarnedDataWithGameMode extends CharacterEarnedData {
  gameMode: GameMode
}

const summary = computed<CharacterEarnedDataWithGameMode[]>(() => Object.entries(summaryByGameModeCharacterEarningStatistics(rawEarningStatistics.value)).map(([gameMode, data]) => ({
  gameMode: gameMode as GameMode,
  ...data,
})))

const chart = shallowRef<InstanceType<typeof VChart> | null>(null)

const option = shallowRef<EChartsOption>({
  legend: {
    data: legend.value,
    itemGap: 16,
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

const onLegendSelectChanged = (e: LegendSelectEvent) => {
  activeSeries.value = Object.entries(e.selected)
    .filter(([_legend, status]) => Boolean(status))
    .map(([legend, _status]) => legend)
}

const fetchPageData = (characterId: number) => Promise.all([onUpdate(characterId)])

onBeforeRouteUpdate(async (to, from) => {
  if (to.name === from.name && to.name === 'CharactersIdStats') {
    const characterId = Number(to.params.id)
    await fetchPageData(characterId)
  }
  return true
})

await fetchPageData(character.value.id)
</script>

<template>
  <div class="mx-auto max-w-2xl space-y-12 pb-12">
    <div class="flex max-h-[90vh] min-w-[56rem] flex-col pl-5 pr-10 pt-8">
      <div class="flex items-center gap-4">
        <OTabs
          v-model="statTypeModel"
          type="fill-rounded"
          content-class="hidden"
        >
          <OTabItem
            :value="CharacterEarningType.Exp"
            :label="$t('character.earningStats.type.experience')"
          />
          <OTabItem
            :value="CharacterEarningType.Gold"
            :label="$t('character.earningStats.type.gold')"
          />
        </OTabs>

        <OTabs
          v-model="zoomModel"
          type="fill-rounded"
          content-class="hidden"
        >
          <OTabItem
            v-for="(zoomValue, zoomKey) in durationByZoom"
            :key="zoomKey"
            :value="zoomKey"
            :label="
              $t(
                `dateTimeFormat.${Object.keys(zoomValue).includes('days') ? 'dd' : 'hh'}`,
                zoomValue as any,
              )
            "
          />
        </OTabs>

        <div class="flex-1 text-lg font-semibold">
          <Coin
            v-if="statTypeModel === CharacterEarningType.Gold"
            :value="total"
            :class="total < 0 ? 'text-status-danger' : 'text-status-success'"
          />

          <div
            v-else
            class="flex items-center gap-1.5 align-text-bottom font-bold text-primary"
          >
            <OIcon
              icon="experience"
              size="2xl"
            />
            <span class="leading-none">{{ $n(total) }}</span>
          </div>
        </div>
      </div>

      <VChart
        ref="chart"
        class="mb-6 h-[30rem]"
        theme="crpg"
        :option="option"
        :loading="loading"
        :loading-options="loadingOptions"
        @legendselectchanged="onLegendSelectChanged"
        @datazoom="onDataZoomChanged"
      />

      <OTable
        :data="summary"
        bordered
        narrowed
        sort-icon="chevron-up"
        sort-icon-size="xs"
      >
        <OTableColumn
          v-slot="{ row }: { row: CharacterEarnedDataWithGameMode }"
        >
          <div class="flex items-center gap-1.5 align-text-bottom font-bold">
            <OIcon :icon="gameModeToIcon[row.gameMode as GameMode]" />
            {{ $t(`game-mode.${row.gameMode}`) }}
          </div>
        </OTableColumn>

        <OTableColumn
          v-slot="{ row }: { row: CharacterEarnedDataWithGameMode }"
          field="timeEffort"
          :label="$t('character.earningStats.summary.timeEffort')"
          sortable
        >
          {{ $t('dateTimeFormat.ss', { secondes: Number(row.timeEffort) }) }}
        </OTableColumn>

        <OTableColumn
          field="experience"
          sortable
        >
          <template #header>
            <OIcon
              icon="experience"
              class="text-primary"
              size="2xl"
            />
          </template>
          <template #default="{ row }: { row: CharacterEarnedDataWithGameMode }">
            {{ $n(Number(row.experience)) }}
            ({{ $n(Number(row.experience) / Number(row.timeEffort)) }}/s)
          </template>
        </OTableColumn>

        <OTableColumn
          field="gold"
          sortable
        >
          <template #header>
            <Coin />
          </template>
          <template #default="{ row }: { row: CharacterEarnedDataWithGameMode }">
            {{ $n(Number(row.gold)) }}
            ({{ $n(Number(row.gold) / Number(row.timeEffort)) }}/s)
          </template>
        </OTableColumn>

        <template #empty>
          <ResultNotFound />
        </template>
      </OTable>
    </div>
  </div>
</template>

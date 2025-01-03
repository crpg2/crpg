<script lang="ts" setup>
// TODO: FIXME: composition + components
import type { BarSeriesOption } from 'echarts/charts'
import type {
  DataZoomComponentOption,
  GridComponentOption,
  LegendComponentOption,
  ToolboxComponentOption,
  TooltipComponentOption,
} from 'echarts/components'
import type { ComposeOption } from 'echarts/core'
import type { DurationLike } from 'luxon'

import { BarChart } from 'echarts/charts'
import {
  DataZoomComponent,
  GridComponent,
  LegendComponent,
  ToolboxComponent,
  TooltipComponent,
} from 'echarts/components'
import { registerTheme, use } from 'echarts/core'
import { SVGRenderer } from 'echarts/renderers'
import { DateTime } from 'luxon'
import VChart from 'vue-echarts'

import type { CharacterEarnedMetadata } from '~/models/activity-logs'
import type { TimeSeries } from '~/models/timeseries'

import theme from '~/assets/themes/oruga-tailwind/echart-theme.json'
import { useAsyncCallback } from '~/composables/utils/use-async-callback'
import { CharacterEarningType, getCharacterEarningStatistics } from '~/services/characters-service'
import { d, n, t } from '~/services/translate-service'
import { characterKey } from '~/symbols/character'

use([ToolboxComponent, BarChart, TooltipComponent, LegendComponent, DataZoomComponent, GridComponent, SVGRenderer])
registerTheme('crpg', theme)
type EChartsOption = ComposeOption<
  | LegendComponentOption
  | ToolboxComponentOption
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
// contains raw api response for computing statistics for game-mode summary
const { execute: loadCharacterEarningStatistics, state: rawEarningStatistics }
  = await useAsyncState(
    ({ id }: { id: number }) => getCharacterEarningStatistics(id, start.value),
    [],
    {
      immediate: false,
      resetOnExecute: false,
    },
  )

// TODO: spec
// converts raw api response to array simplified timeSeries per gamemode for echart
const characterEarningStatistics = computed(() => {
  const type = statTypeModel.value
  return rawEarningStatistics.value.reduce((out, l) => {
    const currentEl = out.find(el => el.name === t(`game-mode.${l.metadata.gameMode}`))

    if (currentEl) {
      currentEl.data.push([
        l.createdAt,
        Number.parseInt(type === CharacterEarningType.Exp ? l.metadata.experience : l.metadata.gold, 10),
      ])
    }
    else {
      out.push({
        data: [
          [
            l.createdAt,
            Number.parseInt(
              type === CharacterEarningType.Exp ? l.metadata.experience : l.metadata.gold,
              10,
            ),
          ],
        ],
        name: t(`game-mode.${l.metadata.gameMode}`),
      })
    }

    return out
  }, [] as TimeSeries[])
})

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
  toolbox: {
    show: true,
    right: 0,
    top: 0,
    feature: {
      dataView: {
        // TODO i18n
        title: 'Summary per Game-Mode',
        // TODO i18n
        lang: ['Statistics per Game-Mode', 'Close', 'Refresh'],
        optionToContent: () => convertSummaryToFormattedHtml(sumGameModeValues()),
        readOnly: true,
      },
    },
  },
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

// takes raw api-response and computes over the values per gameMode and adds them up into one earning-object
// then returns a list with one earning-object per game-mode
const sumGameModeValues = (): CharacterEarnedMetadata[] => {
  const result: CharacterEarnedMetadata[] = []
  for (const gameModeResult of rawEarningStatistics.value.map(res => res.metadata)) {
    const currentGameModeSummary = result.find(gme => gme.gameMode === gameModeResult.gameMode)

    // init game-mode if needed
    if (currentGameModeSummary === undefined) {
      result.push({
        characterId: 'we dont care here',
        gameMode: gameModeResult.gameMode,
        experience: gameModeResult.experience,
        gold: gameModeResult.gold,
        timeEffort: gameModeResult.timeEffort,
      })
      continue
    }

    // TODO: fix ugly time conversions
    currentGameModeSummary.experience = (Number.parseInt(currentGameModeSummary.experience, 10) + Number.parseInt(gameModeResult.experience, 10)).toString()
    currentGameModeSummary.gold = (Number.parseInt(currentGameModeSummary.gold, 10) + Number.parseInt(gameModeResult.gold, 10)).toString()
    currentGameModeSummary.timeEffort = (Number.parseFloat(currentGameModeSummary.timeEffort) + Number.parseFloat(gameModeResult.timeEffort)).toString()
  }
  return result
}

// convert a list of CharacterEarnedMetadata into a somewhat formatted HTML-string
const convertSummaryToFormattedHtml = (gameModeSummaries: CharacterEarnedMetadata[]) => {
  let result = ''
  for (const gameModeSummary of gameModeSummaries) {
    const experience = Number.parseInt(gameModeSummary.experience, 10)
    const gold = Number.parseInt(gameModeSummary.gold, 10)
    const seconds = Number.parseInt(gameModeSummary.timeEffort, 10)

    result += t(`game-mode.${gameModeSummary.gameMode}`)
    result += `<br> &nbsp; &nbsp; &nbsp; &nbsp;${n(experience)} Exp`
    result += `<br> &nbsp; &nbsp; &nbsp; &nbsp;${n(gold)} Gold`
    result += `<br> &nbsp; &nbsp; &nbsp; &nbsp;${n(seconds)} Seconds`
    result += `<br> &nbsp; &nbsp; &nbsp; &nbsp;${n(experience / seconds)} Exp/s`
    result += `<br> &nbsp; &nbsp; &nbsp; &nbsp;${n(gold / seconds)} Gold/s<br>`
  }
  return result
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
            :label="$t('character.earningChart.type.experience')"
          />
          <OTabItem
            :value="CharacterEarningType.Gold"
            :label="$t('character.earningChart.type.gold')"
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
        class="h-[30rem]"
        theme="crpg"
        :option="option"
        :loading="loading"
        :loading-options="loadingOptions"
        @legendselectchanged="onLegendSelectChanged"
        @datazoom="onDataZoomChanged"
      />
    </div>
  </div>
</template>

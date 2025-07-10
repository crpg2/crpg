<script setup lang="ts">
import type { SelectItem, TableColumn, TabsItem } from '@nuxt/ui'
import type { ColumnFiltersState, SortingState } from '@tanstack/table-core'

import { useRouteQuery } from '@vueuse/router'
import { CompetitiveRank, CompetitiveRankTable, UButton, UIcon, UInput, UiTableColumnHeader, UiTableColumnHeaderLabel, UModal, USelect, UserMedia, UTooltip } from '#components'
import { h, tw } from '#imports'

import type { CharacterClass } from '~/models/character'
import type { CharacterCompetitiveNumbered } from '~/models/competitive'
import type { GameMode } from '~/models/game-mode'
import type { Region } from '~/models/region'

import { CHARACTER_CLASS } from '~/models/character'
import { GAME_MODE } from '~/models/game-mode'
import { REGION } from '~/models/region'
import { characterClassToIcon, getCompetitiveValueByGameMode } from '~/services/character-service'
import { gameModeToIcon, rankedGameModes } from '~/services/game-mode-service'
import { getLeaderBoard } from '~/services/leaderboard-service'
import { useUserStore } from '~/stores/user'

definePageMeta({
  layoutOptions: {
    bg: 'background-2.webp',
  },
})

const { t } = useI18n()
const userStore = useUserStore()
const route = useRoute('leaderboard')

const gameModeModel = useRouteQuery<GameMode>('gameMode', GAME_MODE.CRPGBattle)
const characterClassModel = useRouteQuery<CharacterClass | undefined>('class', undefined)
const regionModel = useRouteQuery<Region>('region', REGION.Eu)

const {
  execute: loadLeaderBoard,
  isLoading: leaderBoardLoading,
  state: leaderboard,
} = useAsyncState(() => getLeaderBoard({
  characterClass: characterClassModel.value,
  gameMode: gameModeModel.value,
  region: regionModel.value,
}), [])

watch(
  () => route.query,
  () => loadLeaderBoard(),
)

const { rankTable } = useRankTable()

const regionItems = Object.values(REGION).map<TabsItem>(region => ({
  label: t(`region.${region}`),
  value: region,
}))

const gameModeItems = rankedGameModes.map<TabsItem>(mode => ({
  label: t(`game-mode.${mode}`),
  icon: `crpg:${gameModeToIcon[mode]}`,
  value: mode,
}))

const columnFilters = ref<ColumnFiltersState>([])

const sorting = ref<SortingState>([
  { id: 'position', desc: false },
])

const globalFilter = ref<string | undefined>(undefined)

const columns: TableColumn<CharacterCompetitiveNumbered>[] = [
  {
    accessorKey: 'position',
    enableGlobalFilter: false,
    id: 'position',
    header: ({ column }) => h(UiTableColumnHeader, {
      label: t('leaderboard.table.cols.top'),
      withSort: true,
      sorted: column.getIsSorted(),
      onSort: () => column.toggleSorting(column.getIsSorted() === 'asc'),
    }),
    meta: {
      class: {
        th: tw`w-18`,
      },
    },
  },
  {
    accessorKey: 'statistics',
    enableGlobalFilter: false,
    header: () => h(UiTableColumnHeader, {
      label: t('leaderboard.table.cols.rank'),
    }, {
      'label-trailing': () => h(UModal, {
        title: t('rankTable.title'),
        close: {
          size: 'sm',
          color: 'secondary',
          variant: 'solid',
        },
        ui: {
          content: tw`max-w-5xl`,
        },
      }, {
        default: () => h(UButton, {
          color: 'neutral',
          variant: 'ghost',
          size: 'xs',
          square: true,
          icon: 'crpg:help-circle',
        }),
        body: () => h(CompetitiveRankTable, { rankTable: rankTable.value }),
      }),
    }),
    cell: ({ row }) => h(CompetitiveRank, {
      rankTable: rankTable.value,
      competitiveValue: getCompetitiveValueByGameMode(row.original.statistics, gameModeModel.value),
    }),
    meta: {
      class: {
        th: tw`w-48`,
      },
    },
  },
  {
    accessorKey: 'user.name',
    // @ts-expect-error TODO:
    header: () => h(UInput, {
      'icon': 'crpg:search',
      'variant': 'soft',
      'size': 'xs',
      'placeholder': t('leaderboard.table.cols.player'),
      'modelValue': globalFilter.value,
      'onUpdate:modelValue': (val: string) => globalFilter.value = val,
    }),
    cell: ({ row }) => h(UserMedia, { user: row.original.user, hiddenPlatform: true }),
    meta: {
      class: {
        th: tw`max-w-96`,
      },
    },
  },
  {
    accessorKey: 'class',
    enableGlobalFilter: false,
    header: ({ column }) => {
      return h(UiTableColumnHeader, {
        label: t('leaderboard.table.cols.class'),
        withFilter: true,
        filtered: column.getIsFiltered(),
        onResetFilter: () => column.setFilterValue(undefined),
      }, {
        filter: () =>
          // @ts-expect-error TODO:
          h(USelect, {
            'variant': 'none',
            'multiple': false,
            'trailing-icon': '',
            'size': 'xl',
            'ui': {
              content: 'min-w-fit',
              base: 'px-0 py-0',
            },
            'items': Object.values(CHARACTER_CLASS).map<SelectItem>(charClass => ({
              value: charClass,
              icon: `crpg:${characterClassToIcon[charClass]}`,
              label: t(`character.class.${charClass}`),
            })),
            'modelValue': column.getFilterValue(),
            'onUpdate:modelValue': column.setFilterValue,
          }, {
            default: () => h(UiTableColumnHeaderLabel, {
              label: t('leaderboard.table.cols.class'),
              withFilter: true,
            }),
          }),
      })
    },
    cell: ({ row }) => h(UTooltip, { text: t(`character.class.${row.original.class}`) }, () => h(UIcon, {
      name: `crpg:${characterClassToIcon[row.original.class]}`,
      class: 'size-6',
    })),
    meta: {
      class: {
        th: tw`w-24`,
      },
    },
  },
  {
    accessorKey: 'level',
    enableGlobalFilter: false,
    header: ({ column }) => h(UiTableColumnHeader, {
      label: t('leaderboard.table.cols.level'),
      withSort: true,
      sorted: column.getIsSorted(),
      onSort: () => column.toggleSorting(column.getIsSorted() === 'asc'),
    }),
    meta: {
      class: {
        th: tw`w-24`,
      },
    },
  },
]
</script>

<template>
  <UContainer>
    <div
      class="
        mx-auto max-w-4xl py-8
        md:py-16
      "
    >
      <div class="mb-20">
        <div class="mb-5 flex justify-center">
          <UIcon
            name="crpg:trophy-cup"
            class="size-12 text-crpg-gold-600"
          />
        </div>
        <UiHeading :title="$t('leaderboard.title')" />
      </div>

      <div class="mb-4 flex items-center gap-6">
        <UTabs
          v-model="regionModel"
          :items="regionItems"
          size="xl"
          variant="pill"
          :content="false"
        />

        <USeparator orientation="vertical" class="h-8" />

        <UTabs
          v-model="gameModeModel"
          :items="gameModeItems"
          size="xl"
          variant="pill"
          :content="false"
        />
      </div>

      <UTable
        v-model:global-filter="globalFilter"
        v-model:column-filters="columnFilters"
        v-model:sorting="sorting"
        class="relative rounded-md border border-muted"
        :loading="leaderBoardLoading"
        :data="leaderboard"
        :columns
        :meta="{
          class: {
            tr: (row) => row.original.user.id === userStore.user?.id ? tw`text-primary` : '',
          },
        }"
      >
        <template #empty>
          <UiResultNotFound />
        </template>
      </UTable>
    </div>
  </UContainer>
</template>

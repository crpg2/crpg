<script setup lang="ts">
import type { SelectItem, TableColumn, TabsItem } from '@nuxt/ui'
import type { SortingState } from '@tanstack/table-core'

import { useRouteQuery } from '@vueuse/router'
import { CompetitiveRank, LazyCompetitiveRankTable, UButton, UIcon, UiGridColumnHeader, UiGridColumnHeaderLabel, UInput, UModal, USelect, UserMedia, UTooltip } from '#components'

import type { CharacterClass } from '~/models/character'
import type { CharacterCompetitiveNumbered } from '~/models/competitive'
import type { GameMode } from '~/models/game-mode'

import { useUser } from '~/composables/user/use-user'
import { CHARACTER_CLASS } from '~/models/character'
import { GAME_MODE } from '~/models/game-mode'
import { characterClassToIcon, getCompetitiveValueByGameMode } from '~/services/character-service'
import { gameModeToIcon, rankedGameModes } from '~/services/game-mode-service'
import { getLeaderBoard } from '~/services/leaderboard-service'

definePageMeta({
  layoutOptions: {
    bg: 'background-2.webp',
  },
})

const { t } = useI18n()
const { user } = useUser()
const route = useRoute('leaderboard')

const gameModeModel = useRouteQuery<GameMode>('gameMode', GAME_MODE.CRPGBattle)
const characterClassModel = useRouteQuery<CharacterClass | undefined>('class', undefined)
const { regionModel, regions } = useRegionQuery()

const {
  state: leaderboard,
  executeImmediate: loadLeaderBoard,
  isLoading: leaderBoardLoading,
} = useAsyncState(() => getLeaderBoard({
  characterClass: characterClassModel.value,
  gameMode: gameModeModel.value,
  region: regionModel.value,
}), [], { resetOnExecute: false })

watch(
  () => route.query,
  () => loadLeaderBoard(),
)

const { rankTable } = useRankTable()

const sorting = ref<SortingState>([
  { id: 'position', desc: false },
])

const globalFilter = ref<string>('')

const columns = computed<TableColumn<CharacterCompetitiveNumbered>[]>(() => [
  {
    accessorKey: 'position',
    enableGlobalFilter: false,
    id: 'position',
    header: ({ column }) => h(UiGridColumnHeader, {
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
    header: () => h(UiGridColumnHeader, {
      label: t('leaderboard.table.cols.rank'),
    }, {
      'label-trailing': () => h(UModal, {
        title: t('rankTable.title'),
        ui: {
          content: tw`max-w-5xl`,
        },
      }, {
        default: () => h(UButton, {
          color: 'neutral',
          variant: 'ghost',
          square: true,
          icon: 'crpg:help-circle',
        }),
        body: () => h(LazyCompetitiveRankTable, { rankTable: rankTable.value }),
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
    // @ts-expect-error TODO: https://github.com/nuxt/ui/issues/2968
    header: () => h(UInput, {
      'icon': 'crpg:search',
      'placeholder': t('leaderboard.table.cols.player'),
      'modelValue': globalFilter.value,
      'onUpdate:modelValue': (val: string) => globalFilter.value = val,
    }, {}),
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
      return h(UiGridColumnHeader, {
        label: t('leaderboard.table.cols.class'),
        withFilter: true,
        filtered: column.getIsFiltered(),
        onResetFilter: () => column.setFilterValue(undefined),
      }, {
        filter() {
          // TODO: use facets
          // @ts-expect-error TODO: https://github.com/nuxt/ui/issues/2968
          return h(USelect, {
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
            default: () => h(UiGridColumnHeaderLabel, {
              label: t('leaderboard.table.cols.class'),
              withFilter: true,
            }),
          })
        },
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
    header: ({ column }) => h(UiGridColumnHeader, {
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
])
</script>

<template>
  <UContainer>
    <div
      class="mx-auto max-w-4xl py-6"
    >
      <UiHeading class="mb-14" :title="$t('leaderboard.title')">
        <template #icon>
          <UIcon
            name="crpg:trophy-cup"
            class="size-12 text-gold"
          />
        </template>
      </UiHeading>

      <div class="mb-4 flex gap-6">
        <UTabs
          v-model="regionModel"
          :items="regions.map<TabsItem>(region => ({
            label: $t(`region.${region}`),
            value: region,
          }))"
          size="xl"
          color="neutral"
          :content="false"
        />

        <UTabs
          v-model="gameModeModel"
          :items="rankedGameModes.map<TabsItem>(mode => ({
            label: $t(`game-mode.${mode}`),
            icon: `crpg:${gameModeToIcon[mode]}`,
            value: mode,
          }))"
          size="xl"
          color="neutral"
          :content="false"
        />
      </div>

      <UTable
        v-model:global-filter="globalFilter"
        v-model:sorting="sorting"
        class="relative rounded-md border border-muted"
        :loading="leaderBoardLoading"
        :data="leaderboard"
        :columns
        :meta="{
          class: {
            tr: (row) => row.original.user.id === user?.id ? tw`text-primary` : '',
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

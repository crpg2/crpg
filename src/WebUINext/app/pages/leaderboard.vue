<script setup lang="ts">
import type { DropdownMenuItem, TableColumn, TabsItem } from '@nuxt/ui'
import type { ColumnFiltersState } from '@tanstack/table-core'

import { useRouteQuery } from '@vueuse/router'
import { CompetitiveRank, CompetitiveRankTable, UIcon, UInput, UiTableColumnHeader, UModal, UserMedia, UTooltip } from '#components'
import { tw } from '#imports'

import type { CharacterCompetitiveNumbered } from '~/models/competitive'

import { CharacterClass } from '~/models/character'
import { GameMode } from '~/models/game-mode'
import { Region } from '~/models/region'
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

const gameModeModel = useRouteQuery<GameMode>('gameMode', GameMode.Battle)
const characterClassModel = useRouteQuery<CharacterClass | undefined>('class', undefined)
const regionModel = useRouteQuery<Region>('region', Region.Eu)

function setColumnFilters(state: ColumnFiltersState) {
  if (!state.length) {
    characterClassModel.value = undefined
    return
  }
  // TODO: FIXME: шляпа
  characterClassModel.value = state[0]?.value[0] as CharacterClass
}

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
  () => loadLeaderBoard(300),
)

const { rankTable } = useRankTable()

const regionItems = Object.keys(Region).map<TabsItem>(region => ({
  label: t(`region.${region}`),
  value: region,
}))

const gameModeItems = rankedGameModes.map<TabsItem>(mode => ({
  label: t(`game-mode.${mode}`),
  icon: `crpg:${gameModeToIcon[mode]}`,
  value: mode,
}))

const columnFilters = ref<ColumnFiltersState>([])

const table = useTemplateRef('table')

const globalFilter = ref('')

const columns: TableColumn<CharacterCompetitiveNumbered>[] = [
  {
    accessorKey: 'position',
    enableGlobalFilter: false,
    id: 'position',
    header: ({ column }) => h(UiTableColumnHeader, {
      label: t('leaderboard.table.cols.rank'),
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
    header: () => h('div', {
      class: 'flex items-center gap-1',
    }, [
      t('leaderboard.table.cols.rank'),
      h(UModal, {
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
        default: () => h(UIcon, { name: 'crpg:help-circle', class: 'size-3.5 cursor-pointer flex-col text-muted hover:text-toned' }),
        body: () => h(CompetitiveRankTable, { rankTable: rankTable.value }),
      }),
    ]),
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
    header: () => h(UInput, {
      'icon': 'crpg:search',
      'variant': 'ghost',
      'size': 'xs',
      'placeholder': t('leaderboard.table.cols.player'),
      'modelValue': globalFilter.value,
      'onUpdate:modelValue': val => globalFilter.value = val,
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
      const filterValue = (column.getFilterValue() || []) as string[]
      return h(UiTableColumnHeader, {
        label: t('leaderboard.table.cols.class'),
        withFilter: true,
        filtered: column.getIsFiltered(),
        filterDropdownItems: Object.values(CharacterClass).map<DropdownMenuItem>(charClass => ({
          icon: `crpg:${characterClassToIcon[charClass]}`,
          label: `${t(`character.class.${charClass}`)}`,
          type: 'checkbox',
          checked: filterValue.includes(charClass),
          onUpdateChecked() {
            column.setFilterValue([charClass])
          },
        })),
        onResetFilter: () => column.setFilterValue(undefined),
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
    <div class="mx-auto max-w-4xl py-8 md:py-16">
      <div class="mb-20">
        <div class="mb-5 flex justify-center">
          <UIcon
            name="crpg:trophy-cup"
            class="size-12 text-more-support"
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
        ref="table"
        v-model:global-filter="globalFilter"
        class="relative rounded-md border border-muted"
        :loading="leaderBoardLoading"
        :data="leaderboard"
        :columns
        :state="{
          columnFilters,
        }"
        :meta="{
          class: {
            tr: (row) => row.original.user.id === userStore.user?.id ? tw`text-primary` : '',
          },
        }"
        @update:column-filters="setColumnFilters"
      >
        <template #empty>
          <UiResultNotFound />
        </template>
      </UTable>
    </div>
  </UContainer>
</template>

<script setup lang="ts">
import type { SelectItem, TableColumn, TabsItem } from '@nuxt/ui'
import type { ColumnFiltersState, VisibilityState } from '@tanstack/vue-table'

import { getFacetedRowModel, getFacetedUniqueValues, getPaginationRowModel } from '@tanstack/vue-table'
import { ClanTagIcon, UBadge, UButton, UiGridColumnHeader, UiGridColumnHeaderLabel, UInput, USelect, UTooltip } from '#components'
import { navigateTo, tw } from '#imports'

import type { ClanWithMemberCount } from '~/models/clan'

import { useUser } from '~/composables/user/use-user'
import { LANGUAGE } from '~/models/language'
import { SomeRole } from '~/models/role'
import { getClans } from '~/services/clan-service'

definePageMeta({
  roles: SomeRole,
})

const { clan } = useUser()
const { t } = useI18n()

const globalFilterByName = ref<string | undefined>(undefined)

// TODO: region & pagination as query params - improve API
const { state: clans, isLoading: loadingClans } = useAsyncState(() => getClans(), [])

const { regionModel, regions } = useRegionQuery()
const table = useTemplateRef('table')

// TODO: это не синхронизированно, т.к. нет onFilterCHange
const columnFilters = ref<ColumnFiltersState>([
  {
    id: 'clan_region',
    value: regionModel.value,
  },
])

const { getInitialPaginationState, pagination } = usePagination()

watch(regionModel, () => {
  columnFilters.value = [
    {
      id: 'clan_region',
      value: regionModel.value,
    },
  ]

  // TODO: на search и сортировку тоже сбрасывать? сделать обертку мб с бызовым поведением
  table.value?.tableApi.resetPagination()
})

const columns = computed<TableColumn<ClanWithMemberCount>[]>(() => [
  {
    accessorKey: 'clan.tag',
    header: t('clan.table.column.tag'),
    cell: ({ row }) => h('div', {
      class: 'flex items-center gap-2',
    }, [
      h(ClanTagIcon, { color: row.original.clan.primaryColor, class: 'size-5' }),
      h('div', row.original.clan.tag),
    ]),
    meta: {
      class: {
        th: tw`w-32`,
      },
    },
  },
  {
    accessorKey: 'clan.name',
    // @ts-expect-error TODO: https://github.com/nuxt/ui/issues/2968
    header: () => h(UInput, {
      'icon': 'crpg:search',
      'placeholder': t('clan.table.column.name'),
      'modelValue': globalFilterByName.value,
      'onUpdate:modelValue': (val: string) => globalFilterByName.value = val,
    }),
    cell: ({ row }) => h('div', {
      class: 'flex items-center gap-2',
    }, [
      h('span', row.original.clan.name),
      ...(clan.value?.id === row.original.clan.id
        ? [h('span', { 'data-aq-clan-row': 'self-clan' }, `(${t('you')})`)]
        : []),
    ]),
    meta: {
      class: {
        th: tw`w-64`,
      },
    },
  },
  {
    id: 'clan_languages',
    accessorKey: 'clan.languages',
    enableGlobalFilter: false,
    header: ({ column }) => {
      // TODO: fix facets in UTable
      // const uniqueKeys: string[] = [...new Set(Array.from(column.getFacetedUniqueValues().keys()).flat())]
      // TODO: рефакторинг UiTableColumnHeader

      return h(UiGridColumnHeader, {
        label: t('clan.table.column.languages'),
        withFilter: true,
        filtered: column.getIsFiltered(),
        onResetFilter: () => column.setFilterValue(undefined),
      }, {
        filter: () =>
          // @ts-expect-error TODO: https://github.com/nuxt/ui/issues/2968
          h(USelect, {
            'variant': 'none',
            'multiple': true,
            'trailing-icon': '',
            'size': 'xl',
            'ui': {
              content: 'min-w-fit',
              base: 'px-0 py-0', // TODO:
            },
            'items': Object.values(LANGUAGE).map<SelectItem>(l => ({
              value: l,
              label: `${t(`language.${l}`)} - ${l}`,
            })),
            'modelValue': column.getFilterValue(),
            'onUpdate:modelValue': column.setFilterValue,
          }, {
            default: () => h(UiGridColumnHeaderLabel, {
              label: t('clan.table.column.languages'),
              withFilter: true,
            }),
          }),
      })
    },
    filterFn: 'arrIncludesSome',
    cell: ({ row }) => h('div', {
      class: 'flex items-center gap-1.5',
    }, row.original.clan.languages.map(l =>
      h(UTooltip, { text: t(`language.${l}`) }, () => h(UBadge, {
        color: 'primary',
        variant: 'subtle',
        label: l,
      })))),
  },
  {
    accessorKey: 'memberCount',
    enableGlobalFilter: false,
    header: ({ column }) => h(UiGridColumnHeader, {
      label: t('clan.table.column.members'),
      withSort: true,
      sorted: column.getIsSorted(),
      onSort: () => column.toggleSorting(column.getIsSorted() === 'asc'),
    }),
    meta: {
      class: {
        th: tw`w-24 text-right`,
        td: tw`w-24 text-right`,
      },
    },
  },
  {
    id: 'clan_region',
    accessorFn: row => row.clan.region,
    enableGlobalFilter: false,
  },
])

// Hack for region filtering. Need to declare a column but not render it.
const columnVisibility = ref<VisibilityState>({
  clan_region: false,
})

const regionItems = regions.map<TabsItem>(region => ({
  label: t(`region.${region}`),
  value: region,
}))
</script>

<template>
  <UContainer>
    <div
      class="
        mx-auto max-w-4xl space-y-3 py-8
        md:py-16
      "
    >
      <div class="space-y-4">
        <div class="flex flex-wrap items-center justify-between gap-4">
          <UTabs
            v-model="regionModel"
            :items="regionItems"
            size="xl"
            color="neutral"
            :content="false"
          />

          <div class="flex items-center gap-2">
            <UButton
              v-if="clan"
              :to="{ name: 'clans-id', params: { id: clan.id } }"
              icon="crpg:member"
              variant="subtle"
              size="xl"
              :label="$t('clan.action.goToMyClan')"
              data-aq-my-clan-button
            />

            <UButton
              v-else
              :to="{ name: 'clans-create' }"
              icon="crpg:add"
              variant="subtle"
              size="xl"
              :label="$t('clan.action.create')"
              data-aq-create-clan-button
            />
          </div>
        </div>

        <UTable
          ref="table"
          v-model:column-filters="columnFilters"
          v-model:pagination="pagination"
          v-model:global-filter="globalFilterByName"
          v-model:column-visibility="columnVisibility"
          class="relative rounded-md border border-muted"
          :loading="loadingClans"
          :data="clans"
          :columns
          :meta="{
            class: {
              tr: (row) => clan?.id === row.original.clan.id ? tw`text-primary` : '',
            },
          }"
          :initial-state="{
            pagination: getInitialPaginationState(),
          }"
          :pagination-options="{
            getPaginationRowModel: getPaginationRowModel(),
          }"
          :faceted-options="{
            getFacetedRowModel: getFacetedRowModel(),
            getFacetedUniqueValues: getFacetedUniqueValues(),
          }"
          @select="(_, row) => navigateTo({ name: 'clans-id', params: { id: row.original.clan.id } })"
        >
          <template #empty>
            <UiResultNotFound />
          </template>
        </UTable>

        <UiGridPagination
          v-if="table?.tableApi"
          :table-api="toRef(() => table!.tableApi)"
        />
      </div>
    </div>
  </UContainer>
</template>

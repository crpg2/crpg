<script setup lang="ts">
import type { SelectItem, TableColumn, TabsItem } from '@nuxt/ui'
import type { ColumnFiltersState, VisibilityState } from '@tanstack/vue-table'

import { getFacetedRowModel, getFacetedUniqueValues, getPaginationRowModel } from '@tanstack/vue-table'

import type { ClanWithMemberCount } from '~/models/clan'

import { ClanTagIcon, UBadge, UButton, UiGridColumnHeader, UiGridColumnHeaderSelectFilter, UInput, UTooltip } from '#components'
import { navigateTo, tw } from '#imports'
import { useUser } from '~/composables/user/use-user'
import { SomeRole } from '~/models/role'
import { getClans } from '~/services/clan-service'

definePageMeta({
  roles: SomeRole,
})

const { clan } = useUser()
const { t } = useI18n()

const globalFilterByName = ref<string | undefined>(undefined)

const { state: clans, isLoading: loadingClans } = useAsyncState(() => getClans(), [])

const { regionModel, regions } = useRegionQuery()
const table = useTemplateRef('table')

const columnFilters = ref<ColumnFiltersState>([
  {
    id: 'clan_region',
    value: regionModel.value,
  },
])

watch(regionModel, () => {
  columnFilters.value = [
    {
      id: 'clan_region',
      value: regionModel.value,
    },
  ]
  table.value?.tableApi.resetPagination()
})

const { getInitialPaginationState, pagination } = usePagination()

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
      const uniqueKeys: string[] = [...new Set(Array.from(column.getFacetedUniqueValues().keys()).flat())]
      return h(UiGridColumnHeader, {
        label: t('clan.table.column.languages'),
        withFilter: true,
        filtered: column.getIsFiltered(),
        onResetFilter: () => column.setFilterValue(undefined),
      }, {
        filter: () =>
          h(UiGridColumnHeaderSelectFilter, {
            'label': t('clan.table.column.languages'),
            'items': uniqueKeys.map<SelectItem>(l => ({
              value: l,
              label: `${t(`language.${l}`)} - ${l}`,
            })),
            'modelValue': column.getFilterValue(),
            'onUpdate:modelValue': column.setFilterValue,
          }),
      })
    },
    filterFn: 'arrIncludesSome',
    cell: ({ row }) => h('div', {
      class: 'flex items-center gap-1.5',
    }, row.original.clan.languages.map(l =>
      h(UTooltip, {
        text: t(`language.${l}`),
      }, () => h(UBadge, {
        color: 'primary',
        variant: 'subtle',
        label: l,
      })))),
    meta: {
      class: {
        td: tw`py-1`,
      },
    },
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
  label: t(`region.${region}`, 0),
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
          :page="pagination.pageIndex + 1"
          :size="pagination.pageSize"
          :total="table?.tableApi.getFilteredRowModel().rows.length ?? 0"
          @update:page="(page) => table?.tableApi.setPageIndex(page - 1)"
        />
      </div>
    </div>
  </UContainer>
</template>

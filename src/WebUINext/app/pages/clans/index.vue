<script setup lang="ts">
import type { SelectItem, TableColumn, TabsItem } from '@nuxt/ui'
import type { ColumnFiltersState, PaginationState, VisibilityState } from '@tanstack/vue-table'

import { getFacetedRowModel, getFacetedUniqueValues, getPaginationRowModel } from '@tanstack/vue-table'
import { ClanTagIcon, UBadge, UButton, UInput, UiTableColumnHeader, UiTableColumnHeaderLabel, USelect, UTooltip } from '#components'
import { navigateTo, tw } from '#imports'

import type { ClanWithMemberCount } from '~/models/clan'

import { useRegionQuery } from '~/composables/use-region'
import { REGION } from '~/models/region'
import { SomeRole } from '~/models/role'
import { getClans } from '~/services/clan-service'
import { useUserStore } from '~/stores/user'

definePageMeta({
  roles: SomeRole,
})

const userStore = useUserStore()
const { t } = useI18n()

const globalFilterByName = useRouteQuery<string | undefined>('searchModel', undefined)

// TODO: region as query, pagination - improve REST API
const {
  state: clans,
  execute: loadClans,
  isLoading: loadingClans,
} = useAsyncState(() => getClans(), [])

const { regionModel, regions } = useRegionQuery()
const table = useTemplateRef('table')

const columnFilters = ref<ColumnFiltersState>([
  {
    id: 'clan_region',
    value: REGION.Eu,
  },
])

function getInitialPaginationState(): PaginationState {
  return {
    pageIndex: 0,
    pageSize: 10, // TODO: FIXME:
  }
}

const pagination = ref<PaginationState>(getInitialPaginationState())

watch(regionModel, () => {
  table.value?.tableApi.setColumnFilters([
    {
      id: 'clan_region',
      value: regionModel.value,
    },
  ])

  // TODO: на search и сортировку тоже сбрасывать? сделать обертку мб с бызовым поведением
  table.value?.tableApi.resetPagination()
})

// TODO: to cmp
const columns = computed<TableColumn<ClanWithMemberCount>[]>(() => [
  {
    accessorKey: 'clan.tag',
    header: t('clan.table.column.tag'),
    cell: ({ row }) => h('div', {
      class: 'flex items-center gap-2',
    }, [
      h(ClanTagIcon, { color: row.original.clan.primaryColor }),
      h('div', row.original.clan.tag),
    ]),
    meta: {
      class: {
        td: tw`w-32`,
      },
    },
  },
  {
    accessorKey: 'clan.name',
    // @ts-expect-error TODO:
    header: () => h(UInput, {
      'icon': 'crpg:search',
      'variant': 'soft',
      'size': 'xs',
      'placeholder': t('clan.table.column.name'),
      'modelValue': globalFilterByName.value,
      'onUpdate:modelValue': (val: string) => globalFilterByName.value = val,
    }),
    cell: ({ row }) => h('div', {
      class: 'flex items-center gap-2',
    }, [
      h('span', row.original.clan.name),
      ...(userStore.clan?.id === row.original.clan.id
        ? [h('span', { 'data-aq-clan-row': 'self-clan' }, `(${t('you')})`)]
        : []),
    ]),
    meta: {
      class: {
        td: tw`w-64`,
      },
    },
  },
  {
    id: 'clan_languages',
    accessorKey: 'clan.languages',
    enableGlobalFilter: false,
    header: ({ column }) => {
      const uniqueKeys: string[] = [...new Set(Array.from(column.getFacetedUniqueValues().keys()).flat())]
      return h(UiTableColumnHeader, {
        label: t('clan.table.column.languages'),
        withFilter: true,
        filtered: column.getIsFiltered(),
        onResetFilter: () => column.setFilterValue(undefined),
      }, {
        filter: () =>
          // @ts-expect-error TODO:
          h(USelect, {
            'variant': 'none',
            'multiple': true,
            'trailing-icon': '',
            'size': 'xl',
            'ui': {
              content: 'min-w-fit',
              base: 'px-0 py-0',
            },
            'items': uniqueKeys.map<SelectItem>(l => ({
              value: l,
              label: `${t(`language.${l}`)} - ${l}`,
            })),
            'modelValue': column.getFilterValue(),
            'onUpdate:modelValue': column.setFilterValue,
          }, {
            default: () => h(UiTableColumnHeaderLabel, {
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
        variant: 'soft',
        size: 'sm',
        label: l,
      })))),
  },
  {
    accessorKey: 'memberCount',
    enableGlobalFilter: false,
    header: ({ column }) => h(UiTableColumnHeader, {
      label: t('clan.table.column.members'),
      withSort: true,
      sorted: column.getIsSorted(),
      onSort: () => column.toggleSorting(column.getIsSorted() === 'asc'),
    }),
    meta: {
      class: {
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
            size="md"
            variant="pill"
            :content="false"
          />

          <div class="flex items-center gap-2">
            <UTooltip
              v-if="userStore.clan"
              :text="$t('clan.action.goToMyClan')"
            >
              <UButton
                :to="{ name: 'clans-id', params: { id: userStore.clan.id } }"
                icon="crpg:member"
                variant="subtle"
                data-aq-my-clan-button
              />
            </UTooltip>

            <UTooltip
              v-else
              :text="$t('clan.action.create')"
            >
              <UButton
                :to="{ name: 'clans-create' }"
                icon="crpg:add"
                color="secondary"
                data-aq-create-clan-button
              />
            </UTooltip>
          </div>
        </div>

        <UTable
          ref="table"
          v-model:pagination="pagination"
          v-model:column-filters="columnFilters"
          v-model:global-filter="globalFilterByName"
          v-model:column-visibility="columnVisibility"
          class="relative rounded-md border border-muted"
          :loading="loadingClans"
          :data="clans"
          :columns
          :meta="{
            class: {
              tr: (row) => userStore.clan?.id === row.original.clan.id ? tw`text-primary` : '',
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
          @select="(row) => navigateTo({ name: 'clans-id', params: { id: row.original.clan.id } })"
        >
          <template #empty>
            <UiResultNotFound />
          </template>
        </UTable>

        <UPagination
          v-if="table?.tableApi.getCanNextPage() || table?.tableApi.getCanPreviousPage()"
          class="flex justify-center"
          variant="soft"
          color="secondary"
          active-variant="solid"
          active-color="primary"
          :page="pagination.pageIndex + 1"
          :show-controls="false"
          :default-page="(table?.tableApi?.initialState.pagination.pageIndex || 0) + 1"
          :items-per-page="table?.tableApi?.initialState.pagination.pageSize"
          :total="table?.tableApi?.getFilteredRowModel().rows.length"
          @update:page="(p) => table?.tableApi?.setPageIndex(p - 1)"
        />
      </div>
    </div>
  </UContainer>
</template>

<script setup lang="ts">
import type { DropdownMenuItem, TableColumn, TabsItem } from '@nuxt/ui'
import type { ColumnFiltersState, PaginationState, VisibilityState } from '@tanstack/vue-table'

import { functionalUpdate, getFacetedRowModel, getFacetedUniqueValues, getPaginationRowModel } from '@tanstack/vue-table'
import { ClanTagIcon, UBadge, UButton, UDropdownMenu } from '#components'
import { navigateTo } from '#imports'
import { h } from 'vue'

import type { ClanWithMemberCount } from '~/models/clan'

import { useLanguages } from '~/composables/use-language'
import { useRegionQuery } from '~/composables/use-region'
import { usePagination } from '~/composables/utils/use-pagination'
import { useSearchDebounced } from '~/composables/utils/use-search-debounce'
import { Region } from '~/models/region'
import { SomeRole } from '~/models/role'
import { getClans, getFilteredClans } from '~/services/clan-service'
import { useUserStore } from '~/stores/user'

definePageMeta({
  roles: SomeRole,
})

const router = useRouter()
const userStore = useUserStore()
const { t } = useI18n()

const { pageModel, perPage } = usePagination()
const { searchModel } = useSearchDebounced()

// TODO: region as query, pagination - improve REST API
const {
  state: clans,
  execute: loadClans,
  isLoading: loadingClans,
} = useAsyncState(() => getClans(), [])

const { regionModel, regions } = useRegionQuery()
const { languages, languagesModel } = useLanguages()
const table = useTemplateRef('table')

const columnVisibility = ref<VisibilityState>({
  clan_region: false,
})

const columnFilters = ref<ColumnFiltersState>([
  {
    id: 'clan_region',
    value: Region.Eu,
  },
])

function getInitialPaginationState(): PaginationState {
  return { pageIndex: 0, pageSize: 2 }
}

const pagination = ref<PaginationState>(getInitialPaginationState())

watch(regionModel, () => {
  table.value?.tableApi.setColumnFilters([
    {
      id: 'clan_region',
      value: regionModel.value,
    },
  ])

  // TODO:
  table.value?.tableApi.resetPagination()
})

const filteredClans = computed(() =>
  getFilteredClans(clans.value, regionModel.value, languagesModel.value, searchModel.value),
)

const aggregatedLanguages = computed(() =>
  languages.filter(l =>
    clans.value
      .filter(c => c.clan.region === regionModel.value)
      .some(c => c.clan.languages.includes(l))),
)

const rowClass = (clan: ClanWithMemberCount) =>
  userStore.clan?.id === clan.clan.id ? tw`text-primary` : tw`text-content-100`

const onClickRow = (clan: ClanWithMemberCount) =>
  router.push({ name: 'clans-id', params: { id: clan.clan.id } })

const columns: TableColumn<ClanWithMemberCount>[] = [
  {
    accessorFn: row => row.clan.tag,
    meta: {
      class: {
        td: tw`w-32`,
      },
    },
    header: t('clan.table.column.tag'),
    cell: ({ row, column, getValue }) => h('div', {
      class: 'flex items-center gap-2',
    }, [
      h(ClanTagIcon, { color: row.original.clan.primaryColor }),
      h('div', row.original.clan.tag),
    ]),
  },
  {
    accessorFn: row => row.clan.name,
    meta: {
      class: {
        td: tw`w-64`,
      },
    },
    header: t('clan.table.column.name'),
    cell: ({ row, column, getValue }) => h('div', {
      class: 'flex items-center gap-2',
    }, [
      h('span', row.original.clan.name),
      ...(userStore.clan?.id === row.original.clan.id
        ? [
            h('span', { 'data-aq-clan-row': 'self-clan' }, `(${t('you')})`),
          ]
        : []),
    ]),
  },
  {
    id: 'clan_languages',
    accessorKey: 'clan.languages',
    enableGlobalFilter: false,
    header: ({ column }) => {
      const isFiltered = column.getIsFiltered()
      const filterValue = (column.getFilterValue() || []) as string[]

      const uniqueKeys: string[] = [...new Set(Array.from(column.getFacetedUniqueValues().keys()).flat())]

      const items = uniqueKeys.map(l => ({
        label: `${t(`language.${l}`)} - ${l}`,
        type: 'checkbox' as const,
        checked: filterValue.includes(l),
        onSelect(e: Event) {
          e.preventDefault()
        },
        onUpdateChecked() {
          column.setFilterValue(toggle(filterValue, l))
        },
      })) satisfies DropdownMenuItem[]

      // @ts-expect-error TODO: FIXME:
      return h(UDropdownMenu, {
        modal: false,
        items,
      }, () => h(UButton, {
        color: 'neutral',
        variant: 'ghost',
        label: t('clan.table.column.languages'),
        class: '-mx-2.5',
        icon: isFiltered ? 'i-lucide-funnel-x' : 'i-lucide-funnel',
      }))
    },
    filterFn: 'arrIncludesSome',
    cell: ({ row, column, getValue }) => h('div', {
      class: 'flex items-center gap-1.5',
    }, row.original.clan.languages.map(l => h(UBadge, {
      color: 'primary',
      variant: 'soft',
      size: 'sm',
      label: l,
    }))),
  },
  {
    accessorKey: 'memberCount',
    enableGlobalFilter: false,
    header: ({ column }) => {
      const isSorted = column.getIsSorted()
      // TODO: to cmp
      return h(UButton, {
        color: 'neutral',
        variant: 'ghost',
        label: t('clan.table.column.members'),
        // TODO:
        icon: isSorted
          ? isSorted === 'asc'
            ? 'i-lucide-arrow-up-narrow-wide'
            : 'i-lucide-arrow-down-wide-narrow'
          : 'i-lucide-arrow-up-down',
        class: '-mx-2.5',
        onClick: () => column.toggleSorting(column.getIsSorted() === 'asc'),
      })
    },
  },
  {
    id: 'clan_region',
    accessorFn: row => row.clan.region,
    enableGlobalFilter: false,
  },
]

const regionItems = regions.map<TabsItem>(region => ({
  label: t(`region.${region}`),
  value: region,
}))
</script>

<template>
  <div class="container">
    <div class="mx-auto max-w-4xl py-8 md:py-16">
      <div class="space-y-3">
        <div class="flex flex-wrap items-center justify-between gap-4">
          <UTabs
            v-model="regionModel"
            :items="regionItems"
            size="xl"
            variant="pill"
            :content="false"
          />

          <div class="flex items-center gap-2">
            <UInput
              v-model="searchModel"
              color="secondary"
              variant="outline"
              size="md"
              :placeholder="$t('action.search')"
              icon="crpg:search"
              data-aq-search-clan-input
            />

            <UTooltip
              v-if="userStore.clan"
              :text="$t('clan.action.goToMyClan')"
            >
              <UButton
                :to="{ name: 'clans-id', params: { id: userStore.clan.id } }"
                icon="crpg:member"
                color="secondary"
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
          v-model:global-filter="searchModel"
          v-model:column-visibility="columnVisibility"
          class="relative rounded-md border border-muted"
          :loading="loadingClans"
          :data="clans"
          :columns
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
          @select="(row) => {
            navigateTo({ name: 'clans-id', params: { id: row.original.clan.id } })
          }"
        >
          <template #empty>
            <UiResultNotFound />
          </template>
        </UTable>

        <div class="flex justify-center">
          <UPagination
            variant="soft"
            color="secondary"
            active-variant="solid"
            active-color="primary"
            show-edges
            :page="pagination.pageIndex + 1"
            :show-controls="false"
            :default-page="(table?.tableApi?.getState().pagination.pageIndex || 0) + 1"
            :items-per-page="table?.tableApi?.getState().pagination.pageSize"
            :total="table?.tableApi?.getFilteredRowModel().rows.length"
            @update:page="(p) => table?.tableApi?.setPageIndex(p - 1)"
          />
        </div>
      </div>

      <pre>{{ table?.tableApi?.getState() }}</pre>

      <div class="mb-6 flex flex-wrap items-center justify-between gap-4">
        <OTabs
          v-model="regionModel"
          content-class="hidden"
        >
          <OTabItem
            v-for="region in regions"
            :key="region"
            :label="$t(`region.${region}`, 0)"
            :value="region"
          />
        </OTabs>

        <div class="flex items-center gap-2">
          <div class="w-44">
            <OInput
              v-model="searchModel"
              type="text"
              expanded
              clearable
              :placeholder="$t('action.search')"
              icon="search"
              rounded
              size="sm"
              data-aq-search-clan-input
            />
          </div>

          <NuxtLink
            v-if="userStore.clan"
            :to="{ name: 'clans-id', params: { id: userStore.clan.id } }"
          >
            <OButton
              v-tooltip.bottom="$t('clan.action.goToMyClan')"
              data-aq-my-clan-button
              rounded
              icon-left="member"
              size="sm"
              variant="secondary"
              data-aq-to-clan-button
            />
          </NuxtLink>

          <NuxtLink
            v-else
            :to="{ name: 'clans-create' }"
          >
            <OButton
              v-tooltip.bottom="$t('clan.action.create')"
              rounded
              icon-left="add"
              variant="secondary"
              size="sm"
              data-aq-create-clan-button
            />
          </NuxtLink>
        </div>
      </div>

      <OTable
        v-model:current-page="pageModel"
        :data="filteredClans"
        :per-page="perPage"
        :paginated="filteredClans.length > perPage"
        hoverable
        bordered
        sort-icon="chevron-up"
        sort-icon-size="xs"
        :default-sort="['memberCount', 'desc']"
        :row-class="rowClass"
        @click="onClickRow"
      >
        <OTableColumn
          v-slot="{ row: clan }: { row: ClanWithMemberCount }"
          field="clan.tag"
          :label="$t('clan.table.column.tag')"
          :width="120"
        >
          <div class="flex items-center gap-2">
            <ClanTagIcon :color="clan.clan.primaryColor" />
            {{ clan.clan.tag }}
          </div>
        </OTableColumn>

        <OTableColumn
          v-slot="{ row: clan }: { row: ClanWithMemberCount }"
          field="clan.name"
          :label="$t('clan.table.column.name')"
        >
          {{ clan.clan.name }}
          <span
            v-if="userStore.clan?.id === clan.clan.id"
            data-aq-clan-row="self-clan"
          >
            ({{ $t('you') }})
          </span>
        </OTableColumn>

        <OTableColumn
          field="clan.languages"
          :width="220"
        >
          <template #header>
            <UiTHDropdown
              :label="$t('clan.table.column.languages')"
              :shown-reset="Boolean(languagesModel.length)"
              @reset="languagesModel = []"
            >
              <UiDropdownItem
                v-for="l in aggregatedLanguages"
                :key="l"
              >
                <OCheckbox
                  v-model="languagesModel"
                  :native-value="l"
                  class="items-center"
                  :label="`${$t(`language.${l}`)} - ${l}`"
                />
              </UiDropdownItem>
            </UiTHDropdown>
          </template>

          <template #default="{ row: clan }: { row: ClanWithMemberCount }">
            <div class="flex items-center gap-1.5">
              <UiTag
                v-for="l in clan.clan.languages"
                :key="l"
                v-tooltip="$t(`language.${l}`)"
                :label="l"
                variant="primary"
              />
            </div>
          </template>
        </OTableColumn>

        <OTableColumn
          v-slot="{ row: clan }: { row: ClanWithMemberCount }"
          field="memberCount"
          :label="$t('clan.table.column.members')"
          :width="40"
          position="right"
          numeric
          sortable
        >
          {{ clan.memberCount }}
        </OTableColumn>

        <template #empty>
          <UiResultNotFound />
        </template>
      </OTable>
    </div>
  </div>
</template>

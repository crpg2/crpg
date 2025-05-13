<script setup lang="ts">
import type { DropdownMenuItem, TableColumn } from '@nuxt/ui'
import type { ColumnFiltersState, PaginationState } from '@tanstack/vue-table'

import { functionalUpdate, getFacetedRowModel, getFacetedUniqueValues, getPaginationRowModel } from '@tanstack/vue-table'
import { ClanTagIcon, UBadge, UButton, UDropdownMenu } from '#components'
import { h } from 'vue'

import type { ClanWithMemberCount } from '~/models/clan'

import { useLanguages } from '~/composables/use-language'
import { useRegionQuery } from '~/composables/use-region'
import { usePagination } from '~/composables/utils/use-pagination'
import { useSearchDebounced } from '~/composables/utils/use-search-debounce'
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
const { state: clans, execute: loadClans } = useAsyncState(() => getClans(), [])

const { regionModel, regions } = useRegionQuery()
const { languages, languagesModel, resetLanguagesModel } = useLanguages()

watch(regionModel, resetLanguagesModel)

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
  userStore.clan?.id === clan.clan.id ? 'text-primary' : 'text-content-100'

const onClickRow = (clan: ClanWithMemberCount) =>
  router.push({ name: 'clans-id', params: { id: clan.clan.id } })

const columns: TableColumn<ClanWithMemberCount>[] = [
  {
    accessorFn: row => row.clan.tag,
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
    // :label="`${$t(`language.${l}`)} - ${l}`"
    accessorKey: 'clan.languages',
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
    header: ({ column }) => {
      const isSorted = column.getIsSorted()
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
]

const pagination = ref<PaginationState>({
  pageIndex: 0,
  pageSize: 2,
})

const columnFilters = ref<ColumnFiltersState>([])

const table = useTemplateRef('table')
</script>

<template>
  <div class="container">
    <div class="mx-auto max-w-4xl py-8 md:py-16">
      <div>
        <!-- TODO: wrapper -->
        <UTable
          ref="table"
          v-model:pagination="pagination"
          v-model:column-filters="columnFilters"
          :data="clans"
          :columns
          :pagination-options="{
            getPaginationRowModel: getPaginationRowModel(),
          }"
          :faceted-options="{
            getFacetedRowModel: getFacetedRowModel(),
            getFacetedUniqueValues: getFacetedUniqueValues(),
          }"
        />

        <pre>{{ table?.tableApi?.getState() }}</pre>

        <div class="flex justify-center border-t border-default pt-4">
          <UPagination
            variant="soft"
            color="secondary"
            active-variant="solid"
            active-color="primary"
            show-edges
            :show-controls="false"
            :default-page="(table?.tableApi?.getState().pagination.pageIndex || 0) + 1"
            :items-per-page="table?.tableApi?.getState().pagination.pageSize"
            :total="table?.tableApi?.getFilteredRowModel().rows.length"
            @update:page="(p) => table?.tableApi?.setPageIndex(p - 1)"
          />
        </div>
      </div>

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

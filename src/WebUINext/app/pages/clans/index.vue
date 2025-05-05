<script setup lang="ts">
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

const { pageModel, perPage } = usePagination()
const { searchModel } = useSearchDebounced()

// TODO: region as query, pagination - improve REST API
const { execute: loadClans, state: clans } = useAsyncState(() => getClans(), [], {
  immediate: true,
})

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
</script>

<template>
  <div class="container">
    <div class="mx-auto max-w-4xl py-8 md:py-16">
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

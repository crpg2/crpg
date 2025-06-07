<script setup lang="ts">
import type { UserRestrictionWithActive } from '~/models/user'

import { usePagination } from '~/composables/utils/use-pagination'
import { computeLeftMs, parseTimestamp } from '~/utils/date'

defineProps<{
  restrictions: UserRestrictionWithActive[]
  hiddenCols?: string[]
}>()

const { pageModel, perPage } = usePagination()
</script>

<template>
  <OTable
    v-model:current-page="pageModel"
    :data="restrictions"
    :per-page="perPage"
    :paginated="restrictions.length > perPage"
    hoverable
    bordered
    narrowed
    :debounce-search="300"
    sort-icon="chevron-up"
    sort-icon-size="xs"
    :default-sort="['id', 'desc']"
  >
    <OTableColumn
      v-slot="{ row: restriction }: { row: UserRestrictionWithActive }"
      field="id"
      :width="60"
      label="id"
      sortable
    >
      {{ restriction.id }}
    </OTableColumn>
    <OTableColumn
      v-slot="{ row: restriction }: { row: UserRestrictionWithActive }"
      field="active"
      :label="$t('restriction.table.column.status')"
      :width="90"
      sortable
    >
      <UiTag
        v-if="restriction.active"
        v-tooltip="$t('dateTimeFormat.dd:hh:mm', parseTimestamp(computeLeftMs(restriction.createdAt, Number(restriction.duration))))"
        :label="$t('restriction.status.active')"
        variant="success"
        size="sm"
      />
      <UiTag
        v-else
        variant="info"
        size="sm"
        disabled
        :label="$t('restriction.status.inactive')"
      />
    </OTableColumn>

    <OTableColumn
      v-if="!hiddenCols?.includes('restrictedUser')"
      field="restrictedUser.name"
      :label="$t('restriction.table.column.user')"
      searchable
    >
      <template #searchable="props">
        <OInput
          v-model="props.filters[props.column.field]"
          :placeholder="$t('action.search')"
          icon="search"
          class="w-40"
          size="xs"
          clearable
        />
      </template>

      <template #default="{ row: restriction }: { row: UserRestrictionWithActive }">
        <NuxtLink
          :to="{ name: 'moderator-user-id-restrictions', params: { id: restriction.restrictedUser.id } }"
          class="inline-block hover:text-content-100"
        >
          <UserMedia
            class="max-w-48"
            :user="restriction.restrictedUser"
            hidden-clan
          />
        </NuxtLink>
      </template>
    </OTableColumn>

    <OTableColumn
      v-slot="{ row: restriction }: { row: UserRestrictionWithActive }"
      field="createdAt"
      :label="$t('restriction.table.column.createdAt')"
      :width="160"
      sortable
    >
      {{ $d(restriction.createdAt, 'short') }}
    </OTableColumn>

    <OTableColumn
      v-slot="{ row: restriction }: { row: UserRestrictionWithActive }"
      field="duration"
      :label="$t('restriction.table.column.duration')"
      :width="160"
    >
      {{ $t('dateTimeFormat.dd:hh:mm', parseTimestamp(Number(restriction.duration))) }}
    </OTableColumn>

    <OTableColumn
      v-slot="{ row: restriction }: { row: UserRestrictionWithActive }"
      field="type"
      :label="$t('restriction.table.column.type')"
      :width="60"
    >
      {{ $t(`restriction.type.${restriction.type}`) }}
    </OTableColumn>

    <OTableColumn
      v-slot="{ row: restriction }: { row: UserRestrictionWithActive }"
      field="reason"
      :label="$t('restriction.table.column.reason')"
    >
      <UiCollapsibleText :text="restriction.reason" />
    </OTableColumn>

    <OTableColumn
      v-slot="{ row: restriction }: { row: UserRestrictionWithActive }"
      field="publicReason"
      :label="$t('restriction.table.column.publicReason')"
    >
      <UiCollapsibleText :text="restriction.publicReason" />
    </OTableColumn>

    <OTableColumn
      v-slot="{ row: restriction }: { row: UserRestrictionWithActive }"
      field="restrictedByUser.name"
      :label="$t('restriction.table.column.restrictedBy')"
      :width="200"
    >
      <UserMedia
        :user="restriction.restrictedByUser"
        class="max-w-48"
        hidden-clan
      />
    </OTableColumn>

    <template #empty>
      <UiResultNotFound />
    </template>
  </OTable>
</template>

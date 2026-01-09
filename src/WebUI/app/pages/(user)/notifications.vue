<script setup lang="ts">
import type { TableColumn } from '@nuxt/ui'
import type { ColumnFiltersState } from '@tanstack/vue-table'

import { functionalUpdate, getCoreRowModel, getFacetedRowModel, getFacetedUniqueValues, getFilteredRowModel, getPaginationRowModel, useVueTable } from '@tanstack/vue-table'

import type { Notification, NotificationState, NotificationType } from '~/models/notifications'

import { useUsersNotifications } from '~/composables/user/use-user-notifications'
import { NOTIFICATION_STATE } from '~/models/notifications'
import { SomeRole } from '~/models/role'

definePageMeta({
  roles: SomeRole,
})

const {
  notifications,
  notificationsDict,
  isLoading,
  hasUnreadNotifications,
  readNotification,
  readAllNotifications,
  deleteNotification,
  deleteAllNotifications,
} = useUsersNotifications()

const { pagination, setPagination } = usePagination({ pageSize: 15 })

const columns: TableColumn<Notification>[] = [
  {
    accessorFn: row => row.id,
    id: 'id',
  },
  {
    accessorFn: row => row.type,
    id: 'types',
    filterFn: 'arrIncludesSome',
  },
  {
    accessorFn: row => row.state,
    id: 'state',
    filterFn: 'arrIncludesAll',
  },
]

const scrollToTop = () => window.scrollTo({ top: 0, behavior: 'smooth' })

const onlyStateUnread = ref<boolean>(false)
const types = ref<NotificationType[]>([])

const columnFilters = computed<ColumnFiltersState>(() => [
  ...(types.value.length ? [{ id: 'types', value: types.value }] : []),
  ...(onlyStateUnread.value ? [{ id: 'state', value: [NOTIFICATION_STATE.Unread] }] : []),
])

const grid = useVueTable({
  get data() {
    return notifications
  },
  columns,
  getCoreRowModel: getCoreRowModel(),
  getFilteredRowModel: getFilteredRowModel(),
  getFacetedRowModel: getFacetedRowModel(),
  getFacetedUniqueValues: getFacetedUniqueValues(),
  filterFns: {
    includesSome,
  },
  state: {
    get columnFilters() {
      return columnFilters.value
    },
    get pagination() {
      return pagination.value
    },
  },
  getPaginationRowModel: getPaginationRowModel(),
  onPaginationChange: (updater) => {
    setPagination(functionalUpdate(updater, pagination.value))
    scrollToTop()
  },
})

const aggregatedTypes = computed(() => {
  const column = grid.getColumn('types')
  if (!column) {
    return []
  }
  return [...new Set(Array.from(column.getFacetedUniqueValues().keys()).flat())]
})

const isEmpty = computed(() => !grid.getRowCount())
</script>

<template>
  <UContainer>
    <div class="mx-auto max-w-2xl py-12">
      <UiHeading :title="$t('user.notifications.title')" class="mb-14 text-center" />

      <div class="mb-4 flex justify-between gap-4">
        <div class="flex items-center gap-4">
          <USelectMenu
            v-model="types"
            class="w-48"
            color="neutral"
            variant="subtle"
            placeholder="By type"
            multiple
            :items="aggregatedTypes"
            :ui="{
              content: 'w-auto',
            }"
          />

          <USwitch
            v-model="onlyStateUnread"
            variant="card"
            label="Only Unread"
          />
        </div>

        <div v-if="!isEmpty" class="flex gap-4">
          <UButton
            :disabled="!hasUnreadNotifications"
            variant="ghost"
            color="neutral"
            :label="$t('user.notifications.action.readAll.title')"
            @click="readAllNotifications"
          />

          <AppConfirmActionPopover
            :confirm-label="$t('action.ok')"
            :label="$t('user.notifications.action.deleteAll.confirmTitle')"
            @confirm="deleteAllNotifications"
          >
            <UButton
              variant="ghost"
              color="error"
              icon="crpg:close"
              :label="$t('user.notifications.action.deleteAll.title')"
            />
          </AppConfirmActionPopover>
        </div>
      </div>

      <div class="relative flex flex-col flex-wrap gap-4">
        <UiLoading :active="isLoading" />

        <UserNotificationCard
          v-for="item in grid.getRowModel().rows"
          :key="item.id"
          :notification="item.original"
          :dict="notificationsDict"
          @read="readNotification(item.original.id)"
          @delete="deleteNotification(item.original.id)"
        />

        <UiCard v-if="!isLoading && isEmpty">
          <UiResultNotFound />
        </UiCard>

        <UiGridPagination
          v-if="!isEmpty"
          :table-api="toRef(() => grid)"
        />
      </div>
    </div>
  </UContainer>
</template>

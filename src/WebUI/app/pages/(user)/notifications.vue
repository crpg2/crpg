<script setup lang="ts">
import type { TableColumn } from '@nuxt/ui'
import type { ColumnFiltersState } from '@tanstack/vue-table'

import { functionalUpdate, getCoreRowModel, getFacetedRowModel, getFacetedUniqueValues, getFilteredRowModel, getPaginationRowModel, useVueTable } from '@tanstack/vue-table'

import type { Notification } from '~/models/notifications'

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

const { pagination, setPagination } = usePagination({ pageSize: 5 })

const { t } = useI18n()

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
const typesModel = ref<NotificationTypeItemOption[]>([])
interface NotificationTypeItemOption {
  id: string
  label: string
}

const columnFilters = computed<ColumnFiltersState>(() => [
  ...(typesModel.value.length ? [{ id: 'types', value: typesModel.value.map(item => item.id) }] : []),
  ...(onlyStateUnread.value ? [{ id: 'state', value: [NOTIFICATION_STATE.Unread] }] : []),
])

const grid = useVueTable({
  get data() {
    return notifications.value
  },
  columns,
  getCoreRowModel: getCoreRowModel(),
  getFilteredRowModel: getFilteredRowModel(),
  getFacetedRowModel: getFacetedRowModel(),
  getFacetedUniqueValues: getFacetedUniqueValues(),
  getPaginationRowModel: getPaginationRowModel(),
  filterFns: {
    includesSome,
  },
  autoResetPageIndex: false,
  state: {
    get columnFilters() {
      return columnFilters.value
    },
    get pagination() {
      return pagination.value
    },
  },
  onPaginationChange: (updater) => {
    setPagination(functionalUpdate(updater, pagination.value))
    scrollToTop()
  },
})

function fixPaginationAfterDelete() {
  /**
   * autoResetPageIndex disabled
   * for better UX when deleting notifications
   * by default, any change in data resets pagination to default
   */
  const totalRows = grid.getFilteredRowModel().rows.length
  const pageSize = pagination.value.pageSize

  const maxPageIndex = Math.max(0, Math.ceil(totalRows / pageSize) - 1)

  if (pagination.value.pageIndex > maxPageIndex) {
    setPagination({
      ...pagination.value,
      pageIndex: maxPageIndex,
    })
  }
}

watch(columnFilters, () => {
  setPagination({
    ...pagination.value,
    pageIndex: 0,
  })
})

watch(notifications, () => {
  fixPaginationAfterDelete()
})

const typesItems = computed<NotificationTypeItemOption[]>(() => {
  const column = grid.getColumn('types')
  if (!column) {
    return []
  }
  return [
    ...new Set(
      Array.from(column.getFacetedUniqueValues().keys())
        .flat()
        .map(id => ({
          label: t(`notification.${id}.title`),
          id,
        })),
    ),
  ]
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
            v-model="typesModel"
            class="w-48"
            color="neutral"
            size="xl"
            variant="subtle"
            :placeholder="$t('user.notifications.filter.byType')"
            multiple
            :items="typesItems"
            :ui="{
              content: 'w-auto',
            }"
          />

          <USwitch
            v-model="onlyStateUnread"
            variant="card"
            size="xl"
            :label="$t('user.notifications.filter.onlyUnread')"
          />
        </div>

        <UDropdownMenu
          v-if="!isEmpty"
          size="xl"
          :items="[
            {
              label: $t('user.notifications.action.readAll.title'),
              disabled: !hasUnreadNotifications,
              onSelect: () => readAllNotifications(),
            },
            {
              label: $t('user.notifications.action.deleteAll.title'),
              icon: 'crpg:close',
              color: 'error',
              onSelect: () => deleteAllNotifications(),
            },
          ]"
          :modal="false"
        >
          <UButton
            variant="outline"
            color="neutral"
            size="xl"
            icon="i-lucide-ellipsis-vertical"
          />
        </UDropdownMenu>
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

<script setup lang="ts">
import { useUsersNotifications } from '~/composables/user/use-user-notifications'

definePage({
  meta: {
    layout: 'default',
    roles: ['User', 'Moderator', 'Admin'],
  },
})

const {
  notifications,
  isLoading,
  isEmpty,
  hasUnreadNotifications,
  readNotification,
  readAllNotifications,
  deleteNotification,
  deleteAllNotifications,
} = useUsersNotifications()
</script>

<template>
  <div class="container">
    <div class="mx-auto max-w-2xl py-12">
      <h1 class="mb-14 text-center text-xl text-content-100">
        {{ $t('user.notifications.title') }}
      </h1>

      <div v-if="!isEmpty" class="mb-4 flex justify-end gap-4">
        <OButton
          :disabled="!hasUnreadNotifications"
          variant="transparent"
          outlined
          size="xs"
          label="Mark all as read"
          @click="readAllNotifications"
        />

        <ConfirmActionTooltip
          :confirm-label="$t('action.ok')"
          title="Are you sure you want to delete all notifications?"
          placement="bottom"
          @confirm="deleteAllNotifications"
        >
          <OButton
            variant="transparent"
            outlined
            size="xs"
            icon-left="close"
            label="Delete all"
          />
        </ConfirmActionTooltip>
      </div>

      <div class="flex flex-col flex-wrap gap-4">
        <OLoading :active="isLoading" :full-page="false" />

        <NotificationCard
          v-for="notification in notifications.notifications"
          :key="notification.id"
          :notification="notification"
          :dict="notifications.dict"
          @read="readNotification(notification.id)"
          @delete="deleteNotification(notification.id)"
        />

        <ResultNotFound v-if="!isLoading && isEmpty" message="Notifications not found..." />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useUsersNotifications } from '~/composables/user/use-user-notifications'
import { SomeRole } from '~/models/role'

definePageMeta({
  roles: SomeRole,
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
  <UContainer>
    <div class="mx-auto max-w-2xl py-12">
      <h1 class="mb-14 text-center text-xl text-content-100">
        {{ $t('user.notifications.title') }}
      </h1>

      <div v-if="!isEmpty" class="mb-4 flex justify-end gap-4">
        <UButton
          :disabled="!hasUnreadNotifications"
          variant="ghost"
          color="neutral"
          size="sm"
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
            size="sm"
            icon="crpg:close"
            :label="$t('user.notifications.action.deleteAll.title')"
          />
        </AppConfirmActionPopover>
      </div>

      <div class="flex flex-col flex-wrap gap-4">
        <UiLoading :active="isLoading" />

        <UserNotificationCard
          v-for="notification in notifications.notifications"
          :key="notification.id"
          :notification
          :dict="notifications.dict"
          @read="readNotification(notification.id)"
          @delete="deleteNotification(notification.id)"
        />

        <UiResultNotFound v-if="!isLoading && isEmpty" />
      </div>
    </div>
  </UContainer>
</template>

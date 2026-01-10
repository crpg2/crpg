<script setup lang="ts">
import type { MetadataDict } from '~/models/metadata'
import type { UserNotification } from '~/models/user'

import { useLocaleTimeAgo } from '~/composables/utils/use-locale-time-ago'
import { NOTIFICATION_STATE } from '~/models/notifications'

const { notification } = defineProps<{
  notification: UserNotification
  dict: MetadataDict
}>()

defineEmits<{
  read: []
  delete: []
}>()

const timeAgo = useLocaleTimeAgo(notification.createdAt)

const isUnread = computed(() => notification.state === NOTIFICATION_STATE.Unread)
</script>

<template>
  <UCard
    variant="subtle"
    :ui="{
      footer: 'flex items-end gap-4',
      body: 'relative',
    }"
  >
    <UiDataCell>
      <AppMetadataRender
        :keypath="`notification.${notification.type}.tpl`"
        :metadata="notification.metadata"
        :dict
        class="flex-1 pr-8"
      />
    </UiDataCell>

    <UTooltip v-if="isUnread" :text="$t('user.notifications.unreadNotification')">
      <UIcon
        name="crpg:item-rank-duotone"
        class="absolute top-3 right-3 z-10 size-4 text-notification outline-0 select-none"
      />
    </UTooltip>

    <template #footer>
      <UTooltip :text="$d(new Date(notification.createdAt), 'short')">
        <UiTextView variant="caption-xs">
          {{ timeAgo }}
        </UiTextView>
      </UTooltip>

      <div class="mr-0 ml-auto flex gap-3">
        <UButton
          v-if="isUnread"
          variant="ghost"
          color="neutral"
          size="xs"
          :label="$t('user.notifications.action.read.title')"
          @click="$emit('read')"
        />
        <UButton
          variant="ghost"
          color="error"
          size="xs"
          icon="crpg:close"
          :label="$t('user.notifications.action.delete.title')"
          @click="$emit('delete')"
        />
      </div>
    </template>
  </UCard>
</template>

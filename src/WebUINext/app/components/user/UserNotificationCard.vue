<script setup lang="ts">
import type { MetadataDict } from '~/models/metadata'
import type { UserNotification } from '~/models/user'

import { useLocaleTimeAgo } from '~/composables/utils/use-locale-time-ago'
import { NotificationState } from '~/models/notifications'

const { notification } = defineProps<{
  notification: UserNotification
  dict: MetadataDict
}>()

defineEmits<{
  read: []
  delete: []
}>()

const timeAgo = useLocaleTimeAgo(notification.createdAt)

const isUnread = computed(() => notification.state === NotificationState.Unread)
</script>

<template>
  <div class="relative flex items-start gap-5 rounded-lg bg-base-200 p-3 text-content-200">
    <div class="flex size-8 items-center justify-center gap-1.5 rounded-full bg-content-600">
      <UiSpriteSymbol
        name="logo"
        viewBox="0 0 162 124"
        class="w-3/4"
      />
    </div>

    <div class="flex-1 space-y-4">
      <AppMetadataRender
        :keypath="`notification.tpl.${notification.type}`"
        :metadata="notification.metadata"
        :dict
        class="pr-8"
      />

      <div class="flex items-end gap-4">
        <UTooltip :text="$d(new Date(notification.createdAt), 'short')">
          <UBadge size="xs" variant="soft" color="neutral" :label="timeAgo" />
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
            color="neutral"
            size="xs"
            icon="crpg:close"
            :label="$t('user.notifications.action.delete.title')"
            @click="$emit('delete')"
          />
        </div>
      </div>
    </div>

    <UTooltip v-if="isUnread" :text="$t('user.notifications.unreadNotification')">
      <UIcon
        name="crpg:item-rank-duotone"
        class="absolute top-3 right-3 z-10 size-4 outline-0 select-none"
        :style="{
          color: '#53bc96',
        }"
      />
    </UTooltip>
  </div>
</template>

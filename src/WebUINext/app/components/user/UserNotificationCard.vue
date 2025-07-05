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
  <UCard
    variant="subtle"
    :ui="{
      footer: 'flex items-end gap-4',
      body: 'relative flex items-start gap-4',
    }"
  >
    <div class="flex size-8 items-center justify-center gap-1.5 rounded-full bg-accented">
      <UiSpriteSymbol
        name="logo"
        class="w-3/4"
      />
    </div>

    <AppMetadataRender
      :keypath="`notification.tpl.${notification.type}`"
      :metadata="notification.metadata"
      :dict
      class="flex-1 pr-8"
    />

    <UTooltip v-if="isUnread" :text="$t('user.notifications.unreadNotification')">
      <UIcon
        name="crpg:item-rank-duotone"
        class="absolute top-3 right-3 z-10 size-4 outline-0 select-none"
        :style="{
          color: '#53bc96',
        }"
      />
    </UTooltip>

    <template #footer>
      <UTooltip :text="$d(new Date(notification.createdAt), 'short')">
        <UBadge size="sm" variant="soft" color="neutral" :label="timeAgo" />
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

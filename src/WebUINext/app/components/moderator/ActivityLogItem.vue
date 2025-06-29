<script setup lang="ts">
import type {
  ActivityLog,
  ActivityLogType,
} from '~/models/activity-logs'
import type { MetadataDict } from '~/models/metadata'
import type { UserPublic } from '~/models/user'

import { useLocaleTimeAgo } from '~/composables/utils/use-locale-time-ago'

const {
  user,
  activityLog,
  isSelfUser,
  dict,
} = defineProps<{
  activityLog: ActivityLog
  user: UserPublic
  dict: MetadataDict
  isSelfUser: boolean
}>()

const emit = defineEmits<{
  addType: [type: ActivityLogType]
  addUser: [user: number]
}>()

const timeAgo = useLocaleTimeAgo(activityLog.createdAt)
</script>

<template>
  <div
    class="inline-flex w-auto flex-col space-y-2 rounded-lg bg-base-200 p-4"
    :class="[isSelfUser ? 'self-start' : 'self-end']"
  >
    <div class="flex items-center gap-2">
      <NuxtLink :to="{ name: 'moderator-user-id-restrictions', params: { id: user.id } }">
        <UserMedia :user size="sm" />
      </NuxtLink>

      <div class="text-2xs text-muted">
        {{ $d(activityLog.createdAt, 'long') }} ({{ timeAgo }})
      </div>

      <UBadge
        variant="subtle"
        size="sm"
        color="neutral"
        :label="activityLog.type"
        @click="emit('addType', activityLog.type)"
      />
    </div>

    <AppMetadataRender
      :keypath="`activityLog.tpl.${activityLog.type}`"
      :metadata="activityLog.metadata"
      :dict
    >
      <template
        v-if="activityLog.metadata.targetUserId"
        #user="{ user: scopeUser }"
      >
        <div class="inline-flex items-center gap-1 align-middle">
          <NuxtLink
            :to="{ name: 'moderator-user-id-restrictions', params: { id: activityLog.metadata.targetUserId } }"
            class="inline-block hover:text-content-100"
            target="_blank"
          >
            <UserMedia :user="scopeUser" size="sm" />
          </NuxtLink>
          <UButton
            v-if="isSelfUser"
            size="xs"
            icon="crpg:plus"
            color="neutral"
            variant="ghost"
            @click="$emit('addUser', scopeUser.id)"
          />
        </div>
      </template>
    </AppMetadataRender>
  </div>
</template>

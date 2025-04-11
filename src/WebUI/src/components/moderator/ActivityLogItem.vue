<script setup lang="ts">
import type {
  ActivityLog,
  ActivityLogMetadataDicts,
  ActivityLogType,
} from '~/models/activity-logs'
import type { UserPublic } from '~/models/user'

import { useLocaleTimeAgo } from '~/composables/use-locale-time-ago'

const { user, activityLog, isSelfUser, dict } = defineProps<{
  activityLog: ActivityLog
  user: UserPublic
  dict: ActivityLogMetadataDicts
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
      <RouterLink
        :to="{ name: 'ModeratorUserIdRestrictions', params: { id: user.id } }"
        class="inline-block hover:text-content-100"
      >
        <UserMedia :user />
      </RouterLink>

      <div class="text-2xs text-content-300">
        {{ $d(activityLog.createdAt, 'long') }} ({{ timeAgo }})
      </div>

      <Tag
        class="ml-auto mr-0"
        variant="primary"
        :label="activityLog.type"
        @click="emit('addType', activityLog.type)"
      />
    </div>

    <MetadataRender
      :keypath="`activityLog.tpl.${activityLog.type}`"
      :activity-log="activityLog"
      :dict
    >
      <template #user="{ user: scopeUser }">
        <div
          class="inline-flex items-center gap-1 align-middle"
        >
          <RouterLink
            :to="{
              name: 'ModeratorUserIdRestrictions',
              params: { id: activityLog.metadata.targetUserId },
            }"
            class="inline-block hover:text-content-100"
            target="_blank"
          >
            <UserMedia :user="scopeUser" class=" text-content-100" />
          </RouterLink>
          <OButton
            v-if="isSelfUser"
            size="2xs"
            icon-left="add"
            rounded
            variant="secondary"
            @click="$emit('addUser', scopeUser.id)"
          />
        </div>
      </template>
    </MetadataRender>
  </div>
</template>

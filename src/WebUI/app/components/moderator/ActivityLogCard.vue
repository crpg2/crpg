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
  <UCard variant="outline">
    <template #header>
      <UiDataCell>
        <template #leftContent>
          <NuxtLink :to="{ name: 'moderator-user-id-restrictions', params: { id: user.id } }">
            <UserMedia :user size="md" />
          </NuxtLink>
        </template>

        <div class="text-xs text-muted">
          {{ $d(activityLog.createdAt, 'long') }} ({{ timeAgo }})
        </div>

        <template #rightContent>
          <UBadge
            variant="subtle"
            color="neutral"
            :label="activityLog.type"
            @click="emit('addType', activityLog.type)"
          />
        </template>
      </UiDataCell>
    </template>

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
            target="_blank"
          >
            <UserMedia :user="scopeUser" size="md" />
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
  </UCard>
</template>

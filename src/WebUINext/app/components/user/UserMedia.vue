<script setup lang="ts">
import type { UserPublic } from '~/models/user'

const {
  user,
  hiddenClan = false,
  hiddenPlatform = false,
  hiddenTitle = false,
  isSelf = false,
  size = 'sm',
} = defineProps<{
  user: UserPublic
  isSelf?: boolean
  hiddenPlatform?: boolean
  hiddenTitle?: boolean
  hiddenClan?: boolean
  size?: 'sm' | 'xl'
}>()
</script>

<template>
  <div class="inline-flex items-center gap-1 align-middle">
    <UAvatar
      :src="user.avatar"
      :size
      :alt="user.name"
      :class="[{ 'ring-2 ring-status-success': isSelf }]"
    />

    <UserClan
      v-if="!hiddenClan && user.clanMembership"
      :clan="user.clanMembership.clan"
      :clan-role="user.clanMembership.role"
    />

    <div
      v-if="!hiddenTitle"
      class="max-w-52 truncate"
      :title="user.name"
    >
      {{ user.name }}
      <template v-if="isSelf">
        ({{ $t('you') }})
      </template>
    </div>

    <UserPlatform
      v-if="!hiddenPlatform"
      :platform="user.platform"
      :platform-user-id="user.platformUserId"
      :user-name="user.name"
    />
  </div>
</template>
